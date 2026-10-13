/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 06-02-2019
 Description: This plugin is to update candidate status and Assigned date if the candidate is assigned to adviser.
 Create step: Pre operation and Synchronous 
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.Plugins
{
    public class UpdateContactOwner : IPlugin
    {
        private const string OWNER_ATTRIB = "ownerid";

        private ITracingService TracingService = null;
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

            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (!string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
            var PreImage = PluginExecutionContext.PreEntityImages["PreImage"] as Entity;

            if (TargetEntity.Contains("ownerid") && TargetEntity["ownerid"] != null)
            {
                TracingService.Trace(string.Format("Context Depth: {0}", PluginExecutionContext.Depth));
                if (ValidForPreOwnerChange(TargetEntity, PreImage))
                {
                    var owner = TargetEntity["ownerid"] as EntityReference;
                    var preOwner = PreImage["ownerid"] as EntityReference;

                    if (owner.LogicalName == "systemuser")
                    {
                        if (DataServices.UserIsInRole(OrganizationService, "GITIS - TPUK - One to One Advisor", owner.Id) 
                            || DataServices.UserIsInRole(OrganizationService, "GITIS - TPUK - One to One Advisor Website", owner.Id))
                        //Need to get from config in future
                        {
                            TracingService.Trace("Target owner is in correct User Role!");

                            TargetEntity["dfe_dateassignedtoadvisor"] = DateTime.Now;
                            TargetEntity["dfe_isadvisorrequiredos"] = new OptionSetValue(222750001);  //Yes
                            TargetEntity["dfe_candidatestatus"] = new OptionSetValue(222750002);// assigned  

                            if (preOwner.LogicalName == "systemuser")
                            {
                                TracingService.Trace(string.Format("Setting to PreOwner with Id {0}", preOwner.Id));
                                TargetEntity["dfe_previousowner"] = preOwner;
                            }
                            if (PreImage.Contains("dfe_dateassignedtoadvisor") && PreImage.GetAttributeValue<DateTime>("dfe_dateassignedtoadvisor") != null)
                            {
                                DateTime prevDateTime = PreImage.GetAttributeValue<DateTime>("dfe_dateassignedtoadvisor");
                                TracingService.Trace(string.Format("Setting assigned date time of {0} to 'date assigned to previous owner datetime'", prevDateTime));
                                TargetEntity["dfe_dateassignedtopreviousowner"] = prevDateTime;
                            }

                            TracingService.Trace("Assign is done!");
                            //Create To-Do Item record
                            var toDoItemRecord = new Entity("dfe_advisertodoitem");
                            toDoItemRecord["dfe_name"] = "New candidate - make initial contact";
                            toDoItemRecord["dfe_regardingcandidate"] = TargetEntity.ToEntityReference();
                            toDoItemRecord["dfe_duedate"] = DataServices.GetNextWorkingDay(DateTime.Now);
                            toDoItemRecord["ownerid"] = TargetEntity["ownerid"];
                            OrganizationService.Create(toDoItemRecord);
                            TracingService.Trace($"To-Do item record created");
                        }
                    }
                    else if (owner.LogicalName == "team")
                    {
                        TracingService.Trace($"Owner name in Target:{owner.Name}");
                        var ownerName = owner.Name;
                        if (string.IsNullOrEmpty(owner.Name))
                        {
                            var teamEntity = DataServices.GetTeamById(OrganizationService, owner.Id);
                            ownerName = teamEntity["name"].ToString();
                        }
                        TracingService.Trace($"Owner name Final:{ownerName}");
                        if (ownerName == "TPUK")
                        {
                            if (TargetEntity.Contains("dfe_candidatestatus") && TargetEntity["dfe_candidatestatus"] != null)
                            {
                                var value = (OptionSetValue)TargetEntity["dfe_candidatestatus"];
                                TracingService.Trace($"Candidate Status Value:{value.Value}");
                                if (value.Value == 222750003) //Previously Assigned Candidate
                                {
                                    TracingService.Trace($"Creating Stage record");
                                    //Create Stage Record with Previous Adviser, Candidate Adviser Status Reason and date reassigned – to record previous values for reporting
                                    var stageRecord = new Entity("dfe_stage");
                                    var reason = PreImage.GetAttributeValue<OptionSetValue>("dfe_candidateadviserstatusreason");
                                    if (reason != null)
                                    {
                                        //var optionText = DataServices.GetOptionSetValueLabel(OrganizationService, "contact", "dfe_candidateadviserstatusreason", reason.Value);
                                        var optionText = PreImage.FormattedValues["dfe_candidateadviserstatusreason"];
                                        stageRecord["dfe_name"] = optionText;
                                    }
                                    else
                                    {
                                        stageRecord["dfe_name"] = "Candidate closed without reason";
                                    }

                                    stageRecord["dfe_candidateid"] = TargetEntity.ToEntityReference();
                                    stageRecord["dfe_dateassignedtocurrentowner"] = DateTime.Now;

                                    if (PreImage.Contains("dfe_dateassignedtoadvisor") && PreImage.GetAttributeValue<DateTime>("dfe_dateassignedtoadvisor") != null)
                                    {
                                        stageRecord["dfe_dateassignedtopreviousowner"] = PreImage["dfe_dateassignedtoadvisor"];
                                    }

                                    stageRecord["dfe_previousowner"] = (EntityReference)PreImage["ownerid"];

                                    if (PreImage.Contains("dfe_currentadviserteamid") && PreImage["dfe_currentadviserteamid"] != null)
                                    {
                                        stageRecord["dfe_previousteam"] = PreImage["dfe_currentadviserteamid"];
                                    }
                                    //stageRecord["dfe_currentowner"] = owner;
                                    //stageRecord["dfe_currentteam"] = PreImage["ownerid"];

                                    OrganizationService.Create(stageRecord);
                                    TracingService.Trace($"Stage record Created");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ValidForPreOwnerChange(Entity TargetEntity, Entity PreImage)
        {
            if (PreImage.Contains(OWNER_ATTRIB))
            {
                EntityReference targetOwnerId = TargetEntity.GetAttributeValue<EntityReference>(OWNER_ATTRIB);
                EntityReference preImageOwnerId = PreImage.GetAttributeValue<EntityReference>(OWNER_ATTRIB);

                TracingService.Trace("Target Entity and Pre Image both have owner attributes");
                TracingService.Trace(string.Format("Target Owner Name = '{0}' ID = '{1}', type = '{2}'", targetOwnerId.Name, targetOwnerId.Id, targetOwnerId.LogicalName));
                TracingService.Trace(string.Format("Pre-Image Owner Name = '{0}' ID = '{1}', type = '{2}'", preImageOwnerId.Name, preImageOwnerId.Id, preImageOwnerId.LogicalName));

                if (targetOwnerId.Id.CompareTo(preImageOwnerId.Id) != 0)
                {
                    TracingService.Trace("Target owner and pre_image owner are different therefore valid for a pre-owner update.");
                    return true;
                }
                else
                {
                    TracingService.Trace("Target owner and pre_image owner are the same hence should not update.");
                    return false;
                }
            }
            else 
            {
                TracingService.Trace("Target Entity has attribute but preimage does not");
                return true;
            }
        }

        private void CreateToDOItemOnAllocation(IOrganizationService organizationService)
        {
                       
        }
    }
}