namespace BPS
{
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Web;
    using Microsoft.Extensions.Logging;
    using Microsoft.Identity.Client;

    internal class DataverseClient
    {
        public DataverseClient(string orgApiUrl, ILogger logger)
        {
            orgUrl = orgApiUrl;
            _logger = logger;
        }

        private static readonly string clientId = Environment.GetEnvironmentVariable("clientId");
        private static readonly string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
        private static readonly string tenantId = Environment.GetEnvironmentVariable("tenantId");
        
        public string orgUrl { get; set; }
        private readonly ILogger _logger;

        public async Task<Uri> GetMdlAccessUri(bool useBps = false)
        {
            const int tokenValidityInMinutes = 60; // Maximum value allowed by MDL is 60 minutes.            

            var scopes = new[] { $"{orgUrl}/.default" };

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var token = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            var client = new HttpClient
            {
                BaseAddress = new Uri($"{orgUrl}/api/data/v9.2/"),
                Timeout = TimeSpan.FromMinutes(2)
            };


            // Default headers for each Web API call.
            // See https://docs.microsoft.com/powerapps/developer/data-platform/webapi/compose-http-requests-handle-errors#http-headers
            HttpRequestHeaders headers = client.DefaultRequestHeaders;
            headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            headers.Add("OData-MaxVersion", "4.0");
            headers.Add("OData-Version", "4.0");
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            

            var response = await client.GetAsync("datalakefolders?$filter=name%20eq%20%27Customer%20Insights%20Journeys%27&$select=path,name");
            response.EnsureSuccessStatusCode();
            string dataLakeFolderPath;
            using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                JsonElement root = doc.RootElement;
                dataLakeFolderPath = root.GetProperty("value").EnumerateArray().First().GetProperty("path").GetString()!;
                _logger.LogInformation($"Found data lake folder path: {dataLakeFolderPath}");
            }

            response = await client.GetAsync("RetrieveAnalyticsStoreDetails");
            response.EnsureSuccessStatusCode();
            string containerBaseUrl;
            using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                JsonElement root = doc.RootElement;
                containerBaseUrl = root.GetProperty("AnalyticsStoreDetails").GetProperty("Endpoint").GetString()!;
                _logger.LogInformation($"Found base container URL: {containerBaseUrl}");
            }

            var containerUrl = $"{containerBaseUrl}/{dataLakeFolderPath}";
            response = await client.GetAsync($"RetrieveAnalyticsStoreAccess(Url=@p1,ResourceType='Folder',Permissions='Read,List',SasTokenValidityInMinutes={tokenValidityInMinutes},UseBlobProxy={useBps.ToString().ToLowerInvariant()})?@p1={HttpUtility.UrlEncode("'" + containerUrl + "'")}");
            response.EnsureSuccessStatusCode();
            using (JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                JsonElement root = doc.RootElement;
                var mdlToken = root.GetProperty("SASToken").GetString()!;

                if (useBps)
                {
                    return new Uri(mdlToken);
                }
                else
                {
                    return new Uri(new Uri(containerUrl), mdlToken);
                }
            }
        }
    }
}
