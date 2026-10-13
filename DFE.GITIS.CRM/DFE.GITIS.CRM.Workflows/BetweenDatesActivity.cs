using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFE.GITIS.CRM.Workflows
{
    public class BetweenDatesActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Start Date")]
        public InArgument<DateTime> StartDate { get; set; }

        [RequiredArgument]
        [Input("End Date")]
        public InArgument<DateTime> EndDate { get; set; }

        [Output("Result")]
        public OutArgument<int> Result { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var tracer = context.GetExtension<ITracingService>();

            int resultOfDays = BetweenDates(StartDate.Get(context), EndDate.Get(context));
            tracer.Trace(string.Format("Number of days between start date {0} and end date {1} is {2}.",StartDate, EndDate, resultOfDays));
            
            Result.Set(context, resultOfDays);
        }

        private int BetweenDates(DateTime start, DateTime end)
        {
            return Math.Abs((end - start).Days);
        }
    }
}
