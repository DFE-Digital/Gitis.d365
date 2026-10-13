/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 14-04-2020
 Description: This plugin is to create a staging record to capture previous and current ITT Start year field values from contact when ITT Start Year field is updated in contact.
 Update step: Post operation and Synchronous. PreImage was registered. 
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class UpdateContactIttStartYear : IPlugin
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
                if (TargetEntity.Contains("dfe_ittyear") && TargetEntity["dfe_ittyear"] != null && PreImage.Contains("dfe_ittyear") && PreImage["dfe_ittyear"] != null)
                {
                    OptionSetValue preOSVIttYear = PreImage.GetAttributeValue<OptionSetValue>("dfe_ittyear");
                    OptionSetValue targetOSVIttYear = TargetEntity.GetAttributeValue<OptionSetValue>("dfe_ittyear");

                    if (preOSVIttYear.Value != targetOSVIttYear.Value)
                    {
                        tracingService.Trace($"Creating stage record");
                        var stageRecord = new Entity("dfe_stage");
                        stageRecord["dfe_name"] = "Rolled over ITT Year";
                        stageRecord["dfe_candidateid"] = TargetEntity.ToEntityReference();
                        stageRecord["dfe_previousittstartyear"] = PreImage["dfe_ittyear"];
                        stageRecord["dfe_currentittstartyear"] = TargetEntity["dfe_ittyear"];
                        OrganizationService.Create(stageRecord);
                        tracingService.Trace($"Stage record Created");
                    }
                }
            }
        }
    }
}
