using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.CRM.Functions.GovNotifyIntegration
{
    public class UpdateCRMWithStatus
    {
        private static readonly string _clientId = Environment.GetEnvironmentVariable("D365_CLIENT_ID");
        private static readonly string _secret = Environment.GetEnvironmentVariable("D365_SECRET");
        public static readonly string BASE_ADDRESS = Environment.GetEnvironmentVariable("D365_BASE_ADDRESS");
        //private static readonly string _apiEndpoint = Environment.GetEnvironmentVariable("D365_API_ENDPOINT");
        //private static readonly string _serviceBusEndpoint = Environment.GetEnvironmentVariable("RootManageSharedAccessKey_SERVICEBUS");
        
        private readonly ILogger<UpdateCRMWithStatus> _logger;

        public UpdateCRMWithStatus(ILogger<UpdateCRMWithStatus> logger)
        {
            _logger = logger;
        }

        [Function(nameof(UpdateCRMWithStatus))]
        public async Task Run([ServiceBusTrigger("smsqueue-crm", Connection = "RootManageSharedAccessKey_SERVICEBUS")] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(message.Body));

                _logger.LogInformation($"SMS record id:{data.Id}");
                _logger.LogInformation($"Notify Status:{data.SmsDeliveryStatus}");

                var smsRecordId = new Guid(data.Id.ToString());
                var smsStatus = data.SmsDeliveryStatus.ToString().ToLower();

                if (smsStatus != "delivered" &&
                   smsStatus != "permanent-failure" &&
                   smsStatus != "temporary-failure" &&
                   smsStatus != "technical-failure" &&
                   smsStatus != "sent" &&
                   smsStatus != "sending" &&
                   smsStatus != "pending")
                {
                    _logger.LogInformation($"Unsupported Status {smsStatus}");
                    return;
                }

                UpdateSMS(smsRecordId,smsStatus);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public void UpdateSMS(Guid smsRecordId, string smsStatus)
        {

            using (var svc = new ServiceClient($@"AuthType=ClientSecret;Url={BASE_ADDRESS};ClientId={_clientId};ClientSecret={_secret}"))
            {

                var entity = new Entity("dfe_sms", smsRecordId);
                var smsDeliveryStatus = smsStatus == "delivered" ? SMSDeliveryStatus.Delivered :
                    smsStatus == "permanent-failure" ? SMSDeliveryStatus.FailedPermanently :
                    smsStatus == "temporary-failure" ? SMSDeliveryStatus.FailedTemporary :
                    smsStatus == "technical-failure" ? SMSDeliveryStatus.FailedTechnical :
                    smsStatus == "sent" ? SMSDeliveryStatus.SentInternational :
                    smsStatus == "sending" ? SMSDeliveryStatus.Sending : SMSDeliveryStatus.Pending;

                entity["dfe_smsdeliverystatus"] = new OptionSetValue((int)smsDeliveryStatus);

                svc.Update(entity);               


            }
        }

    }
}
