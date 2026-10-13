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
    public class PostOperationContactChannelCreation : IPlugin
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

                var currentRecord = organisationService.Retrieve("dfe_contactchannelcreation", targetEntity.Id, new ColumnSet(true));

                var isMatch = CheckIfConfigExists(organisationService, 
                    currentRecord.Contains("dfe_creationchannelactivities") ? ((OptionSetValue)currentRecord["dfe_creationchannelactivities"]).Value : 0, 
                    currentRecord.Contains("dfe_creationchannelservice") ? ((OptionSetValue)currentRecord["dfe_creationchannelservice"]).Value : 0, 
                    currentRecord.Contains("dfe_creationchannelsource") ? ((OptionSetValue)currentRecord["dfe_creationchannelsource"]).Value : 0);

                organisationService.Update(new Entity(currentRecord.LogicalName, currentRecord.Id) { ["dfe_validationpass"] =  isMatch });
            }

        }

        private bool CheckIfConfigExists(IOrganizationService service, int? creationActivities, int? channelService, int? channelSource)
        {
            // Set Condition Values
            var query_dfe_creationchannelactivities = creationActivities == 0 ? null : creationActivities;
            var query_dfe_creationchannelservice = channelService == 0 ? null : channelService;
            var query_dfe_creationchannelsource = channelSource == 0 ? null : channelSource;
            var query_statuscode = 1;

            // Instantiate QueryExpression query
            var query = new QueryExpression("dfe_creationchannelconfig");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add conditions to query.Criteria
            if (query_dfe_creationchannelactivities != null) {
                query.Criteria.AddCondition("dfe_creationchannelactivities", ConditionOperator.Equal, query_dfe_creationchannelactivities);
            }
            else
            {
                query.Criteria.AddCondition("dfe_creationchannelactivities", ConditionOperator.Null);
            };
            if (query_dfe_creationchannelservice != null)
            {
                query.Criteria.AddCondition("dfe_creationchannelservice", ConditionOperator.Equal, query_dfe_creationchannelservice);
            }
            else
            {
                query.Criteria.AddCondition("dfe_creationchannelservice", ConditionOperator.Null);
            };
            query.Criteria.AddCondition("dfe_creationchannelsource", ConditionOperator.Equal, query_dfe_creationchannelsource);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, query_statuscode);

            var response = service.RetrieveMultiple(query);

            return response?.Entities?.Any() ?? false;

        }


    }
}

