using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFE.GITIS.CRM.Workflows
{
    public class CreateEmailCheckListActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Phone")]
        [ReferenceTarget("phonecall")]
        public InArgument<EntityReference> PhoneCall { get; set; }

        [RequiredArgument]
        [Input("From Mailbox")]
        [ReferenceTarget("queue")]
        public InArgument<EntityReference> Queue { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var tracer = context.GetExtension<ITracingService>();

            EmailDataCapture emailDataCapture = new EmailDataCapture(orgService, tracer);

            // Create the email template
            emailDataCapture.CreateEmailTemplateGivenClosedPhoneCall(
                PhoneCall.Get(context).Id,
                Queue.Get(context).Id);
        }
    }
}
