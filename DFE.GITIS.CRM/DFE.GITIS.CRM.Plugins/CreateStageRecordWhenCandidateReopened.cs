
/*****************************************************************************************
 Author: Srikanth Nadella
 Date: 12-11-2024
 Description: This plugin is to create candidate preferred subject list automatically.
 Create step: Post operation and Synchronous 
 **************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateStageRecordWhenCandidateReopened : IPlugin
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
            var organisationService = ServiceFactory.CreateOrganizationService(null);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }

            var preImage = PluginExecutionContext.PreEntityImages["PreImage"];
            var postImage = PluginExecutionContext.PostEntityImages["PostImage"];
            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "update", StringComparison.InvariantCultureIgnoreCase))
            {
                //Create an associated privacy policy record for newly created contacts with in dynamics

                var currentStatus = postImage.Contains("dfe_candidateadviserstatus") ? postImage["dfe_candidateadviserstatus"] : null;
                var currentDateReleased = postImage.Contains("dfe_datereleased") ? postImage["dfe_datereleased"] : null;
                var preStatus = preImage.Contains("dfe_candidateadviserstatus") ? preImage["dfe_candidateadviserstatus"] : null;
                var candidateWasClosed = WasCandidateClosed(organisationService, postImage.Id);

                if (preStatus != null && ((OptionSetValue)preStatus).Value == 222750001 && currentStatus != null && currentDateReleased != null && candidateWasClosed)
                {
                    Entity record = new Entity("dfe_stage");
                    record["dfe_name"] = "Reopened After Release"; // Text
                    record["dfe_date"] = DateTime.Now;
                    record["dfe_candidateid"] = new EntityReference("contact", postImage.Id); // Lookup
                    Guid id = organisationService.Create(record);
                }

            }

        }

        private bool WasCandidateClosed(IOrganizationService organizationService, Guid contactId)
        {
            // Set Condition Values
            var query_dfe_name = "Candidate was closed";
            var query_statuscode = 1;
            var query_dfe_candidateid = contactId.ToString();

            // Instantiate QueryExpression query
            var query = new QueryExpression("dfe_stage");

            // Add all columns to query.ColumnSet
            query.ColumnSet.AllColumns = true;

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("dfe_name", ConditionOperator.Equal, query_dfe_name);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, query_statuscode);
            query.Criteria.AddCondition("dfe_candidateid", ConditionOperator.Equal, query_dfe_candidateid);

            var response = organizationService.RetrieveMultiple(query);

            if (response != null && response.Entities.Count > 0)
            {
                return true;
            }

            return false;
        }


    }
}


