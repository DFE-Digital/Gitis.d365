/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 10-01-2019
 Description: This plugin is to create a followup phone call based on the outcome codes of the phone call.
 Create step: Post operation and Synchronous 
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateFollowupCalls : IPlugin
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

            if (!string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
            //if (!PluginExecutionContext.PreEntityImages.Contains("PreImage") || PluginExecutionContext.PreEntityImages["PreImage"] as Entity == null)
            //{
            //    return;
            //}
            //var PreImage = PluginExecutionContext.PreEntityImages["PreImage"] as Entity;

            if (!PluginExecutionContext.PostEntityImages.Contains("PostImage") || PluginExecutionContext.PostEntityImages["PostImage"] as Entity == null)
            {
                return;
            }
            var PostImage = PluginExecutionContext.PostEntityImages["PostImage"] as Entity;

            if (TargetEntity.Contains("dfe_calloutcomecodeoutbound") && TargetEntity["dfe_calloutcomecodeoutbound"] != null)
            {
                var selectedOption = (OptionSetValue)TargetEntity["dfe_calloutcomecodeoutbound"];
                DataServices.PopulateConfigValues(OrganizationService);
                if (DataServices.OutcomeCodesForFollowupCall.Contains(selectedOption.Value))
                {
                    var newPhoneCall = new Entity("phonecall");                                      
                    var weightToReduce = DataServices.WeightReductionForFollowups;

                    if(!PostImage.Contains("dfe_callweighting") || !PostImage.Contains("dfe_callattemptnumber"))
                    {
                        return;
                    }

                    var callWeight = (int)PostImage["dfe_callweighting"];
                    var callAttemptNumber = (int)PostImage["dfe_callattemptnumber"];
                    if (callAttemptNumber == DataServices.MaxNoOfCallAttempts)
                    {
                        return;
                    }
                    newPhoneCall["dfe_callattemptnumber"] = callAttemptNumber + 1;
                    newPhoneCall["dfe_callweighting"] = callWeight - weightToReduce;
                    newPhoneCall["dfe_appointmentrequired"] = false;

                    // I don't this code is required, but leaving here for reference - VG - 04082021
                    //Set owner as outbound team
                    //if (PostImage.Contains("dfe_channelcreation") && PostImage["dfe_channelcreation"] != null)
                    //{
                    //    var channelCreation = (OptionSetValue)PostImage["dfe_channelcreation"];
                    //    if (channelCreation.Value == 222750003)// GITIS-Website Callback request
                    //    {
                    //        newPhoneCall["ownerid"] = new EntityReference("team", DataServices.OutboundWebsiteTeamGuid);
                    //    }
                    //    else
                    //    {
                    //        newPhoneCall["ownerid"] = new EntityReference("team", DataServices.OutboundTeamGuid);
                    //    }
                    //}
                    //else
                    //{
                    //    newPhoneCall["ownerid"] = new EntityReference("team", DataServices.OutboundTeamGuid);//is it a bug??
                    //}

                    newPhoneCall["ownerid"] = new EntityReference("team", DataServices.OutboundWebsiteTeamGuid);

                    if (PostImage.Contains("to") && PostImage["to"] != null)
                    {
                        var candidates = (PostImage["to"] as EntityCollection);
                        if (candidates != null && candidates.Entities.Count == 1 && candidates.Entities[0] != null)
                        {
                            var candidateEntity = candidates.Entities[0].ToEntity<Entity>();
                            if (!DataServices.CandidateProfileAllowPhoneCalls(OrganizationService, candidateEntity, candidateEntity.Id))
                            {
                                return;
                            }
                            if (candidateEntity["partyid"] is EntityReference candidate)
                            {
                                var entityCollection = new EntityCollection();
                                var activityParty = new Entity("activityparty");
                                activityParty["partyid"] = new EntityReference("contact", candidate.Id);
                                entityCollection.Entities.Add(activityParty);
                                newPhoneCall["to"] = entityCollection;

                                //assign regarding object id , to fetch open calls when merging
                                newPhoneCall["regardingobjectid"] = new EntityReference("contact", candidate.Id);
                            }
                        }
                    }
                    //if (PostImage.Contains("description") && PostImage["description"] != null)
                    //{
                    //    newPhoneCall["description"] = PostImage["description"];
                    //}
                    //if (PostImage.Contains("dfe_postcallnotes") && PostImage["dfe_postcallnotes"] != null)
                    //{
                    //    newPhoneCall["dfe_postcallnotes"] = PostImage["dfe_postcallnotes"];
                    //}
                    if (PostImage.Contains("dfe_talkingpoints") && PostImage["dfe_talkingpoints"] != null)
                    {
                        newPhoneCall["dfe_talkingpoints"] = PostImage["dfe_talkingpoints"];
                    }
                    if (PostImage.Contains("phonenumber") && PostImage["phonenumber"] != null)
                    {
                        newPhoneCall["phonenumber"] = PostImage["phonenumber"];
                    }
                    if (PostImage.Contains("subject") && PostImage["subject"] != null)
                    {
                        newPhoneCall["subject"] = PostImage["subject"];
                    }
                    newPhoneCall["directioncode"] = PostImage["directioncode"];

                    var phoneCallId = OrganizationService.Create(newPhoneCall);

                    //Create Queue Item record
                    //var queueitem = new Entity("queueitem");
                    //queueitem["dfe_callweighting"] = callWeight - weightToReduce; ;
                    //queueitem["objectid"] = new EntityReference("phonecall", phoneCallId);
                    //queueitem["queueid"] = new EntityReference("queue", DataServices.OutboundCallQueueGuid);                    
                    //OrganizationService.Create(queueitem);

                    //Copy call topics from Previous phone call
                    CreateCallTopics(OrganizationService, TargetEntity.Id, phoneCallId);
                }
            }
        }

        private void CreateCallTopics(IOrganizationService organizationService, Guid oldPhoneCallId, Guid newPhoneCallId)
        {
            var callTopic = new Entity("dfe_calltopics");
            callTopic["dfe_phonecall"] = new EntityReference("phonecall", newPhoneCallId);
            var records = DataServices.GetCallTopicsForPhoneCall(organizationService, oldPhoneCallId);
            if (records != null && records.Entities.Count > 0)
            {
                foreach (var callTopicRecord in records.Entities)
                {
                    var topicId = (EntityReference)callTopicRecord["dfe_topic"];
                    callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Id);                    
                    organizationService.Create(callTopic);
                }
            }            
        }
    }
}