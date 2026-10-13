using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(sp =>
{
    var projectId = Environment.GetEnvironmentVariable("GcpProjectId");
    var key = (Environment.GetEnvironmentVariable("googlecredentials:private_key") ?? "").Replace("\\n", "\n");
    var email = Environment.GetEnvironmentVariable("googlecredentials:client_email");
    var token = Environment.GetEnvironmentVariable("googlecredentials:token_uri") ?? "https://oauth2.googleapis.com/token";

    var sa = new ServiceAccountCredential(
        new ServiceAccountCredential.Initializer(email, token)
        .FromPrivateKey(key));

    var cred = GoogleCredential.FromServiceAccountCredential(sa);
    return BigQueryClient.Create(projectId, cred);
});

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
