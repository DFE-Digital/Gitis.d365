/*****************************************************************************************
 Author: Srikanth Nadella
 Date: 12-11-2024
 Description: This plugin is to create candidate preferred subject list automatically.
 Create step: Post operation and Synchronous 
 **************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class AutoNumberContactChannelCreation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            // Obtain the execution context service from the service provider.
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the Organization Service factory service from the service provider
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the Organization Service.
            var organisationService = ServiceFactory.CreateOrganizationService(null);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity targetEntity)
            {
               
                var currentId = GetCurrentId(organisationService, ((EntityReference)targetEntity["dfe_contactid"]).Id);

                var nextId = currentId + 1;

                tracingService.Trace($"Next Id: {nextId}");

                targetEntity.Attributes["dfe_stepnumber"] = $"{nextId}";
            }

        }

        private static int GetCurrentId(IOrganizationService service, Guid contactId)
        {
            

            // Instantiate QueryExpression query
            var query = new QueryExpression("dfe_contactchannelcreation");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("dfe_contactid", ConditionOperator.Equal, contactId);

            // Add orders
            query.AddOrder("dfe_stepnumber", OrderType.Descending);


            var contactChannelCreations = service.RetrieveMultiple(query);

            var firstRecord = contactChannelCreations?.Entities?.FirstOrDefault();

            return firstRecord != null && firstRecord.Contains("dfe_stepnumber") ? !string.IsNullOrWhiteSpace(firstRecord["dfe_stepnumber"].ToString()) ? int.Parse(firstRecord["dfe_stepnumber"].ToString()) : 0 : 0;
        }




    }
}

