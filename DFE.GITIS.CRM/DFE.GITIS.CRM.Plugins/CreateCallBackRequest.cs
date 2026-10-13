/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 24-01-2019
 Description: This plugin runs in pre operation mode , capture information from call back request from web and calculates weight, Talking points before creating phone call.
 Create step: Pre operation and Synchronous 
 **************************************************************************************/
using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateCallBackRequest : IPlugin
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
                var optionSelected = (OptionSetValue)TargetEntity["dfe_channelcreation"];
                DataServices.PopulateConfigValues(OrganizationService);
                if (optionSelected.Value == 222750000)//Callback Request
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        MergeOpenPhoneCalls(OrganizationService, contactId, TargetEntity,optionSelected.Value);
                    }
                }
                else if (optionSelected.Value == 222750002)//Summer campaign
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
                        MergeOpenPhoneCalls(OrganizationService, candidate.Id, TargetEntity, optionSelected.Value);
                    //        }
                    //    }
                    }
                }
                else if (optionSelected.Value == 222750003)// GITIS-Website qual check call back
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        MergeOpenPhoneCalls(OrganizationService, contactId, TargetEntity, optionSelected.Value);
                        
                    }
                }
                else if (optionSelected.Value == 222750004)// GITIS-Website Book a call back
                {
                    if (TargetEntity.Contains("dfe_tocontactguid") && TargetEntity["dfe_tocontactguid"] != null)
                    {
                        var contactId = Guid.Parse(TargetEntity["dfe_tocontactguid"].ToString());
                        MergeOpenPhoneCalls(OrganizationService, contactId, TargetEntity, optionSelected.Value);
                        
                    }
                }
            }
        }


        private void MergeOpenPhoneCalls(IOrganizationService OrganizationService, Guid contactId, Entity TargetEntity, int channel)
        {
            var openCalls = DataServices.GetOpenOutboundPhoneCallsByContact(OrganizationService, contactId);
            if (openCalls != null && openCalls.Entities.Count > 0)//Open outbound calls found
            {
                int totalCallWeight=0;
                string totalTalkingPoints=string.Empty;
                if (channel == 222750000)//Callback request
                {
                    totalCallWeight = DataServices.CallbackRequestCallWeighting;
                    totalTalkingPoints = DataServices.CallbackRequestTalkingPoints;                    
                }
                else if (channel == 222750002)//Summer Campaign
                {
                    totalCallWeight = DataServices.SummerCampaignCallWeighting;
                    totalTalkingPoints = DataServices.SummerCampaignTalkingPoints;
                }
                else if (channel == 222750003)// GITIS-Website qual check call back
                {
                    totalCallWeight = DataServices.WebsiteCallWeighting;
                    totalTalkingPoints = DataServices.WebsiteTalkingPoints;
                }
                else if (channel == 222750004)// GITIS-Website book a Call back
                {
                    totalCallWeight = DataServices.WebsiteBookACallCallWeighting;
                    if (TargetEntity.Contains("dfe_talkingpoints") && TargetEntity["dfe_talkingpoints"] != null)
                    {
                        var talkingPoints = Convert.ToString(TargetEntity["dfe_talkingpoints"]);
                        totalTalkingPoints = $"* {talkingPoints}";
                    }                    
                }
                var TalkingPointsToadd = "";
                foreach (var openCall in openCalls.Entities)
                {
                    if (openCall.Contains("dfe_callweighting") && openCall["dfe_callweighting"] != null)
                    {
                        var callWeight = (int)openCall["dfe_callweighting"];
                        totalCallWeight = totalCallWeight + callWeight;
                    }
                    if (openCall.Contains("dfe_talkingpoints") && openCall["dfe_talkingpoints"] != null)
                    {
                        var callTalkingPoints = openCall["dfe_talkingpoints"].ToString();
                        List<string> finalTalkingPoints = new List<string>();
                        foreach (var ttp in totalTalkingPoints.Split('*'))
                        {
                            if (!string.IsNullOrWhiteSpace(ttp))
                            {
                                finalTalkingPoints.Add(ttp.Trim());
                            }
                        }
                        foreach (var ctp in callTalkingPoints.Split('*'))
                        {
                            if (!string.IsNullOrWhiteSpace(ctp))
                            {
                                if (!finalTalkingPoints.Contains(ctp.Trim()))
                                {
                                    finalTalkingPoints.Add(ctp.Trim());
                                }
                            }
                        }
                        foreach (var ftp in finalTalkingPoints)
                        {
                            TalkingPointsToadd = TalkingPointsToadd + $"* {ftp} \n";
                        }
                    }
                }
                TargetEntity["dfe_talkingpoints"] = TalkingPointsToadd;
                TargetEntity["dfe_callweighting"] = totalCallWeight;

                TargetEntity["dfe_appointmentrequired"] = false;
                TargetEntity["dfe_callattemptnumber"] = 1;
                TargetEntity["directioncode"] = true;//Outbound
                //if (channel == 222750000)
                //{
                    var entityCollection = new EntityCollection();
                    var activityParty = new Entity("activityparty");
                    activityParty["partyid"] = new EntityReference("contact", contactId);
                    entityCollection.Entities.Add(activityParty);
                    TargetEntity["to"] = entityCollection;
                //}
                //if (channel == 222750000)//Callback request
                //{
                    TargetEntity["regardingobjectid"] = new EntityReference("contact", contactId);
                //}
            }
            else if (openCalls != null && openCalls.Entities.Count == 0) // No open outbound phone calls found, create one
            {
                TargetEntity["dfe_callattemptnumber"] = 1;                
                TargetEntity["dfe_appointmentrequired"] = false;
                TargetEntity["directioncode"] = true;//Outbound

                if (channel == 222750000)//Callback request
                {
                    TargetEntity["dfe_callweighting"] = DataServices.CallbackRequestCallWeighting;
                    TargetEntity["dfe_talkingpoints"] = DataServices.CallbackRequestTalkingPoints;
                    TargetEntity["regardingobjectid"] = new EntityReference("contact", contactId);
                }
                else if (channel == 222750002)
                {
                    TargetEntity["dfe_callweighting"] = DataServices.SummerCampaignCallWeighting;
                    TargetEntity["dfe_talkingpoints"] = DataServices.SummerCampaignTalkingPoints;
                }
                else if (channel == 222750003)// GITIS-Website qual check call back
                {
                    TargetEntity["dfe_callweighting"] = DataServices.WebsiteCallWeighting;
                    TargetEntity["dfe_talkingpoints"] = DataServices.WebsiteTalkingPoints;
                    TargetEntity["regardingobjectid"] = new EntityReference("contact", contactId);
                }
                else if (channel == 222750004)//  GITIS-Website book a Call back
                {
                    TargetEntity["dfe_callweighting"] = DataServices.WebsiteBookACallCallWeighting;                    
                    TargetEntity["regardingobjectid"] = new EntityReference("contact", contactId);
                    if (TargetEntity.Contains("dfe_talkingpoints")  && TargetEntity["dfe_talkingpoints"] != null)
                    {
                        var talkingPoints = Convert.ToString(TargetEntity["dfe_talkingpoints"]);
                        TargetEntity["dfe_talkingpoints"] = $"* {talkingPoints}";
                    }
                }
                var entityCollection = new EntityCollection();
                var activityParty = new Entity("activityparty");
                activityParty["partyid"] = new EntityReference("contact", contactId);
                entityCollection.Entities.Add(activityParty);
                TargetEntity["to"] = entityCollection;
            }
        }
    }
}