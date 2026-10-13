using DFE.GITIS.CRM.Workflows.Proxy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFE.GITIS.CRM.Workflows
{
    public class GetContentSettingsFromPipeline : CodeActivity
    {
        [Output("1 Week Message Title")]
        public OutArgument<string> _oneWeekReminderTitle { get; set; }

        [Output("1 Week Message Content")]
        public OutArgument<string> _oneWeekReminderMessage { get; set; }

        [Output("1 Week Reminder Start Boundary")]
        public OutArgument<DateTime> _oneWeekReminderStartBoundary { get; set; }

        [Output("1 Week Reminder End Boundary")]
        public OutArgument<DateTime> _oneWeekReminderEndBoundary { get; set; }

        [Output("1 Day Message Title")]
        public OutArgument<string> _oneDayReminderTitle { get; set; }

        [Output("1 Day Message Content")]
        public OutArgument<string> _oneDayReminderMessage { get; set; }

        [Output("1 Day Reminder Start Boundary")]
        public OutArgument<DateTime> _oneDayReminderStartBoundary { get; set; }

        [Output("1 Day Reminder End Boundary")]
        public OutArgument<DateTime> _oneDayReminderEndBoundary { get; set; }

        [Output("Candidate Home Phone")]
        public OutArgument<string> _homePhone { get; set; }

        [Output("Candidate Mobile Phone")]
        public OutArgument<string> _mobilePhone { get; set; }

        [Output("Candidate Work Phone")]
        public OutArgument<string> _workPhone { get; set; }

        public const string ONE_WEEK_REMINDER_TITLE_FIELDNAME = "dfe_1weekremindersmstitle";
        public const string ONE_WEEK_REMINDER_MESSAGE_FIELDNAME = "dfe_1weekremindersmsmessage";
        public const string ONE_WEEK_REMINDER_START_BOUNDARY_FIELDNAME = "dfe_1weekreminderstartboundary";
        public const string ONE_WEEK_REMINDER_END_BOUNDARY_FIELDNAME = "dfe_1weekreminderendboundary";
        public const string ONE_DAY_REMINDER_TITLE_FIELDNAME = "dfe_1dayremindersmstitle";
        public const string ONE_DAY_REMINDER_MESSAGE_FIELDNAME = "dfe_1dayremindersmsmessage";
        public const string ONE_DAY_REMINDER_START_BOUNDARY_FIELDNAME = "dfe_1dayreminderstartboundary";
        public const string ONE_DAY_REMINDER_END_BOUNDARY_FIELDNAME = "dfe_1dayreminderendboundary";

        public const string CANDIDATE_HOME_TELEPHONE_FIELDNAME = "address1_telephone1";
        public const string CANDIDATE_MOBILE_TELEPHONE_FIELDNAME = "mobilephone";
        public const string CANDIDATE_WORK_TELEPHONE_FIELDNAME = "telephone1";

        protected override void Execute(CodeActivityContext context)
        {
            var tracer = context.GetExtension<ITracingService>();
            tracer.Trace("Execute.");
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            var channelActivity = workflowContext?.PostEntityImages[msdyncrm_customerjourneycustomchannelactivity.EntityLogicalName]?
                .ToEntity<msdyncrm_customerjourneycustomchannelactivity>();

            var contentSettingRecordId = channelActivity?.msdyncrm_EntityId; // selected content setting record
            var contactId = channelActivity?.msdyncrm_Contact?.Id;

            if (string.IsNullOrEmpty(contentSettingRecordId) || contactId == null)
            {
                throw new InvalidPluginExecutionException("No context setting or contact id in pipeline.");
            }

            tracer.Trace($"content setting id : {contentSettingRecordId} - Retrieving content settings");

            var contentSettings = orgService.Retrieve(msdyncrm_contentsettings.EntityLogicalName, new Guid(contentSettingRecordId),
                new ColumnSet(
                    ONE_WEEK_REMINDER_TITLE_FIELDNAME,
                    ONE_WEEK_REMINDER_MESSAGE_FIELDNAME,
                    ONE_WEEK_REMINDER_START_BOUNDARY_FIELDNAME,
                    ONE_WEEK_REMINDER_END_BOUNDARY_FIELDNAME, 
                    ONE_DAY_REMINDER_TITLE_FIELDNAME,
                    ONE_DAY_REMINDER_MESSAGE_FIELDNAME, 
                    ONE_DAY_REMINDER_START_BOUNDARY_FIELDNAME,
                    ONE_DAY_REMINDER_END_BOUNDARY_FIELDNAME
                ))?
                .ToEntity<msdyncrm_contentsettings>();


            if (contentSettings == null)
                throw new InvalidPluginExecutionException($"Cannot retrieve content setting record {contentSettingRecordId}");

            _oneWeekReminderTitle.Set(context, contentSettings.dfe_1WeekReminderSMSTitle);
            _oneWeekReminderMessage.Set(context, contentSettings.dfe_1WeekReminderSMSMessage);

            if(contentSettings.dfe_1WeekReminderStartBoundary != null)
                _oneWeekReminderStartBoundary.Set(context, contentSettings.dfe_1WeekReminderStartBoundary);
            else
                _oneWeekReminderStartBoundary.Set(context, DateTime.MinValue);

            if (contentSettings.dfe_1WeekReminderEndBoundary != null)
                _oneWeekReminderEndBoundary.Set(context, contentSettings.dfe_1WeekReminderEndBoundary);
            else
                _oneWeekReminderEndBoundary.Set(context, DateTime.MinValue);

            _oneDayReminderTitle.Set(context, contentSettings.dfe_1DayReminderSMSTitle);
            _oneDayReminderMessage.Set(context, contentSettings.dfe_1DayReminderSMSMessage);

            if(contentSettings.dfe_1DayReminderStartBoundary != null)
                _oneDayReminderStartBoundary.Set(context, contentSettings.dfe_1DayReminderStartBoundary);
            else
                _oneDayReminderStartBoundary.Set(context, DateTime.MinValue);

            if (contentSettings.dfe_1DayReminderEndBoundary != null)
                _oneDayReminderEndBoundary.Set(context, contentSettings.dfe_1DayReminderEndBoundary);
            else
                _oneDayReminderEndBoundary.Set(context, DateTime.MinValue);

            tracer.Trace($"Set SMS properties, looking up contact details..");

            var contactDetails = orgService.Retrieve(Contact.EntityLogicalName, (Guid)contactId, new ColumnSet(               
                CANDIDATE_HOME_TELEPHONE_FIELDNAME, 
                CANDIDATE_MOBILE_TELEPHONE_FIELDNAME, 
                CANDIDATE_WORK_TELEPHONE_FIELDNAME))?.ToEntity<Contact>() ;

            if (contactDetails == null)
                throw new InvalidPluginExecutionException($"Cannot retrieve contact record {contactId}");

            tracer.Trace($"Setting phone numbers..");

            _homePhone.Set(context, contactDetails.Address1_Telephone1);
            _mobilePhone.Set(context, contactDetails.MobilePhone);
            _workPhone.Set(context, contactDetails.Telephone1);
        }

    }
}
