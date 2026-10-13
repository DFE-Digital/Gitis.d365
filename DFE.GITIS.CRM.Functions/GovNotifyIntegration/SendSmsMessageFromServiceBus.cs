using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.CRM.Functions.GovNotifyIntegration
{
    public class SendSmsMessageFromServiceBus
    {
        private static readonly string _templateId = Environment.GetEnvironmentVariable("SMS_TEMPLATE_ID");
        private static readonly string _clientKey = Environment.GetEnvironmentVariable("GOV_NOTIFY_SMS_CLIENT_KEY");
        private static readonly string _processQueueName = Environment.GetEnvironmentVariable("PROCESS_SMS_STATUS_QUEUE");
        private static readonly int _morningHourToCheck = short.Parse(Environment.GetEnvironmentVariable("MORNING_HOUR_SEND_CHECK"));
        private static readonly int _eveningHourToCheck = short.Parse(Environment.GetEnvironmentVariable("EVENING_HOUR_SEND_CHECK"));
        private static readonly int _delayMinutesForStatusCheck = short.Parse(Environment.GetEnvironmentVariable("DELAY_MINUTES_SMS_STATUS_CHECK"));

        private readonly ILogger<SendSmsMessageFromServiceBus> _logger;
        private readonly ServiceBusClient _queueClient;

        public SendSmsMessageFromServiceBus(ServiceBusClient queueClient, ILogger<SendSmsMessageFromServiceBus> logger)
        {
            _queueClient = queueClient;
            _logger = logger;
        }

        [Function(nameof(SendSmsMessageFromServiceBus))]        
        public async Task Run([ServiceBusTrigger("sms-from-crm", Connection = "RootManageSharedAccessKey_SERVICEBUS")] ServiceBusReceivedMessage myQueueItem,
        ServiceBusMessageActions messageReceiver)
        {
            try
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(myQueueItem.Body));

                _logger.LogInformation($"Phonenumber: {data.PhoneNumber}");
                _logger.LogInformation($"Message: {data.Message}");

                Dictionary<string, dynamic> smsPersonalisation = null;

                var gmtZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                var gmtNowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtZone);

                _logger.LogInformation($"DateTime: {gmtNowTime}");

                if (gmtNowTime.Hour < _morningHourToCheck || gmtNowTime.Hour > _eveningHourToCheck)
                {
                    _logger.LogInformation($"Outside of allowed sending times.");
                    throw new Exception("Function is running outside of allowed timeframe.");
                }

                var client = new NotificationClient(_clientKey);
                smsPersonalisation = new Dictionary<String, dynamic>
                    {
                        {"message", data.Message.ToString()}
                    };

                try
                {
                    var response = client.SendSms(mobileNumber: data.PhoneNumber.ToString(), templateId: _templateId, personalisation: smsPersonalisation, clientReference: "");

                    _logger.LogInformation($"SMS sent to : {data.PhoneNumber}, SMS Response Id : {response.id}, SMS CRM Id : {data.Id}");

                    data.NotifyId = response.id;
                    var serialisedMessage = JsonConvert.SerializeObject(data);

                    var message = new ServiceBusMessage(serialisedMessage)
                    {
                        ScheduledEnqueueTime = DateTime.UtcNow.AddMinutes(_delayMinutesForStatusCheck)
                    };

                    var sender = _queueClient.CreateSender(_processQueueName);
                    await sender.SendMessageAsync(message);

                    _logger.LogInformation($"Added to queue {_processQueueName}");

                }
                catch (Exception ex) // any other error we want to deadletter
                {
                    if (ex is NotifyClientException)
                    {
                        throw;  // If a notify crafted error we would like to try again
                    }

                    _logger.LogError(ex, $"Exception : {ex.Message}");
                    await messageReceiver.DeadLetterMessageAsync(myQueueItem);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception : {ex.Message}");
                throw;
            }
        }
    }
}
