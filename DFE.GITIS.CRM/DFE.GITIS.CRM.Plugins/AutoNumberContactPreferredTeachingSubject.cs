/*****************************************************************************************
 Author: Srikanth Nadella
 Date: 12-11-2024
 Description: This plugin is to create candidate preferred subject list automatically.
 Create step: Post operation and Synchronous 
 **************************************************************************************/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFE.GITIS.CRM.Plugins
{
    public class AutoNumberContactPreferredTeachingSubject : IPlugin
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

                targetEntity.Attributes["dfe_id"] = $"{nextId}";
            }


        }

        private static int GetCurrentId(IOrganizationService service, Guid contactId)
        {
            var query = new QueryExpression("dfe_contact_preferredteachingsubject");

            query.ColumnSet.AllColumns = true;

            query.Criteria.AddCondition("dfe_contactid", ConditionOperator.Equal, contactId);

            query.AddOrder("dfe_id", OrderType.Descending);

            var contactPreferredSubjects = service.RetrieveMultiple(query);

            var firstRecord = contactPreferredSubjects?.Entities?.FirstOrDefault();



            return firstRecord != null && firstRecord.Contains("dfe_id") ? !string.IsNullOrEmpty(firstRecord["dfe_id"].ToString()) ? int.Parse(firstRecord["dfe_id"].ToString()) : 0 : 0;
        }


    }
}

