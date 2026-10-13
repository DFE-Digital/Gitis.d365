/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 13-12-2018
 Description: This plugin is to delete the phonecall record if the appointment record is deleted.
 This is mainly used to sync between phone call records and Appointent records which are show in Calendar.
 Delete step: Post opertaion step in synchronous mode. PreImage was registered with only one parameter dfe_phonecall.
 **************************************************************************************/

using System;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.Plugins
{
    public class DeleteAppointment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            //var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var OrganizationService = ServiceFactory.CreateOrganizationService(PluginExecutionContext.UserId);
            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "delete", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!PluginExecutionContext.PreEntityImages.Contains("PreImage") || PluginExecutionContext.PreEntityImages["PreImage"] as Entity == null)
                {
                    return;
                }
                var PreImage = PluginExecutionContext.PreEntityImages["PreImage"] as Entity;
                if (PreImage.Contains("dfe_phonecall") && PreImage["dfe_phonecall"] != null && PreImage["dfe_phonecall"] is EntityReference phonecall)
                {
                    OrganizationService.Delete("phonecall", phonecall.Id);
                }
            }
        }
    }
}
