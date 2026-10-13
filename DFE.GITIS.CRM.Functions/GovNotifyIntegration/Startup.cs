using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

[assembly: FunctionsStartup(typeof(DFE.GITIS.CRM.Functions.GovNotifyIntegration.Startup))]

namespace DFE.GITIS.CRM.Functions.GovNotifyIntegration
{
    public class Startup : FunctionsStartup
    {
        public static readonly string BASE_ADDRESS = Environment.GetEnvironmentVariable("D365_BASE_ADDRESS");
        private static readonly string _apiEndpoint = Environment.GetEnvironmentVariable("D365_API_ENDPOINT");
        private static readonly string _serviceBusEndpoint = Environment.GetEnvironmentVariable("RootManageSharedAccessKey_SERVICEBUS");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri($"{BASE_ADDRESS}{_apiEndpoint}");
            builder.Services.AddSingleton(httpClient);

            var queueClient = new ServiceBusClient(_serviceBusEndpoint);
            builder.Services.AddSingleton(queueClient);

        }
    }
}
