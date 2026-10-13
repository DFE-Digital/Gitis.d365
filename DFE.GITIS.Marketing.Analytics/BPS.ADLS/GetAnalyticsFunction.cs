using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace BPS
{
    public class GetAnalyticsFunction
    {
        private readonly ILogger<GetAnalyticsFunction> _logger;
        private readonly string connectionString = Environment.GetEnvironmentVariable("blobStorageString");
        private readonly string containerName = Environment.GetEnvironmentVariable("containerName");
        private static readonly string dataverseUrl = Environment.GetEnvironmentVariable("dataverseUrl");

        public GetAnalyticsFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetAnalyticsFunction>();
        }

        [Function("GetAnalyticsFunction")]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            const bool useBps = true;

            // Number of entities (interaction types) being processed in parallel and number of files being downloaded in parallel for a particular entity.
            // For the entire app, the maximum number of possible parallel downloads is (maxParallelEntityProcessing * maxParallelDownloadsWithinEntity).
            const int maxParallelEntityProcessing = 1;
            const int maxParallelDownloadsWithinEntity = 4;

            _logger.LogInformation("Utility for fetching all CI-J analytics data from MDL for a specific Dataverse organization using the Blob proxy service.");

            _logger.LogInformation("Getting BPS URL and token from Dataverse...");
            var dataverseClient = new DataverseClient(dataverseUrl,_logger);
            var uri = await dataverseClient.GetMdlAccessUri(useBps);

            _logger.LogInformation("URL and token retrieved successfully. Starting MDL data download.");
            var mdlClient = new DataLakeDirectoryClient(uri);

            var entities = new List<EntityType>();
            var manifestFile = mdlClient.GetFileClient("default.manifest.cdm.json");
            using (var manifestStream = manifestFile.OpenRead())
            using (var manifestDoc = JsonDocument.Parse(manifestStream))
            {
                var entitiesElement = manifestDoc.RootElement.GetProperty("entities");

                _logger.LogInformation("Found the following entities:");
                foreach (var entity in entitiesElement.EnumerateArray())
                {
                    var entityInfo = ReadEntity(entity);
                    if (entityInfo != null)
                    {
                        entities.Add(entityInfo);
                    }
                }
            }

            var entitiesToDownload = entities.ToArray(); // Filter entity types here, if needed

            var stopwatch = Stopwatch.StartNew();
            var downloadedSize = 0L;
            object downloadedSizeLock = new object();
            await Parallel.ForEachAsync(
                entitiesToDownload,
                new ParallelOptions { MaxDegreeOfParallelism = maxParallelEntityProcessing },
                async (entity, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    _logger.LogInformation($"Processing entity {entity.Name}");
                    var currentDownloadSize = await DownloadData(mdlClient, entity, maxParallelDownloadsWithinEntity);
                    lock (downloadedSizeLock)
                    {
                        downloadedSize += currentDownloadSize;
                    }
                });

            stopwatch.Stop();

            _logger.LogInformation("Completed successfully.");
            _logger.LogInformation($"Total download: {downloadedSize / 1024}kB in {stopwatch.ElapsedMilliseconds}ms.");

        }

        public async Task<long> DownloadData(DataLakeDirectoryClient client, EntityType entityType, int maxDegreeOfParallelism = 1)
        {
            var stopwatch = Stopwatch.StartNew();
            var downloadedSize = 0L;
            var downloadedSizeLock = new object();

            var directoryClient = client.GetSubDirectoryClient(entityType.Location);
            if (!directoryClient.ExistsBpsFriendly())
            {
                //_logger.LogInformation(($"Entity directory {entityType.Location} was not found.");
                return 0L;
            }

            // Copy only files last modified in "yesterday" (UTC), recurse into partitions
            var utcEnd = DateTimeOffset.UtcNow.Date;         // today 00:00 UTC
            var utcStart = utcEnd.AddDays(-1);                 // yesterday 00:00 UTC

            var paths = directoryClient
                .GetPaths(recursive: true)
                .Where(p => p.IsDirectory != true
                         && p.Name.EndsWith(".parquet", StringComparison.OrdinalIgnoreCase)
                         && !p.Name.Contains("/_delta_log/", StringComparison.OrdinalIgnoreCase)
                         && !p.Name.EndsWith(".checkpoint.parquet", StringComparison.OrdinalIgnoreCase)
                         && !p.Name.Contains("/_temporary/", StringComparison.OrdinalIgnoreCase)
                         && (p.ContentLength ?? 0) > 0
                         && p.LastModified >= utcStart
                         && p.LastModified < utcEnd);


            //var localPath = Path.Combine(targetDirectory, path.Name);
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Get the container client
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await Parallel.ForEachAsync(
                paths,
                new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                async (path, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    // Create the container if it doesn't exist
                    await containerClient.CreateIfNotExistsAsync();

                    _logger.LogInformation($"Path: {path.Name}");
                    // Get the blob client
                    var blobClient = containerClient.GetBlobClient(path.Name);

                    if (!blobClient.Exists())
                    {
                        _logger.LogInformation($"Blob does not exist..");


                        var fileClient = directoryClient.GetFileClient(path);

                        var uploadFileStream = await fileClient.OpenReadAsync();

                        await blobClient.UploadAsync(uploadFileStream);
                        uploadFileStream.Close();
                        lock (downloadedSizeLock)
                        {
                            downloadedSize += path.ContentLength!.Value;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Skipped..");
                    }

                });

            stopwatch.Stop();
            _logger.LogInformation($"Downloaded {downloadedSize / 1024}kB in {stopwatch.ElapsedMilliseconds}ms.");

            return downloadedSize;
        }

        private EntityType? ReadEntity(JsonElement entityElement)
        {
            try
            {
                string name, location;
                if (!entityElement.TryGetProperty("entityName", out JsonElement nameElement) || string.IsNullOrEmpty(nameElement.GetString()))
                {
                    _logger.LogInformation("The property 'entityName' is not present or empty.");
                    return null;
                }

                name = nameElement.GetString()!;

                if (!entityElement.TryGetProperty("dataPartitions", out JsonElement dataPartitionsElement))
                {
                    _logger.LogInformation("The property 'dataPartitions' is not present.");
                    return null;
                }

                try
                {
                    var dataPartitionElement = dataPartitionsElement.EnumerateArray().First();
                    if (!dataPartitionElement.TryGetProperty("location", out JsonElement locationElement) || string.IsNullOrEmpty(locationElement.GetString()))
                    {
                        _logger.LogInformation("The property 'location' is not present or empty.");
                        return null;
                    }

                    location = locationElement.GetString()!;
                }
                catch (InvalidOperationException)
                {
                    _logger.LogInformation("The property 'dataPartitions' is not an array or it's empty.");
                    return null;
                }

                _logger.LogInformation($"Found entity '{name}' located in '{location}'.");
                return new EntityType(name, location);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error while reading entity. The entity will be skipped.");
                _logger.LogInformation(ex.Message);
                return null;
            }
        }

    }
}
