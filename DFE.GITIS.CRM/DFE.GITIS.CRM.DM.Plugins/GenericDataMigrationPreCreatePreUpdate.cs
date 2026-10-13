using System;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.DM.Plugins
{
    public class GenericDataMigrationPreCreatePreUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));            
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var target = PluginExecutionContext.InputParameters["Target"] as Entity;
            if (target.Attributes.ContainsKey("dfe_createdby_dm"))
            {
                target["createdby"] = target["dfe_createdby_dm"];
            }
            if (target.Attributes.ContainsKey("dfe_modifiedon_dm"))
            {
                target["modifiedon"] = target["dfe_modifiedon_dm"];
            }
            if (target.Attributes.ContainsKey("dfe_modifiedby_dm"))
            {
                target["modifiedby"] = target["dfe_modifiedby_dm"];
            }
        }
    }
}
