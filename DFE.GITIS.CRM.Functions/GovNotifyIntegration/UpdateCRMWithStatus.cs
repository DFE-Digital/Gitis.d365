using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DFE.GITIS.CRM.Functions.GovNotifyIntegration
{
    public class UpdateCRMWithStatus
    {
        private HttpClient _httpClient;
        private static readonly string _clientId = Environment.GetEnvironmentVariable("D365_CLIENT_ID");
        private static readonly string _secret = Environment.GetEnvironmentVariable("D365_SECRET");
        private static AuthenticationResult _authenticationResult;

        public UpdateCRMWithStatus(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [FunctionName("UpdateCRMWithStatus")]
        [FixedDelayRetry(120, "00:01:00")]
        [Disable("AzureWebJobs.UpdateCRMWithStatus.Disabled")]
        public void Run([ServiceBusTrigger("smsqueue-crm", Connection = "RootManageSharedAccessKey_SERVICEBUS")] Message myQueueItem, ILogger log
            , MessageReceiver messageReceiver)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(myQueueItem.Body));

                log.LogInformation($"SMS record id:{data.Id}");
                log.LogInformation($"Notify Status:{data.SmsDeliveryStatus}");

                var smsRecordId = new Guid(data.Id.ToString());
                var smsStatus = data.SmsDeliveryStatus.ToString().ToLower();

                if(smsStatus != "delivered" && 
                   smsStatus != "permanent-failure" && 
                   smsStatus != "temporary-failure" && 
                   smsStatus != "technical-failure" &&
                   smsStatus != "sent" &&
                   smsStatus != "sending" &&
                   smsStatus != "pending")
                {
                    log.LogInformation($"Unsupported Status {smsStatus}");
                    return;
                }

                var crmConnection = GetConnection(log);

                var patchPayload = JsonConvert.SerializeObject(new
                {
                    dfe_smsdeliverystatus = 
                    smsStatus == "delivered" ? SMSDeliveryStatus.Delivered : 
                    smsStatus == "permanent-failure" ? SMSDeliveryStatus.FailedPermanently : 
                    smsStatus == "temporary-failure" ? SMSDeliveryStatus.FailedTemporary :
                    smsStatus == "technical-failure" ? SMSDeliveryStatus.FailedTechnical :
                    smsStatus == "sent" ? SMSDeliveryStatus.SentInternational :
                    smsStatus == "sending" ? SMSDeliveryStatus.Sending : SMSDeliveryStatus.Pending

                });

                var content = new StringContent(patchPayload, Encoding.UTF8, "application/json");
                var patchResponse = crmConnection.PatchAsync($"dfe_smses({smsRecordId})", content).Result;
                
                if(patchResponse.IsSuccessStatusCode)
                {
                    log.LogInformation($"Status:Updated");
                }
                else
                {
                    throw new Exception($"Failed patch:{patchResponse.StatusCode} , {patchResponse.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }

        public HttpClient GetConnection(ILogger log)
        {
            if (_authenticationResult == null || DateTime.UtcNow.AddMinutes(1) > _authenticationResult.ExpiresOn)
            {
                var clientCrendential = new ClientCredential(_clientId, _secret);

                var authContext =
                new AuthenticationContext("https://login.microsoftonline.com/fad277c9-c60a-4da1-b5f3-b3b8b34a82f9/oauth2/v2.0/token");

                var authResult =  authContext.AcquireTokenAsync(Startup.BASE_ADDRESS, clientCrendential).Result;

                if (authResult == null)
                {
                    log.LogInformation($"Cannot get token.");
                    throw new Exception("Cannot aquire login token");
                }

                _authenticationResult = authResult;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationResult.AccessToken);
            return _httpClient;
        }

    }
}
