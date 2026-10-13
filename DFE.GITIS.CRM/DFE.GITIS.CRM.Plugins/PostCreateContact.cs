/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 14-11-2018
 Description: This plugin is to create candidate steps automatically when new candidate profile is created. These candidate steps are fetching from Journey Steps entity.
 Create step: Post operation and Synchronous 
 **************************************************************************************/

using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class PostCreateContact : IPlugin
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
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
            {
                //Create an associated privacy policy record for newly created contacts with in dynamics
                if (TargetEntity.Contains("dfe_channelcreation") && TargetEntity["dfe_channelcreation"] != null)
                {
                    var channel = (OptionSetValue)TargetEntity["dfe_channelcreation"];
                    if (channel.Value == 222750011)// Within Dynamics
                    {
                        DataServices.PopulateConfigValues(OrganizationService);
                        var privayPolicy = GetLatestPrivacyPolicyRecord(OrganizationService, DataServices.LatestPrivacyPolicyId);
                        if (privayPolicy != null)
                        {
                            var candidatePrivacyPolicy = new Entity("dfe_candidateprivacypolicy");
                            candidatePrivacyPolicy["dfe_candidate"] = new EntityReference("contact", TargetEntity.Id);
                            candidatePrivacyPolicy["dfe_privacypolicynumber"] = new EntityReference("dfe_privacypolicy", privayPolicy.Id);
                            candidatePrivacyPolicy["dfe_name"] = "Call centre";//privayPolicy["dfe_name"];
                            OrganizationService.Create(candidatePrivacyPolicy);
                        }
                    }
                    else if (channel.Value == 222750041)// Within Dynamics - Website
                    {
                        DataServices.PopulateConfigValues(OrganizationService);
                        var privayPolicy = GetLatestPrivacyPolicyRecord(OrganizationService, DataServices.WebsiteInboundPrivacyPolicyId);
                        if (privayPolicy != null)
                        {
                            var candidatePrivacyPolicy = new Entity("dfe_candidateprivacypolicy");
                            candidatePrivacyPolicy["dfe_candidate"] = new EntityReference("contact", TargetEntity.Id);
                            candidatePrivacyPolicy["dfe_privacypolicynumber"] = new EntityReference("dfe_privacypolicy", privayPolicy.Id);
                            candidatePrivacyPolicy["dfe_name"] = "Call centre - Website";//privayPolicy["dfe_name"];
                            OrganizationService.Create(candidatePrivacyPolicy);
                        }
                    }
                }

                //Create Candidate Steps for ITT candidates
                if (TargetEntity.Contains("dfe_typeofcandidate") && TargetEntity["dfe_typeofcandidate"] != null)
                {
                    var CandidateType = (OptionSetValue)TargetEntity["dfe_typeofcandidate"];
                    if (CandidateType.Value == 222750000)// ITT - Create candidate steps for ITT candidates
                    {
                        CreateITTCandidateSteps(OrganizationService, TargetEntity);
                    }
                    else if (CandidateType.Value == 222750001)// RTT - Create candidate steps for RTT candidates
                    {
                        CreateRTTCandidateSteps(OrganizationService, TargetEntity);
                    }
                }
            }
            else if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!PluginExecutionContext.PreEntityImages.Contains("PreImage") || PluginExecutionContext.PreEntityImages["PreImage"] as Entity == null)
                {
                    return;
                }
                var PreImage = PluginExecutionContext.PreEntityImages["PreImage"] as Entity;
                var previousCandidateType = (OptionSetValue)PreImage["dfe_typeofcandidate"];
                var currentCandidateType = (OptionSetValue)TargetEntity["dfe_typeofcandidate"];
                tracingService.Trace($"previousCandidateType: {previousCandidateType.Value}");
                tracingService.Trace($"currentCandidateType: {currentCandidateType.Value}");
                //throw new InvalidPluginExecutionException($"TargetEntity.Id: {TargetEntity.Id}");
                tracingService.Trace($"TargetEntity.Id: {TargetEntity.Id}");
                tracingService.Trace($"PreImage.Id: {PreImage.Id}");

                if (previousCandidateType.Value != currentCandidateType.Value)
                {
                    if (currentCandidateType.Value == 222750000)// ITT
                    {
                        DeleteExistingCandidateSteps(OrganizationService, TargetEntity, tracingService);
                        CreateITTCandidateSteps(OrganizationService, TargetEntity);
                    }
                    else if (currentCandidateType.Value == 222750001)// RTT
                    {
                        DeleteExistingCandidateSteps(OrganizationService, TargetEntity, tracingService);
                        CreateRTTCandidateSteps(OrganizationService, TargetEntity);
                    }
                }
            }            
        }

        private static void DeleteExistingCandidateSteps(IOrganizationService OrganizationService, Entity TargetEntity, ITracingService tracingService)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            //criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Active
            criteria.Conditions.Add(new ConditionExpression("dfe_contact", ConditionOperator.Equal, TargetEntity.Id));

            var query = new QueryExpression("dfe_candidatejourneystep")
            {
                ColumnSet = new ColumnSet("dfe_candidatejourneystepid"),
                Criteria = criteria
            };
            var candidateSteps = OrganizationService.RetrieveMultiple(query);
            //throw new InvalidPluginExecutionException($"candidateSteps: {candidateSteps.Entities.Count}");
            tracingService.Trace($"candidateSteps: {candidateSteps.Entities.Count}");
            foreach (var candidateStep in candidateSteps.Entities)
            {
                OrganizationService.Delete("dfe_candidatejourneystep", candidateStep.Id);
            }
        }

        private static void CreateRTTCandidateSteps(IOrganizationService OrganizationService, Entity TargetEntity)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Active
            criteria.Conditions.Add(new ConditionExpression("dfe_type", ConditionOperator.Equal, 222750001));//RTT

            var query = new QueryExpression("dfe_adminjourneystep")
            {
                ColumnSet = new ColumnSet("dfe_name", "dfe_order", "dfe_type", "dfe_category"),
                Criteria = criteria
            };
            var journeySteps = OrganizationService.RetrieveMultiple(query);
            Entity candidateStep;
            foreach (var journeyStep in journeySteps.Entities)
            {
                candidateStep = new Entity("dfe_candidatejourneystep");
                candidateStep["dfe_contact"] = TargetEntity.ToEntityReference();
                candidateStep["dfe_name"] = journeyStep["dfe_name"];
                candidateStep["dfe_order"] = journeyStep["dfe_order"];
                candidateStep["dfe_type"] = journeyStep["dfe_type"];
                candidateStep["dfe_category"] = journeyStep["dfe_category"];
                OrganizationService.Create(candidateStep);
            }
        }


        private static void CreateITTCandidateSteps(IOrganizationService OrganizationService, Entity TargetEntity)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Active
            criteria.Conditions.Add(new ConditionExpression("dfe_type", ConditionOperator.Equal, 222750000));     //ITT               

            var query = new QueryExpression("dfe_adminjourneystep")
            {
                ColumnSet = new ColumnSet("dfe_name", "dfe_order", "dfe_type", "dfe_category"),
                Criteria = criteria
            };
            var journeySteps = OrganizationService.RetrieveMultiple(query);
            Entity candidateStep;
            foreach (var journeyStep in journeySteps.Entities)
            {
                candidateStep = new Entity("dfe_candidatejourneystep");
                candidateStep["dfe_contact"] = TargetEntity.ToEntityReference();
                candidateStep["dfe_name"] = journeyStep["dfe_name"];
                candidateStep["dfe_order"] = journeyStep["dfe_order"];
                candidateStep["dfe_type"] = journeyStep["dfe_type"];
                candidateStep["dfe_category"] = journeyStep["dfe_category"];
                OrganizationService.Create(candidateStep);
            }
        }

        private Entity GetLatestPrivacyPolicyRecord(IOrganizationService service, int latestPrivacyPolicyId)
        {
            var criteria = new FilterExpression(LogicalOperator.And);
            criteria.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));//Active
            criteria.Conditions.Add(new ConditionExpression("dfe_policyid", ConditionOperator.Equal, latestPrivacyPolicyId));
            criteria.Conditions.Add(new ConditionExpression("dfe_active", ConditionOperator.Equal, true));//Yes

            var query = new QueryExpression("dfe_privacypolicy")
            {
                ColumnSet = new ColumnSet("dfe_name"),
                Criteria = criteria
            };
            var results = service.RetrieveMultiple(query);
            if (results != null && results.Entities.Count > 0 && results.Entities[0] != null)
            {
                return results.Entities[0];
            }
            return null;
        }
    }
}
