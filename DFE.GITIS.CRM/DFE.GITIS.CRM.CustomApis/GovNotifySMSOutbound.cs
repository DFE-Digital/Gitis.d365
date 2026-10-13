using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using DFE.GITIS.CRM.CustomApis.GovNotify;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace DFE.GITIS.CRM.CustomApis
{

    // Sample plugin that sends a WhatsApp message using Twilio
    public class GovNotifySMSOutbound : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.Get<ITracingService>();
            tracingService.Trace("Executing outbound sample channel plugin");
            var pluginExecutionContext = serviceProvider.Get<IPluginExecutionContext>();

            // "payload" attribute is required by contract
            var payload = pluginExecutionContext.InputParameters["payload"] as string;
            tracingService.Trace(payload);

            var organizationService = serviceProvider.Get<IOrganizationServiceFactory>().CreateOrganizationService(null);


            try
            {
                //dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(myQueueItem.Body));

                tracingService.Trace($"Payload: {payload}");

                var payloadObject = JsonConvert.DeserializeObject<ChannelContracts.Payload>(payload);

                var gmtZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                var gmtNowTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtZone);

                tracingService.Trace($"DateTime: {gmtNowTime}");
                var credentials = GetCredentials(organizationService, payloadObject.ChannelDefinitionId, payloadObject.From);

                tracingService.Trace($"Key: {credentials.APIKey}");
                var keys = ExtractId(credentials.APIKey);

                tracingService.Trace($"Keys 1: {keys[0]},Keys 2: {keys[1]}");


                Entity contact = null;
                if (!payloadObject.MarketingAppContext.IsTestSend)
                {
                    contact = organizationService.Retrieve(payloadObject.MarketingAppContext.UserEntityType, payloadObject.MarketingAppContext.UserId, new ColumnSet("dfe_optoutsms"));
                }

                if (!IsUkPhoneNumber(payloadObject.To))
                {
                    throw new InvalidPluginExecutionException("Is not UK mobilenumber");
                }

                if (payloadObject.MarketingAppContext.IsTestSend || (contact != null && !contact.GetAttributeValue<bool>("dfe_optoutsms")))
                {

                    var response = new GovNotifySMS(tracingService).Send(mobileNumber: payloadObject.To, templateid: "4aaa3e0b-2547-471c-87a5-3e6eb4ba622e", msgContent: payloadObject.Message.text, keys: keys);

                    tracingService.Trace($"SMS sent to : {payloadObject.To}, SMS Response Id : {response.id}");

                    // Saving request id with message id to be able to find it for delivery reports
                    var requestMappingId = organizationService.Create(new Entity("dfe_requestmessagemapping")
                    {
                        ["dfe_messageid"] = response.id,
                        ["dfe_requestid"] = payloadObject.RequestId
                    });

                    // Create an activity party
                    Entity activityParty = new Entity("activityparty");
                    activityParty["partyid"] = new EntityReference(payloadObject.MarketingAppContext.UserEntityType, payloadObject.MarketingAppContext.UserId);

                    // Create an email activity
                    Entity sms = new Entity("dfe_sms");
                    sms["description"] = payloadObject.Message.text; // Multiline Text
                    sms["subject"] = "SMS"; // Text
                    sms["dfe_directioncode"] = true; // Boolean
                    sms["regardingobjectid"] = new EntityReference("dfe_requestmessagemapping", requestMappingId); // Lookup
                    sms["dfe_smsdeliverystatus"] = new OptionSetValue(222750000); // Choice
                    sms["to"] = new EntityCollection(new List<Entity> { activityParty });

                    //Create SMS activity
                    var smsId = organizationService.Create(sms);

                    var responseObject = new ChannelContracts.Response()
                    {
                        ChannelDefinitionId = payloadObject.ChannelDefinitionId,
                        MessageId = response.id,
                        RequestId = payloadObject.RequestId,
                        Status = "Sent",
                        StatusDetails = null
                    };

                    // "response" attribute is required by contract
                    pluginExecutionContext.OutputParameters["response"] = JsonUtils.Serialize(responseObject);
                }
                else
                {
                    throw new InvalidPluginExecutionException("Candidate has disabled SMS communication");
                }


            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception : {ex.Message}");
                throw;
            }
        }

        public static bool IsUkPhoneNumber(string phoneNumber)
        {
            string pattern = @"^(?:\+44\s?7\d{3}|\s?07\d{3}|\+44\s?\d{2,3}|\s?0\d{2,3})\s?\d{3}\s?\d{3}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }


        private string[] ExtractId(string apiKey)
        {
            var returnList = new List<string>();            

            // Define the regex pattern for a GUID
            string pattern = @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

            // Use Regex.Matches to find all matches of the pattern in the input string
            MatchCollection matches = Regex.Matches(apiKey, pattern);

            // Print each matched GUID
            foreach (Match match in matches)
            {
                returnList.Add(match.Value);
            }

            return returnList.ToArray();
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

