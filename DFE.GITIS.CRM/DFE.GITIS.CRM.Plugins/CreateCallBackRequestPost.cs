/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 28-01-2019
 Description: This plugin runs in post operation mode , creates call topics, merges any open outbound phone calls and creates a queue item record.
 Create step: Pre operation and Synchronous 
 **************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateCallBackRequestPost : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            // Obtain the execution context service from the service provider.
            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the Organization Service factory service from the service provider
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the Organization Service.
            var OrganizationService = ServiceFactory.CreateOrganizationService(null);

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (!string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;

            if (TargetEntity.Contains("dfe_channelcreation") && TargetEntity["dfe_channelcreation"] != null)
            {
                DataServices.PopulateConfigValues(OrganizationService);
                var optionSelected = (OptionSetValue)TargetEntity["dfe_channelcreation"];
                if (optionSelected.Value == 222750000)//Callback Request
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        var openCalls = DataServices.GetOpenOutboundPhoneCallsByContact(OrganizationService, contactId);
                        if (openCalls != null && openCalls.Entities.Count > 0)//Open outbound calls found
                        {
                            foreach (var openCall in openCalls.Entities)
                            {
                                if (openCall.Id != TargetEntity.Id)
                                {
                                    //Update exisitng call topics with new phone call id
                                    UpdateCallTopicsForPhoneCall(OrganizationService, openCall.Id, TargetEntity.Id);

                                    //Close the phone call
                                    CloseOpenPhoneCall(OrganizationService, openCall.Id);

                                    // SHUJAT RE-INSTATE //Close the sms reminder for this callback
                                    // SHUJAT RE-INSTATE CloseOpenSms(OrganizationService, openCall.Id);
                                }
                            }
                        }

                        //Create new phone call topic for call back request
                        var callTopic = new Entity("dfe_calltopics");
                        callTopic["dfe_phonecall"] = new EntityReference("phonecall", TargetEntity.Id);
                        Guid? topicId = DataServices.GetTopicByValue(OrganizationService, 222750008);//Call back request
                        if (topicId.HasValue)
                        {
                            callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Value);
                        }
                        OrganizationService.Create(callTopic);

                        //SHUJAT RE-INSTATE //Create a sms reminder for the scheduled call back
                        //SHUJAT RE-INSTATE CreateCallBackSms(OrganizationService, contactId, TargetEntity, PluginExecutionContext, TargetEntity.Id, tracingService);
                    }
                }
                else if (optionSelected.Value == 222750002)//Summer Campaign
                {
                    if (TargetEntity.Contains("regardingobjectid") && TargetEntity["regardingobjectid"] != null)
                    {
                        var candidate = (EntityReference)TargetEntity["regardingobjectid"];
                        //if (TargetEntity.Contains("to") && TargetEntity["to"] != null)
                        //{
                        //    var candidates = (TargetEntity["to"] as EntityCollection);
                        //    if (candidates != null && candidates.Entities.Count == 1 && candidates.Entities[0] != null)
                        //    {
                        //        var candidateEntity = candidates.Entities[0].ToEntity<Entity>();
                        //        if (candidateEntity["partyid"] is EntityReference candidate)
                        //        {
                        var openCalls = DataServices.GetOpenOutboundPhoneCallsByContact(OrganizationService, candidate.Id);
                        if (openCalls != null && openCalls.Entities.Count > 0)//Open outbound calls found
                        {
                            foreach (var openCall in openCalls.Entities)
                            {
                                if (openCall.Id != TargetEntity.Id)
                                {
                                    //Update exisitng call topics with new phone call id
                                    UpdateCallTopicsForPhoneCall(OrganizationService, openCall.Id, TargetEntity.Id);

                                    //Close the phone call
                                    CloseOpenPhoneCall(OrganizationService, openCall.Id);
                                }
                            }
                        }

                        //Create new phone call topic for call back request
                        var callTopic = new Entity("dfe_calltopics");
                        callTopic["dfe_phonecall"] = new EntityReference("phonecall", TargetEntity.Id);
                        Guid? topicId = DataServices.GetSummerCampaignTopic(OrganizationService);
                        if (topicId.HasValue)
                        {
                            callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Value);
                        }
                        OrganizationService.Create(callTopic);
                        //            }
                        //        }

                    }

                }
                else if (optionSelected.Value == 222750003)// GITIS-Website Callback request
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        var openCalls = DataServices.GetOpenOutboundPhoneCallsByContact(OrganizationService, contactId);
                        if (openCalls != null && openCalls.Entities.Count > 0)//Open outbound calls found
                        {
                            foreach (var openCall in openCalls.Entities)
                            {
                                if (openCall.Id != TargetEntity.Id)
                                {
                                    //Update exisitng call topics with new phone call id
                                    UpdateCallTopicsForPhoneCall(OrganizationService, openCall.Id, TargetEntity.Id);

                                    //Close the phone call
                                    CloseOpenPhoneCall(OrganizationService, openCall.Id);

                                    //SHUJAT RE-INSTATE //Close the sms reminder for this callback
                                    //SHUJAT RE-INSTATE CloseOpenSms(OrganizationService, openCall.Id);
                                }
                            }
                        }

                        //Create new phone call topic for call back request
                        var callTopic = new Entity("dfe_calltopics");
                        callTopic["dfe_phonecall"] = new EntityReference("phonecall", TargetEntity.Id);
                        Guid? topicId = DataServices.GetTopicByValue(OrganizationService, 222750027);//GITIS-TTA Service
                        if (topicId.HasValue)
                        {
                            callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Value);
                        }
                        OrganizationService.Create(callTopic);

                        //SHUJAT RE-INSTATE //Create a sms reminder for the scheduled call back
                        //SHUJAT RE-INSTATE CreateCallBackSms(OrganizationService, contactId, TargetEntity, PluginExecutionContext, TargetEntity.Id, tracingService);
                    }
                }
                else if (optionSelected.Value == 222750004)// GITIS-Website book a Call back
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        var openCalls = DataServices.GetOpenOutboundPhoneCallsByContact(OrganizationService, contactId);
                        
                        if (openCalls != null && openCalls.Entities.Count > 0)//Open outbound calls found
                        {
                            foreach (var openCall in openCalls.Entities)
                            {
                                if (openCall.Id != TargetEntity.Id)
                                {
                                    //Update exisitng call topics with new phone call id
                                    UpdateCallTopicsForPhoneCall(OrganizationService, openCall.Id, TargetEntity.Id);

                                    //Close the phone call
                                    CloseOpenPhoneCall(OrganizationService, openCall.Id);

                                    //SHUJAT RE-INSTATE //Close the sms reminder for this callback
                                    //SHUJAT RE-INSTATE CloseOpenSms(OrganizationService, openCall.Id);
                                }
                            }
                        }

                        //Create new phone call topic for call back request
                        var callTopic = new Entity("dfe_calltopics");
                        callTopic["dfe_phonecall"] = new EntityReference("phonecall", TargetEntity.Id);
                        Guid? topicId = DataServices.GetTopicByValue(OrganizationService, 1);//Book a call back
                        if (topicId.HasValue)
                        {
                            callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Value);
                        }
                        OrganizationService.Create(callTopic);

                        //SHUJAT RE-INSTATE //Create a sms reminder for the scheduled call back
                        //SHUJAT RE-INSTATE CreateCallBackSms(OrganizationService, contactId, TargetEntity, PluginExecutionContext, TargetEntity.Id, tracingService);

                    }
                }
            }
        }
        private void CreateCallBackSms(IOrganizationService OrganizationService, Guid contactId, Entity TargetEntity, IPluginExecutionContext PluginExecutionContext, Guid phoneCallId, ITracingService tracing)
        {
            var contact = DataServices.GetContactById(OrganizationService, contactId);
            var callBackSms = new Entity("dfe_sms");
            var from = new Entity("activityparty");
            var to = new Entity("activityparty");

            callBackSms["dfe_directioncode"] = true;//Outgoing
            if (contact == null)
                return;
            callBackSms["scheduledstart"] = GetReminderTimeForSms(TargetEntity);
            callBackSms["scheduledend"] = GetReminderTimeForSms(TargetEntity).AddMinutes(30);
            callBackSms["dfe_mobilenumber"] = contact.Attributes["address1_telephone1"];
            callBackSms["dfe_phonecallid"] = phoneCallId.ToString();

            // set value in TO field
            to["partyid"] = new EntityReference("contact", contactId);
            callBackSms["to"] = new Entity[] { to };

            // set value in FROM field
            from["partyid"] = new EntityReference("systemuser", PluginExecutionContext.UserId);
            callBackSms["from"] = new Entity[] { from };

            callBackSms["subject"] = "Test Callback SMS";//Get the real value??
            callBackSms["description"] = "Hello World";//Get the real value??
            callBackSms["dfe_smsdeliverystatus"] = new OptionSetValue(222750000);//Created

            //Set regarding object property (i.e. The entity record, which u want this sms associated with)
            EntityReference regardingObject = new EntityReference("contact", contactId);
            callBackSms["regardingobjectid"] = regardingObject;

            var smsId = OrganizationService.Create(callBackSms);

            // update the statecode and statuscode
            var updateSmsAttributes = new Entity("dfe_sms", smsId);

            updateSmsAttributes["statecode"] = new OptionSetValue(3);//Scheduled
            updateSmsAttributes["statuscode"] = new OptionSetValue(4);//Scheduled


            tracing.Trace("Updating statecode and statuscode sms fields");

            OrganizationService.Update(updateSmsAttributes);
        }

        private DateTime GetReminderTimeForSms(Entity TargetEntity)
        {
            DateTime smsTime = TargetEntity.GetAttributeValue<DateTime>("scheduledstart");
            
            return smsTime.AddHours(-24);         
        }
        private void UpdateCallTopicsForPhoneCall(IOrganizationService organizationService, Guid phoneCallId, Guid newPhoneCallId)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Status
            criteria.Conditions.Add(new ConditionExpression("statuscode", ConditionOperator.Equal, 1));//Status Reason
            criteria.Conditions.Add(new ConditionExpression("dfe_phonecall", ConditionOperator.Equal, phoneCallId));

            var query = new QueryExpression("dfe_calltopics")
            {
                ColumnSet = new ColumnSet("dfe_calltopicsid"),
                Criteria = criteria
            };
            var callTopics = organizationService.RetrieveMultiple(query);

            if (callTopics != null && callTopics.Entities != null)
            {
                foreach (var callTopic in callTopics.Entities)
                {
                    callTopic["dfe_phonecall"] = new EntityReference("phonecall", newPhoneCallId);
                    organizationService.Update(callTopic);
                }
            }
        }

        private void CloseOpenPhoneCall(IOrganizationService organizationService, Guid phoneCallId)
        {
            // 2- Made, 4 - recieved
            SetStateRequest s = new SetStateRequest()
            {
                EntityMoniker = new EntityReference("phonecall", phoneCallId),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(2), //Made            
            };
            organizationService.Execute(s);
        }

        private void CloseOpenSms(IOrganizationService organizationService, Guid phoneCallId)
        {
            var sms = DataServices.GetSmsByPhonecallId(organizationService, phoneCallId);
            if (sms == null)
                return;

            var closeSms = new Entity("dfe_sms", sms.Id);

            closeSms["statecode"] = new OptionSetValue(1);//Completed
            closeSms["statuscode"] = new OptionSetValue(2);//Completed

            organizationService.Update(closeSms);
        }
    }
}