/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 02-06-2021
 Description: This plugin is to create a staging record to capture previous and current candidate type values from contact.
 Update step: Post operation and Synchronous. PreImage was registered. 
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class UpdateContactCandidateType : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var OrganizationService = ServiceFactory.CreateOrganizationService(null);
            
            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
                {
                    return;
                }
                var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;

                if (!PluginExecutionContext.PreEntityImages.Contains("PreImage") || PluginExecutionContext.PreEntityImages["PreImage"] as Entity == null)
                {
                    return;
                }
                var PreImage = PluginExecutionContext.PreEntityImages["PreImage"] as Entity;
                if (TargetEntity.Contains("dfe_typeofcandidate") && TargetEntity["dfe_typeofcandidate"] != null && PreImage.Contains("dfe_typeofcandidate") && PreImage["dfe_typeofcandidate"] != null)
                {
                    if (PreImage["dfe_typeofcandidate"] != TargetEntity["dfe_typeofcandidate"])
                    {
                        tracingService.Trace($"Creating stage record");
                        var stageRecord = new Entity("dfe_stage");
                        stageRecord["dfe_name"] = "Candidate type changed";
                        stageRecord["dfe_candidateid"] = TargetEntity.ToEntityReference();
                        stageRecord["dfe_previouscandidatetype"] = PreImage["dfe_typeofcandidate"];
                        stageRecord["dfe_currentcandidatetype"] = TargetEntity["dfe_typeofcandidate"];
                        OrganizationService.Create(stageRecord);
                        tracingService.Trace($"Stage record Created");
                    }
                }
            }
        }
    }
}
