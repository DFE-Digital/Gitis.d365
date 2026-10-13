using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using DFE.GITIS.ADS.DIRECTSYNC.DTO;
using DFE.GITIS.ADS.DIRECTSYNC.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DFE.GITIS.ADS.DIRECTSYNC
{
    public class SyncContactFunction
    {
        private readonly ILogger<DIRECTSYNC.SyncContactFunction> _logger;

        public SyncContactFunction(ILogger<DIRECTSYNC.SyncContactFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(DIRECTSYNC.SyncContactFunction))]
        public async Task Run(
            [ServiceBusTrigger("crm-contact-sync", Connection = "Endpoint=sb://gitisadsmarketing-pr.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=*****")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
            //await messageActions.CompleteMessageAsync(message);

            ContactPayload? contact = null;

            try
            {
                contact = JsonSerializer.Deserialize<ContactPayload>(
                    message.Body.ToString(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (contact is null)
                    throw new InvalidOperationException("Message body deserialised to null.");

                _logger.LogInformation(
                    "SyncContact: processing {Id} (delivery {Attempt}, source: {Source})",
                    contact.ContactId, message.DeliveryCount + 1, contact.TriggerSource);

                // ?? Parallel sync ??????????????????????????????????????????????
                var googleService = new GoogleAdsSyncService(_logger);
                var facebookService = new FacebookAdsSyncService(_logger);

                var googleTask = googleService.SyncCustomerMatchAsync(contact);
                var facebookTask = facebookService.SyncCustomAudienceAsync(contact);

                await Task.WhenAll(googleTask, facebookTask);

                var googleResult = googleTask.Result;
                var facebookResult = facebookTask.Result;

                // ?? Write-back ?????????????????????????????????????????????????
                var writeback = new CrmWritebackService(_logger);
                await writeback.UpdateContactAsync(contact.ContactId, googleResult, facebookResult);

                // ?? Determine overall success ??????????????????????????????????
                if (!googleResult.Success && !facebookResult.Success)
                {
                    // Both failed — abandon so Service Bus retries
                    _logger.LogWarning(
                        "Both syncs failed for {Id}. Abandoning for retry.",
                        contact.ContactId);

                    await messageActions.AbandonMessageAsync(message);
                    return;
                }

                // At least one succeeded — complete
                await messageActions.CompleteMessageAsync(message);

                _logger.LogInformation(
                    "SyncContact: {Id} complete — Google: {G}, Facebook: {F}",
                    contact.ContactId,
                    googleResult.Success ? "?" : "?",
                    facebookResult.Success ? "?" : "?");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SyncContact: unhandled exception for {Id} (delivery {Attempt})",
                    contact?.ContactId ?? "unknown", message.DeliveryCount + 1);

                if (message.DeliveryCount >= 2)
                {
                    // This is the 3rd attempt (0-indexed). Dead-letter so it surfaces
                    // in Azure Portal and doesn't loop indefinitely.
                    _logger.LogError(
                        "SyncContact: max retries reached for {Id}. Dead-lettering.",
                        contact?.ContactId ?? "unknown");

                    await messageActions.DeadLetterMessageAsync(
                        message,
                        deadLetterReason: "MaxRetriesExceeded",
                        deadLetterErrorDescription: ex.Message);

                    // Best-effort failure write-back to CRM so staff can see it
                    if (contact is not null)
                    {
                        var writeback = new CrmWritebackService(_logger);
                        await writeback.UpdateContactAsync(
                            contact.ContactId,
                            new SyncResult { Platform = "Google", Success = false, ErrorMessage = ex.Message },
                            new SyncResult { Platform = "Facebook", Success = false, ErrorMessage = ex.Message });
                    }
                }
                else
                {
                    // Allow Service Bus to retry with its backoff
                    await messageActions.AbandonMessageAsync(message);
                }
            }
        }
    }

}
