using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notify.Client;

namespace DFE.GITIS.CRM.Functions.GovNotifyIntegration
{
    public class GetSmsStatus
    {
        private static Lazy<NotificationClient> lazyNotifyClient = new Lazy<NotificationClient>(InitializeNotifyClient);
        private static NotificationClient notifyClient => lazyNotifyClient.Value;

        private static readonly string _processQueueName = Environment.GetEnvironmentVariable("PROCESS_SMS_STATUS_QUEUE");
        private static readonly string _crmQueueName = Environment.GetEnvironmentVariable("UPDATE_CRM_QUEUE");
        private static readonly int _ignoreAfterDays = short.Parse(Environment.GetEnvironmentVariable("IGNORE_AFTER_DAYS"));
        private static readonly int _delayMinutesToReAddToQueue = short.Parse(Environment.GetEnvironmentVariable("RETRY_QUEUE_ADD_DELAY"));
        private static readonly string _clientKey = Environment.GetEnvironmentVariable("GOV_NOTIFY_SMS_CLIENT_KEY");

        private ServiceBusClient _queueClient;

        public GetSmsStatus(ServiceBusClient queueClient)
        {
            _queueClient = queueClient;
        }

        [FunctionName("GetSmsStatus")]
        [FixedDelayRetry(90, "00:01:00")]
        [Disable("AzureWebJobs.GetSmsStatus.Disabled")]
        public async Task Run([ServiceBusTrigger("smsqueue-responses", Connection = "RootManageSharedAccessKey_SERVICEBUS")] Message myQueueItem, ILogger log
            , MessageReceiver messageReceiver)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(myQueueItem.Body));
                log.LogInformation($"Message : {data}");
                log.LogInformation($"Notify Id : {data.NotifyId}");

                var notifyResponse = notifyClient.GetNotificationById(data.NotifyId.ToString());

                log.LogInformation($"Status:{ notifyResponse.status}");
                log.LogInformation($"CreatedOn DateTime:{ data.CreatedOn}, Current DateTime:{ DateTime.Now}");

                if (((DateTime)data.CreatedOn).AddDays(_ignoreAfterDays) < DateTime.Now)
                {
                    log.LogInformation($"Queue message is older than {_ignoreAfterDays} days, ignoring.");
                    return;
                }

                var status = notifyResponse.status.ToLower();

                if (status == "sending" || status == "created" || status == "pending")
                {
                    data.SmsDeliveryStatus = notifyResponse.status;
                    var serialisedMessage = JsonConvert.SerializeObject(data);

                    var messagetoUpdateCrm = new ServiceBusMessage(serialisedMessage);

                    var senderForUpdate = _queueClient.CreateSender(_crmQueueName);
                    await senderForUpdate.SendMessageAsync(messagetoUpdateCrm);

                    var message = new ServiceBusMessage(serialisedMessage)
                    {
                        ScheduledEnqueueTime = DateTime.UtcNow.AddMinutes(_delayMinutesToReAddToQueue)
                    };

                    var sender = _queueClient.CreateSender(_processQueueName);
                    await sender.SendMessageAsync(message);            

                    log.LogInformation($"Sent to {_processQueueName} Queue");
                }
                else if (status == "delivered" || status == "permanent-failure" || status == "temporary-failure" || status == "sent")                   
                {
                    data.SmsDeliveryStatus = notifyResponse.status;
                    var serialisedMessage = JsonConvert.SerializeObject(data);

                    var message = new ServiceBusMessage(serialisedMessage);

                    var sender = _queueClient.CreateSender(_crmQueueName);
                    await sender.SendMessageAsync(message);

                    log.LogInformation($"Sent to {_crmQueueName} Queue");
                }
                else if(status == "technical-failure")
                {
                    await messageReceiver.DeadLetterAsync(myQueueItem.SystemProperties.LockToken);
                    log.LogInformation($"Dead lettered {data.NotifyId} due to remote technical failure.");
                }
                             
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"{ex.Message}");
                throw;
            }
        }

        private static NotificationClient InitializeNotifyClient()
        {
            return new NotificationClient(_clientKey);
        }

    }
}
