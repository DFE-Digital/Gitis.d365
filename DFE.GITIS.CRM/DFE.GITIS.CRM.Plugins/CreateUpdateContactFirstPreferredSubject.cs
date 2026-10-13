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
    public class CreateUpdateContactFirstPreferredSubject : IPlugin
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
                var prefferedTeachingSubjectReference = postImage.Contains("dfe_preferredteachingsubject01") ? (EntityReference)postImage["dfe_preferredteachingsubject01"] : null;
                var currentIttYear = postImage.Contains("dfe_ittyear") ? postImage["dfe_ittyear"] : null;
                var preSubject = preImage.Contains("dfe_preferredteachingsubject01") ? (EntityReference)preImage["dfe_preferredteachingsubject01"] : null;

                var currentRecords = GetCurrentRecords(organisationService, postImage.Id);

                if (currentRecords.Count > 0)
                {
                    foreach (var currentRecord in currentRecords)
                    {
                        var entity = new Entity(currentRecord.LogicalName, currentRecord.Id);
                        entity["dfe_currentrecord"] = false;
                        organisationService.Update(entity);

                    }
                }

                Entity record = new Entity("dfe_contact_preferredteachingsubject");
                record["dfe_contactid"] = new EntityReference("contact", postImage.Id); // Lookup
                record["dfe_currentteachingsubject"] = prefferedTeachingSubjectReference; // Lookup
                record["dfe_currentrecord"] = true; // Boolean
                record["dfe_ittstartyear"] = currentIttYear; // Choice
                record["dfe_previousteachingsubject"] = preSubject; // Lookup
                Guid id = organisationService.Create(record);

            }

            if (string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
            {
                var prefferedTeachingSubjectReference = postImage.Contains("dfe_preferredteachingsubject01") ? (EntityReference)postImage["dfe_preferredteachingsubject01"] : null;
                var currentIttYear = postImage.Contains("dfe_ittyear") ? postImage["dfe_ittyear"] : null;

                Entity record = new Entity("dfe_contact_preferredteachingsubject");
                record["dfe_contactid"] = new EntityReference("contact", postImage.Id); // Lookup
                record["dfe_currentteachingsubject"] = prefferedTeachingSubjectReference; // Lookup
                record["dfe_currentrecord"] = true; // Boolean
                record["dfe_ittstartyear"] = currentIttYear; // Choice
                Guid id = organisationService.Create(record);

            }

        }

        private static List<Entity> GetCurrentRecords(IOrganizationService OrganizationService, Guid contactId)
        {
            var query = new QueryExpression("dfe_contact_preferredteachingsubject");

            query.ColumnSet.AllColumns = true;

            query.Criteria.AddCondition("dfe_contactid", ConditionOperator.Equal, contactId);
            query.Criteria.AddCondition("dfe_currentrecord", ConditionOperator.Equal, true);

            query.AddOrder("dfe_id", OrderType.Ascending);

            var contactPreferredSubjects = OrganizationService.RetrieveMultiple(query);

            return contactPreferredSubjects?.Entities?.ToList();
        }



    }
}

