using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFE.GITIS.CRM.Workflows
{
    public class GetTeamByName : CodeActivity
    {
        #region Input Parameters

        [Input("Team Name")]
        [RequiredArgument]
        public InArgument<string> TeamName { get; set; }

        #endregion

        #region Output Parameters

        [Output("Team Reference")]
        [ReferenceTarget("team")]
        public OutArgument<EntityReference> TeamReference { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            // Get the tracing service for diagnostics
            ITracingService tracingService = context.GetExtension<ITracingService>();

            // Get the workflow context
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

            // Get the organization service factory and create a service instance
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            try
            {
                // Retrieve the input parameter
                string teamName = TeamName.Get(context);

                if (string.IsNullOrWhiteSpace(teamName))
                {
                    throw new InvalidPluginExecutionException("Team Name input parameter cannot be null or empty.");
                }

                tracingService.Trace($"[GetTeamByName] Searching for team with name: {teamName}");

                // Build the query to find the team by name
                QueryExpression query = new QueryExpression("team")
                {
                    ColumnSet = new ColumnSet("teamid", "name"),
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("name", ConditionOperator.Equal, teamName),
                            new ConditionExpression("isdefault", ConditionOperator.Equal, false) // Exclude default teams if needed
                        }
                    },
                    TopCount = 1
                };

                EntityCollection results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    tracingService.Trace($"[GetTeamByName] No team found with name: {teamName}");
                    throw new InvalidPluginExecutionException($"No team found with the name '{teamName}'. Please verify the team name and try again.");
                }

                Entity team = results.Entities[0];
                EntityReference teamRef = new EntityReference("team", team.Id);
                teamRef.Name = team.GetAttributeValue<string>("name");

                tracingService.Trace($"[GetTeamByName] Team found — ID: {team.Id}, Name: {teamRef.Name}");

                // Set the output parameter
                TeamReference.Set(context, teamRef);
            }
            catch (InvalidPluginExecutionException)
            {
                // Re-throw known workflow exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace($"[GetTeamByName] Unexpected error: {ex.Message}");
                throw new InvalidPluginExecutionException($"An unexpected error occurred in GetTeamByName: {ex.Message}", ex);
            }
        }
    }
}