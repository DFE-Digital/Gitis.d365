using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.CustomApis
{

    // This is a plugin for sending delivery report to Channel Definition Delivery report API
    // which should be called by a proxy service that processes incoming notifications from serice provider
    public class DeliveryReportPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
           /* var tracingService = serviceProvider.Get<ITracingService>();
            tracingService.Trace("Executing delivery report plugin");
            var pluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();
            var payload = pluginExecutionContext.InputParameters["payload"] as string;
            tracingService.Trace(payload);
            var organizationService = serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);

            var smsNotificationResponse = JsonUtils.Deserialize<SmsNotificationResponse>(payload);

            var notifyClient = new NotificationClient("rtm__sms_integration__dev-4017adbd-d608-4eb6-ad5d-adb268faa23a-132460cd-c67d-464c-8808-98c0e94b896f");

            var notifyResponse = notifyClient.GetNotificationById(smsNotificationResponse.id);

            var status = notifyResponse.status.ToLower();

            tracingService.Trace($"Status:{status}");

            // Only processing "delivered" status for demo purpose
            if (status == "delivered")
            {
                // Find request id by message id
                var requestId = organizationService.RetrieveMultiple(new QueryExpression("dfe_requestmessagemapping")
                {
                    ColumnSet = new ColumnSet("dfe_requestid"),
                    Criteria = new FilterExpression()
                    {
                        Conditions = { new ConditionExpression("dfe_messageid", ConditionOperator.Equal, smsNotificationResponse.id) }
                    }
                }).Entities[0].GetAttributeValue<string>("dfe_requestid");

                var deliveryReport = new DeliveryReport()
                {
                    ChannelDefinitionId = Guid.Parse("b1b25a46-6da0-4c08-9cf3-505e613c8e30"),
                    // Twilio specific prefix
                    From = "Gov Notify",
                    MessageId = notifyResponse.id,
                    RequestId = requestId,
                    Status = "Delivered",
                    OrganizationId = pluginExecutionContext.OrganizationId.ToString(),
                    StatusDetails = new Dictionary<string, object>()
                };

                var notificatonPayload = JsonUtils.Serialize(deliveryReport);
                tracingService.Trace("Notification payload: {0}", notificatonPayload);

                // Execution of Channel Definitions Notification API
                var response = organizationService.Execute(new OrganizationRequest("msdyn_D365ChannelsNotification")
                {
                    Parameters = {
                        { "notificationPayLoad", notificatonPayload }
                    }
                });

                // Using it for debugging purpose
                pluginExecutionContext.OutputParameters["response"] = response.Results["responseMessage"];
            }*/
        }

        private Credentials GetCredentials(IOrganizationService organizationService, Guid channelDefinitionId, string from)
        {
            var channelInstances = organizationService.RetrieveMultiple(new QueryExpression("msdyn_channelinstance")
            {
                ColumnSet = new ColumnSet("msdyn_extendedentityid"),
                Criteria = new FilterExpression()
                {
                    Conditions = {
                        new ConditionExpression("msdyn_channeldefinitionid", ConditionOperator.Equal, channelDefinitionId),
                        new ConditionExpression("msdyn_contactpoint", ConditionOperator.Equal, from)
                    }
                }
            });

            // Only one channel instance can exist for give definition id and contact point
            var extendedChannelInstance = channelInstances.Entities[0].GetAttributeValue<EntityReference>("msdyn_extendedentityid");

            var sampleChannelInstance = organizationService.Retrieve(extendedChannelInstance.LogicalName, extendedChannelInstance.Id, new ColumnSet("dfe_apikey"));
            var govNotifyAPIKey = sampleChannelInstance.GetAttributeValue<string>("dfe_apikey");

            return new Credentials
            {
                APIKey = govNotifyAPIKey
            };
        }
        private class Credentials
        {
            public string APIKey { get; set; }
        }
    }
}
