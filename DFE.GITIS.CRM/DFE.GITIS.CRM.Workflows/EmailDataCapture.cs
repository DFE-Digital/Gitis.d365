using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.CRM.Workflows
{
    public class EmailDataCapture
    {
        private const string TAG_DATA_CAPTURE_POINTS = "{{!DATA_CAPTURE_POINTS}}";
        private const string EMAIL_TEMPLATE_FIELD = "dfe_selectedemailtemplate";
        private const string EMAIL_DATA_CAPTURE_FIELDS = "dfe_selecteddiscussionpoints";
        private const string EMAIL_REGARDING_CANDIDATE_FIELD = "to";
        private const string PHONECALL_ENTITY = "phonecall";
        private const string PARTYID = "partyid";
        private const string EMAIL_DESCRIPTION = "description";

        public IOrganizationService OrganizationService { get; }
        public ITracingService TracingService { get; }

        public EmailDataCapture(
            IOrganizationService organizationService, 
            ITracingService tracingService)
        {
            OrganizationService = organizationService;
            TracingService = tracingService;
        }

        public void CreateEmailTemplateGivenClosedPhoneCall(Guid phoneCallId, Guid queueId)
        {
            var from = GetFromParty(queueId);

            // Get the phone call 
            var phoneCallRecord = OrganizationService.Retrieve(
                PHONECALL_ENTITY, 
                phoneCallId, 
                new ColumnSet(
                    EMAIL_TEMPLATE_FIELD, 
                    EMAIL_DATA_CAPTURE_FIELDS,
                    EMAIL_REGARDING_CANDIDATE_FIELD));

            // Get the chosen template
            var emailTemplate = RetrieveEmailTemplate(phoneCallRecord.GetAttributeValue<string>(EMAIL_TEMPLATE_FIELD));

            if (emailTemplate == null)
                throw new Exception(string.Format("The template with id {0} does not exist!", emailTemplate));

            // Get the Reference
            var activityParty = phoneCallRecord.GetAttributeValue<EntityCollection>(EMAIL_REGARDING_CANDIDATE_FIELD);
            var contactId = GetPartyId(activityParty[0]);

            // Instantiate an email to be sent created
            InstantiateTemplateRequest request = new InstantiateTemplateRequest() {
                    ObjectId = contactId.Id,
                    ObjectType = contactId.LogicalName,
                    TemplateId = emailTemplate.Id
            };

            // Create the email with the email template
            var response = (InstantiateTemplateResponse)OrganizationService.Execute(request);
            var email = response.EntityCollection[0];
            ReplaceDataCapturePointsWithHtml(email, phoneCallRecord.GetAttributeValue<string>(EMAIL_DATA_CAPTURE_FIELDS));

            var activityPartyTo = new Entity("activityparty");
            activityPartyTo["partyid"] = new EntityReference(contactId.LogicalName, contactId.Id);
            email["to"] = new Entity[] { activityPartyTo }; 
            email["from"] = from;

            OrganizationService.Create(email);
        }

        private Entity[] GetFromParty(Guid queueId)
        {
            // Get a system user to send the email (From: field)
            var activityPartyFrom = new Entity("activityparty");
            activityPartyFrom[PARTYID] = new EntityReference("queue", queueId);
            return new Entity[] { activityPartyFrom};
        }

        private EntityReference GetPartyId(Entity activityParty)
        {
            return activityParty.GetAttributeValue<EntityReference>(PARTYID);
        }

        private Entity RetrieveEmailTemplate(string emailTemplateId)
        {
            return OrganizationService.Retrieve("template", new Guid(emailTemplateId), new ColumnSet(true));
        }

        private void ReplaceDataCapturePointsWithHtml(Entity createdEmail, string capturedPoints)
        {
            string allStrings = capturedPoints.Replace(";", "<br />");
            createdEmail[EMAIL_DESCRIPTION] = createdEmail.GetAttributeValue<string>(EMAIL_DESCRIPTION).Replace(TAG_DATA_CAPTURE_POINTS, allStrings);
        }
    }
}
