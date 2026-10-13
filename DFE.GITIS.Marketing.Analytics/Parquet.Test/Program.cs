// See https://aka.ms/new-console-template for more information
using Google.Apis.Auth.OAuth2;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Parquet;
using Parquet.Data;
using Parquet.Schema;
using System;
using System.IO;

namespace MyConsoleApp
{
    class Program
    {
        // Entry point of the application
        static void Main(string[] args)
        {

            var key = "-----BEGIN PRIVATE KEY-----MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCZt3x2cVrTwbW1fgtjJhQahjkxXW5w05EoLvWU4QpT3PvoueYSPh9xJAmapONYq7TlwnbNttG71U/rflLDBP9NbgE7owoRc3NNlCD5vxKVOJUk88jMwqREPvPgbMV2e7vcDLAHgnAS8ngyGe0FVbNg5K8pySLHe+Km13yeTcXMMjWd+fpnfN3GcaYG/eajW0uAoTZl53DU/x+v6mRdouHIH3lFDWVf5PDTJl+T5vWX4c5uNtYGa5K1cdYQ2wZ6TF/mi+6X9tA4Wat4du7ihtP/10he/Pu13AsUj4XX0shVjptzQwqaUBxSVI9aQVjteilDaRST69QE9XEPdg1ZvoD5AgMBAAECggEAFau6jkWDf+Bf/7WaDAutleJfucfpmVpvrRyXCWWDO+N8WDOtGIai5CPSfszJiGdcr2+aVCd2q2ZynUlH3a/yAOEwp08K96mGaI9+PqIOD/TULKDPKZ7fOF0vq0S/vz5zyPK49mC7RXs4MB1blFRKtQRIdY73qkFFk4Pu+6+3N7GR0Y1FBhp3tiWaxtmzNCliy2U4IJwnRPgRtDdPdrZVYF7hiytbVhfDzZriZmPYzfek6rFF0Wi1O0hsVxuEX5TrjneFmuzUymxAl5/HfpkSWlzUachb/puHRHhzRW9noyX+UINJuddNQMrNJxpCy61uISCqtH2/uZhQf1ZJAGw4SQKBgQDIw7Rj4otV2LnL9f89kSYgPabzH/O3MpSRcC9pqpOvZnRnran7NSsPoajfy66lde17Nx1Ov/qV7pIrBMG76vK1TUDZ95F+3dc3+gPaJaPbgd3eD2/zmRFTOV7TA8QMUi5z0pBSIN3tKD51TywDJ/kJlFhshz4WFm8nc1/9joEcMwKBgQDEAhqNInKd391l1Ft2BN+LXfIquqFc6X7RqbYvfbfzx5tQj2btxmAg2JEPgy8npc/RoGbkdvh1v6Rwj2sd9QnRhG1hcCwjGjxliGCF2TAn/6Zv6MCvVDOtmrtMm93BK4Gosb127H8e/79g3dy9jCls3KGb4anzADNwbbNNZWPCIwKBgHDbmwdfbaZXi7J9WftZIg6TucsIVi/QzjWn0+9ZhWXfeWZi+KAPtmI2JUGRNV8sGNDgHaiMQSiRkTd/mAy0ObXzdJZTFkY0HO0Jraqu6BrYOUnAcdWFYXjgUk7a2hV3CUfwwVJ5yWes7NdzJzGrAdMBwmdX623tNJT/xaE6v5eDAoGAZSt8zuo/5NMe8Kd6ieEcDVBxZbKtBUsUBV4mxj6Ky051EVDjXMlu5qVBwpMArOM98EJtuXDa79P6KYstS+olpjpHRuriLUAmenW49H1VCOBA5TRq+FRq1CmwkWtGO6WcKBmN5KtPNg3llzcM790/qIv88nu4/89Tva0xxjoWEqcCgYAXlJwvW0k3CykPo6KizGZgEE0xOiP4xGbBb9yFZ9Z9gLJk/oELIrzdGghvKeBzhdZPS7mmxDZIFmcEcEqDMM5mS1Bo0zamHhSEmENK2UOmz5/GGfMQQ59EmATArApM6DG0jLLzBenwRlhAGwzNJUvzyRRQpFxUdVY7ib6FvoXtEw==-----END PRIVATE KEY-----".Replace("\\n", "\n");
            var email = "crm2bq-production1@get-into-teaching.iam.gserviceaccount.com";
            var token = "https://oauth2.googleapis.com/token";
            var datasetId = "crm_marketing_data_dev";
            var sa = new ServiceAccountCredential(
       new ServiceAccountCredential.Initializer(email, token)
       .FromPrivateKey(key));

            var cred = GoogleCredential.FromServiceAccountCredential(sa);
            var _bqClient = BigQueryClient.Create("get-into-teaching", cred);
            using var pReader = ParquetReader.CreateAsync("./part-00000-cfd2d7e4-070a-48b1-9e90-dd74dc53e8c8.c000.zstd.parquet");

            var count = pReader.Result.RowGroupCount;

            var dataFields = pReader.Result.Schema.GetDataFields();

            var tableId = GetTableIdFromInteractionTypeAsync("./part-00000-cfd2d7e4-070a-48b1-9e90-dd74dc53e8c8.c000.zstd.parquet");

            EnsureTableExistsAsync(_bqClient, datasetId, tableId, dataFields);


            using var pReader2 = ParquetReader.CreateAsync("./part-00000-cfd2d7e4-070a-48b1-9e90-dd74dc53e8c8.c000.zstd.parquet");

            const int batchSize = 1000;
            for (int rg = 0; rg < pReader2.Result.RowGroupCount; rg++)
            {
                using var rgr = pReader2.Result.OpenRowGroupReader(rg);

                // Read all columns for this row group
                var cols = new DataColumn[dataFields.Length];
                for (int i = 0; i < dataFields.Length; i++)
                {
                    cols[i] = rgr.ReadColumnAsync(dataFields[i]).Result;
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
                        InsertBatchAsync(_bqClient, datasetId, tableId, batch);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    InsertBatchAsync(_bqClient, datasetId, tableId, batch);
                }
            }
        }

        private static string GetTableIdFromInteractionTypeAsync(string name)
        {
          

            using var reader = ParquetReader.CreateAsync(name).Result;
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
            var column = rgReader.ReadColumnAsync(interactionField).Result;

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

            return rawName;
        }


        private static void EnsureTableExistsAsync(BigQueryClient client, string datasetId, string tableId, DataField[] dataFields)
        {
            //Create dataset if missing
            try
                {
                    client.GetDatasetAsync(datasetId);
                }
                catch
                {
                    client.CreateDatasetAsync(datasetId);
                }

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
                client.CreateTableAsync(datasetId, tableId, schemaBuilder.Build());
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

            client.PatchTableAsync(datasetId, tableId, patch);
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
                return BigQueryNumeric.FromDecimal(d, LossOfPrecisionHandling.Truncate);
            }

            if (t == typeof(int) || t == typeof(int?))
            {
                // Upcast to Int64 for BigQuery
                return Convert.ToInt64(value);
            }

            // Other primitives (string, bool, long, float, double, byte[]) can pass through
            return value;
        }

        private static void InsertBatchAsync(
            BigQueryClient client,
            string datasetId,
            string tableId,
            List<BigQueryInsertRow> batch)
        {
            if (batch.Count == 0) return;

            var result = client.InsertRows(datasetId, tableId, batch);
            if (result != null && result.Errors.Any())
            {
                // Log first error for visibility
                var firstErr = result.Errors.First();
                Console.WriteLine("BigQuery insert had errors. First error: {Error}", firstErr.ToString());

            }
        }
    }
}