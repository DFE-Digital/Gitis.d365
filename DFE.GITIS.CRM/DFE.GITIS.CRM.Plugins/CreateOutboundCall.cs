/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 03-01-2019
 Description: This plugin is to create a phone call for calling pot based on the channel creation field.
 Create step: Post operation and Synchronous 
 **************************************************************************************/

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateOutboundCall : IPlugin
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
            try
            {

                var allowPhoneCalls = DataServices.CandidateProfileAllowPhoneCalls(OrganizationService, TargetEntity, TargetEntity.Id);
                tracingService.Trace($"allowPhoneCalls ; {allowPhoneCalls}");
                if (allowPhoneCalls)
                {
                    CreateOutboundPhoneCall(OrganizationService, tracingService, TargetEntity);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.ToString());
            }
        }

        private bool IsValidChannelForOutboundCall(OptionSetValue channel)
        {
            if (channel.Value == 222750007 //Get Into Teaching Registrations Webapp
                || channel.Value == 222750009 //GITIS Web Registration - Event driven
                || channel.Value == 222750014 //TeachFirst
                || channel.Value == 222750006 //UCAS Registration
                || channel.Value == 222750002 //Grad Fayre Registrations-RMP
                || channel.Value == 222750003 //University Campus Pop-up Registrations-RMP
                || channel.Value == 222750004 //University Campus Registrations-RMP
                || channel.Value == 222750012 //NCTL University Campus Virtual Pop-up Registrations-RMP
                || channel.Value == 222750013 //Off Campus-RMP
                || channel.Value == 222750021 //School Experience - Web Registration
                || channel.Value == 222750023 //Freshers Fair
                || channel.Value == 222750020 //Scholarship Registration
                )
            {
                return true;
            }
            return false;
        }

        private void CreateOutboundPhoneCall(IOrganizationService organizationService, ITracingService tracingService, Entity targetEntity)
        {
            if (!targetEntity.Contains("dfe_channelcreation") || targetEntity["dfe_channelcreation"] == null)
            {
                return;
            }
            var channel = (OptionSetValue)targetEntity["dfe_channelcreation"];
            if (IsValidChannelForOutboundCall(channel))
            {
                if (targetEntity.Contains("dfe_eligibilityrulespassed") && targetEntity["dfe_eligibilityrulespassed"] != null)
                {
                    var phoneCallRequired = Convert.ToBoolean(targetEntity["dfe_eligibilityrulespassed"]);
                    if (IsValidChannelForOutboundCall(channel))//Is it required?
                    {
                        if (!phoneCallRequired)
                        {
                            return;
                        }
                    }
                }

                var phoneCall = new Entity("phonecall");
                phoneCall["directioncode"] = true;//Outbound

                //Candidate to Call
                var entityCollection = new EntityCollection();
                var activityParty = new Entity("activityparty");
                activityParty["partyid"] = new EntityReference("contact", targetEntity.Id);
                entityCollection.Entities.Add(activityParty);
                phoneCall["to"] = entityCollection;

                //Phone Call regarding
                phoneCall["regardingobjectid"] = new EntityReference("contact", targetEntity.Id);

                var phoneNumber = DataServices.GetPhoneNumberFromContact(organizationService, targetEntity);
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    phoneCall["phonenumber"] = phoneNumber;
                }
                phoneCall["dfe_callattemptnumber"] = 1;
                phoneCall["subject"] = $"Phone call to {targetEntity["fullname"].ToString()}";
                phoneCall["dfe_appointmentrequired"] = false;
                DataServices.PopulateConfigValues(organizationService);
                //Update Call Weight
                var weight = CalculateWeight(targetEntity);
                phoneCall["dfe_callweighting"] = weight;

                //Update owner of the Phonecall to Outbound Team
                phoneCall["ownerid"] = new EntityReference("team", DataServices.OutboundWebsiteTeamGuid);

                //Update Talking Points
                var talkingPoints = GetTalkingPoints(targetEntity);
                if (!string.IsNullOrWhiteSpace(talkingPoints))
                {
                    phoneCall["dfe_talkingpoints"] = talkingPoints;
                }
                //Create phone call record
                var phoneCallId = organizationService.Create(phoneCall);

                //Create Call Topics
                CreateCallTopics(organizationService, targetEntity, phoneCallId);
            }
        }

        private void CreateCallTopics(IOrganizationService organizationService, Entity targetEntity, Guid phoneCallId)
        {
            if (targetEntity["dfe_channelcreation"] != null)
            {
                var channel = (OptionSetValue)targetEntity["dfe_channelcreation"];
                var callTopic = new Entity("dfe_calltopics");
                callTopic["dfe_phonecall"] = new EntityReference("phonecall", phoneCallId);

                Guid? topicId = DataServices.GetTopicByValue(organizationService, channel.Value);

                if (topicId.HasValue)
                {
                    callTopic["dfe_topic"] = new EntityReference("dfe_topic", topicId.Value);
                }
                organizationService.Create(callTopic);
            }
        }

        private int CalculateWeight(Entity targetEntity)
        {
            int weight = 0;
            if (targetEntity["dfe_channelcreation"] != null)
            {
                var channel = (OptionSetValue)targetEntity["dfe_channelcreation"];
                if (channel.Value == 222750007 || channel.Value == 222750009)//Get Into Teaching Registrations Webapp or GITIS Web Registration - Event driven
                {
                    if (targetEntity["dfe_ittyear"] != null)
                    {
                        var ittYear = (OptionSetValue)targetEntity["dfe_ittyear"];

                        if (ittYear.Value == 22304 || ittYear.Value == 22305)// 2022 or 2023 --Need to get from config
                        {
                            weight = weight + DataServices.WebRegistartionEEACallWeighting;
                        }
                        else
                        {
                            weight = weight + DataServices.WebRegistartionCallWeighting;
                        }
                    }
                    else
                    {
                        weight = weight + DataServices.WebRegistartionCallWeighting;
                    }
                }
                else if (channel.Value == 222750006) //UCAS Registration
                {
                    weight = weight + DataServices.UcasRegistartionCallWeighting;
                }
                else if (channel.Value == 222750014) // TeachFirst
                {
                    weight = weight + DataServices.TeachFirstCallWeighting;
                }
                else if (channel.Value == 222750020) // Scholarship Registration
                {
                    weight = weight + DataServices.ScholarshipCallWeighting;
                }
                else if (channel.Value == 222750002 || channel.Value == 222750003 || channel.Value == 222750004 || channel.Value == 222750012 || channel.Value == 222750013 || channel.Value == 222750023) // RMP & BAM
                {
                    if (targetEntity["dfe_ittyear"] != null)
                    {
                        var ittYear = (OptionSetValue)targetEntity["dfe_ittyear"];
                        if (ittYear.Value == 22304 || ittYear.Value == 22305)// 2022 or 2023 --Need to get from config
                        {
                            weight = weight + DataServices.RmpEEACallWeighting;
                        }
                        else
                        {
                            weight = weight + DataServices.RmpCallWeighting;
                        }
                    }
                    else
                    {
                        weight = weight + DataServices.RmpCallWeighting;
                    }
                }
                else if (channel.Value == 222750021) // School Experience - Web Registration
                {
                    weight = weight + DataServices.SesCallWeighting;
                }
            }
            return weight;
        }


        private string GetTalkingPoints(Entity targetEntity)
        {
            string talkingPoints = "";
            if (targetEntity["dfe_channelcreation"] != null)
            {
                var channel = (OptionSetValue)targetEntity["dfe_channelcreation"];
                if (channel.Value == 222750007 || channel.Value == 222750009)//Get Into Teaching Registrations Webapp or GITIS Web Registration - Event driven
                {
                    talkingPoints = DataServices.WebRegistartionTalkingPoints;
                }
                else if (channel.Value == 222750006) //UCAS Registration
                {
                    talkingPoints = DataServices.UcasRegistartionTalkingPoints;
                }
                else if (channel.Value == 222750014) // TeachFirst
                {
                    talkingPoints = DataServices.TeachFirstTalkingPoints;
                }
                else if (channel.Value == 222750002 || channel.Value == 222750003 || channel.Value == 222750004 || channel.Value == 222750012 || channel.Value == 222750013 || channel.Value == 222750023) // RMP & BAM
                {
                    talkingPoints = DataServices.RmpTalkingPoints;
                }
                else if (channel.Value == 222750021) // School Experience - Web Registration
                {
                    talkingPoints = DataServices.SesTalkingPoints;
                }
                else if (channel.Value == 222750020) // Scholarship Registration
                {
                    talkingPoints = DataServices.ScholarshipTalkingPoints;
                }
            }
            return talkingPoints;
        }
    }
}
