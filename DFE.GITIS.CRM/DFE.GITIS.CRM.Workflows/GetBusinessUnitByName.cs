using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFE.GITIS.CRM.Workflows
{
    public class GetBusinessUnitByName : CodeActivity
    {
        #region Input Parameters

        [Input("Business Unit Name")]
        [RequiredArgument]
        public InArgument<string> BusinessUnitName { get; set; }

        #endregion

        #region Output Parameters

        [Output("Business Unit Reference")]
        [ReferenceTarget("businessunit")]
        public OutArgument<EntityReference> BusinessUnitReference { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            ITracingService tracingService = context.GetExtension<ITracingService>();
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            try
            {
                string businessUnitName = BusinessUnitName.Get(context);

                if (string.IsNullOrWhiteSpace(businessUnitName))
                {
                    throw new InvalidPluginExecutionException("Business Unit Name input parameter cannot be null or empty.");
                }

                tracingService.Trace($"[GetBusinessUnitByName] Searching for business unit with name: {businessUnitName}");

                QueryExpression query = new QueryExpression("businessunit")
                {
                    ColumnSet = new ColumnSet("businessunitid", "name", "parentbusinessunitid"),
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("name", ConditionOperator.Equal, businessUnitName),
                            new ConditionExpression("isdisabled", ConditionOperator.Equal, false)
                        }
                    },
                    TopCount = 1
                };

                EntityCollection results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    tracingService.Trace($"[GetBusinessUnitByName] No business unit found with name: {businessUnitName}");
                    throw new InvalidPluginExecutionException($"No business unit found with the name '{businessUnitName}'. Please verify the name and try again.");
                }

                Entity businessUnit = results.Entities[0];
                EntityReference businessUnitRef = new EntityReference("businessunit", businessUnit.Id);
                businessUnitRef.Name = businessUnit.GetAttributeValue<string>("name");

                tracingService.Trace($"[GetBusinessUnitByName] Business unit found — ID: {businessUnit.Id}, Name: {businessUnitRef.Name}");

                // Optionally log the parent BU for traceability
                EntityReference parentRef = businessUnit.GetAttributeValue<EntityReference>("parentbusinessunitid");
                if (parentRef != null)
                {
                    tracingService.Trace($"[GetBusinessUnitByName] Parent Business Unit: {parentRef.Name} ({parentRef.Id})");
                }

                BusinessUnitReference.Set(context, businessUnitRef);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace($"[GetBusinessUnitByName] Unexpected error: {ex.Message}");
                throw new InvalidPluginExecutionException($"An unexpected error occurred in GetBusinessUnitByName: {ex.Message}", ex);
            }
        }
    }
}