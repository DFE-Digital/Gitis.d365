using Google.Apis.Auth.OAuth2;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BPS.ADLS
{
    public class AnalyticsDataReader
    {
        private readonly ILogger<AnalyticsDataReader> _logger;
        //private static readonly string gtype = Environment.GetEnvironmentVariable("googlecredentials:type");
        //private static readonly string gproject_id = Environment.GetEnvironmentVariable("googlecredentials:project_id");
        //private static readonly string gprivate_key_id = Environment.GetEnvironmentVariable("googlecredentials:private_key_id");
        //private static readonly string gprivate_key = Environment.GetEnvironmentVariable("googlecredentials:private_key");
        //private static readonly string gclient_email = Environment.GetEnvironmentVariable("googlecredentials:client_email");
        //private static readonly string gclient_id = Environment.GetEnvironmentVariable("googlecredentials:client_id");
        ////private static readonly string gauth_uri = Environment.GetEnvironmentVariable("googlecredentials:auth_uri");
        //private static readonly string gtoken_uri = Environment.GetEnvironmentVariable("googlecredentials:token_uri");

        private static readonly string projectId = Environment.GetEnvironmentVariable("GcpProjectId");
        private static readonly string datasetId = Environment.GetEnvironmentVariable("GcpDatasetId");

        private readonly BigQueryClient _bqClient;

        public AnalyticsDataReader(BigQueryClient bqClient, ILogger<AnalyticsDataReader> logger)
        {
            _logger = logger;            
            _bqClient = bqClient;            
        }

        [Function(nameof(AnalyticsDataReader))]
        public async Task Run(
            [BlobTrigger("analytics", Connection = "StorageConnection")] Stream stream,
            string name)
        {
            try
            {

                // --- 2) Derive table name from blob folder (parent directory of the file) ---
                // e.g. analytics/folderA/myfile.parquet => tableName = "folderA"

                _logger.LogInformation("File: {}", name);
                var tableId = await GetTableIdFromInteractionTypeAsync(stream,_logger);

                _logger.LogInformation("Starting AnalyticsDataReader for blob '{Name}'. Target BQ {Project}.{Dataset}.{Table}",
                    name, projectId, datasetId, tableId);
               

                // --- 3) Open Parquet & get schema ---
                using var pReader = await ParquetReader.CreateAsync(stream);
                var dataFields = pReader.Schema.GetDataFields();

                // --- 4) Ensure BigQuery table exists with (at least) these columns ---
                //var bqClient = await BigQueryClient.CreateAsync(projectId);
                EnsureTableExistsAsync(_bqClient, datasetId, tableId, dataFields);

                // --- 5) Read rows and stream-insert into BigQuery ---
                //using var pReader2 = await ParquetReader.CreateAsync(stream);

                const int batchSize = 1000;
                for (int rg = 0; rg < pReader.RowGroupCount; rg++)
                {
                    using var rgr = pReader.OpenRowGroupReader(rg);

                    // Read all columns for this row group
                    var cols = new DataColumn[dataFields.Length];
                    for (int i = 0; i < dataFields.Length; i++)
                    {
                        cols[i] = await rgr.ReadColumnAsync(dataFields[i]);
                    }

                    long rowCount = rgr.RowCount;
                    var batch = new List<BigQueryInsertRow>(batchSize);

                    for (int r = 0; r < rowCount; r++)
                    {
                        var row = new BigQueryInsertRow();
                        for (int c = 0; c < dataFields.Length; c++)
                        {
                            var field = dataFields[c];
                            var value = GetValue(cols[c].Data, r);

                            // Convert to BQ-friendly object
                            row[field.Name] = ToBigQueryValue(field, value);
                        }

                        batch.Add(row);

                        if (batch.Count >= batchSize)
                        {
                            await InsertBatchAsync(_bqClient, datasetId, tableId, batch, _logger);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await InsertBatchAsync(_bqClient, datasetId, tableId, batch, _logger);
                    }
                }

                _logger.LogInformation("Completed insert into {Project}.{Dataset}.{Table} for blob '{Name}'",
                    projectId, datasetId, tableId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyticsDataReader failed.");
                throw;
            }
        }

        private static async Task<string> GetTableIdFromInteractionTypeAsync(Stream parquetStream, ILogger logger)
        {
            // Ensure we're at the start of the stream
            if (parquetStream.CanSeek)
            {
                parquetStream.Position = 0;
            }

            using var reader = await ParquetReader.CreateAsync(parquetStream);
            var dataFields = reader.Schema.GetDataFields();

            // Find the InteractionType column (case-insensitive)
            var interactionField = dataFields
                .FirstOrDefault(f => string.Equals(f.Name, "InteractionType", StringComparison.OrdinalIgnoreCase));

            if (interactionField == null)
            {
                throw new InvalidOperationException("Parquet file does not contain an 'InteractionType' column.");
            }

            // Read only the first row group
            using var rgReader = reader.OpenRowGroupReader(0);
            var column = await rgReader.ReadColumnAsync(interactionField);

            var data = column.Data; // this is an Array
            string? rawName = null;

            // Get first non-null / non-empty value as table name
            for (int i = 0; i < data.Length && rawName == null; i++)
            {
                var v = data.GetValue(i)?.ToString();
                if (!string.IsNullOrWhiteSpace(v))
                {
                    rawName = v;
                }
            }

            if (string.IsNullOrWhiteSpace(rawName))
            {
                throw new InvalidOperationException("InteractionType column is empty; cannot derive table name.");
            }

            return rawName;
        }



        private static void EnsureTableExistsAsync(BigQueryClient client, string datasetId, string tableId, DataField[] dataFields)
        {
            // Create dataset if missing
            try { var dataset = client.GetDatasetAsync(datasetId); }
            catch { var isDatasetCreated = client.CreateDatasetAsync(datasetId).Result; }

            BigQueryTable table = null;
            try
            {
                table = client.GetTableAsync(datasetId, tableId).Result;
            }
            catch
            {
                // Table missing — create with full schema
                var schemaBuilder = new TableSchemaBuilder();
                foreach (var f in dataFields)
                {
                    var (bqType, _) = MapParquetFieldToBigQuery(f);
                    schemaBuilder.Add(f.Name, bqType);
                }
                var tableCreated = client.CreateTableAsync(datasetId, tableId, schemaBuilder.Build()).Result;
                return;
            }

            // Table exists — check for new fields and evolve schema
            var existingFields = table.Schema.Fields.ToList();
            var existingNames = new HashSet<string>(
                existingFields.Select(x => x.Name),
                StringComparer.OrdinalIgnoreCase); // BQ column names are case-insensitive

            var newFields = dataFields
                .Where(f => !existingNames.Contains(f.Name))
                .ToList();

            if (newFields.Count == 0)
                return;

            foreach (var f in newFields)
            {
                var (bqType, _) = MapParquetFieldToBigQuery(f);
                existingFields.Add(new TableFieldSchema
                {
                    Name = f.Name,
                    Type = "STRING",
                    Mode = "NULLABLE" // new columns must be NULLABLE (or REPEATED)
                });
            }

            var patch = new Table
            {
                Schema = new TableSchema { Fields = existingFields }
            };

            var isPatched = client.PatchTableAsync(datasetId, tableId, patch).Result;
        }

        private static (BigQueryDbType Type, bool Nullable) MapParquetFieldToBigQuery(DataField f)
        {
            // Basic mapping for common Parquet primitives
            var nullable = f.HasNulls;

            // Parquet.Net type info is on f.ClrNullableIfHasNullsType / f.ClrType
            var t = f.ClrNullableIfHasNullsType ?? f.ClrType;

            if (t == typeof(string)) return (BigQueryDbType.String, true);
            if (t == typeof(bool) || t == typeof(bool?)) return (BigQueryDbType.Bool, nullable);
            if (t == typeof(int) || t == typeof(int?)) return (BigQueryDbType.Int64, nullable);   // BQ has no INT32; use INT64
            if (t == typeof(long) || t == typeof(long?)) return (BigQueryDbType.Int64, nullable);
            if (t == typeof(float) || t == typeof(float?)) return (BigQueryDbType.Float64, nullable);
            if (t == typeof(double) || t == typeof(double?)) return (BigQueryDbType.Float64, nullable);
            if (t == typeof(decimal) || t == typeof(decimal?)) return (BigQueryDbType.Numeric, nullable);
            if (t == typeof(DateTime) || t == typeof(DateTime?)) return (BigQueryDbType.Timestamp, nullable);
            if (t == typeof(byte[])) return (BigQueryDbType.Bytes, nullable);

            // Fallback to STRING for anything unexpected
            return (BigQueryDbType.String, true);
        }

        private static object GetValue(Array data, int index)
        {
            // Handles nullability; Parquet.Net returns arrays with nulls where appropriate
            return data.GetValue(index);
        }

        private static object ToBigQueryValue(DataField field, object value)
        {
            if (value is null) return null;

            var t = field.ClrNullableIfHasNullsType ?? field.ClrType;

            if (t == typeof(DateTime) || t == typeof(DateTime?))
            {
                // Ensure UTC for TIMESTAMP
                var dt = (DateTime)value;
                return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }

            if (t == typeof(decimal) || t == typeof(decimal?))
            {
                // Map to BigQuery NUMERIC precisely
                var d = (decimal)value;
                return BigQueryNumeric.FromDecimal(d,LossOfPrecisionHandling.Truncate);
            }

            if (t == typeof(int) || t == typeof(int?))
            {
                // Upcast to Int64 for BigQuery
                return Convert.ToInt64(value);
            }

            // Other primitives (string, bool, long, float, double, byte[]) can pass through
            return value;
        }

        private static async Task InsertBatchAsync(
            BigQueryClient client,
            string datasetId,
            string tableId,
            List<BigQueryInsertRow> batch,
            ILogger logger)
        {
            if (batch.Count == 0) return;

            var result = await client.InsertRowsAsync(datasetId, tableId, batch);
            if (result != null && result.Errors.Any())
            {
                // Log first error for visibility
                var firstErr = result.Errors.First();
                logger.LogError("BigQuery insert had errors. First error: {Error}", firstErr.ToString());
                
            }
        }
    }
}
