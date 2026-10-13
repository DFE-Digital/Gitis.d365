using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFE.GITIS.CRM.Workflows
{
    public class CheckForRecentSMSMessage : CodeActivity
    {
        [Input("Home Number")]
        public InArgument<string> _homeNumber { get; set; }

        [Input("Work Number")]
        public InArgument<string> _workNumber { get; set; }

        [Input("Mobile Number")]
        public InArgument<string> _mobileNumber { get; set; }

        [Input("Duplicate Check Hours")]
        public InArgument<string> _duplicateCheckHours { get; set; }

        [Output("Recent Match")]
        public OutArgument<bool> _recentSmsMatch { get; set; }

        [Output("SMS Number")]
        public OutArgument<string> _smsNumberToContact { get; set; }

        public string _targetNumber, _duplicatePastHourCheck;

        protected override void Execute(CodeActivityContext context)
        {
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            var tracer = context.GetExtension<ITracingService>();

            //Set defaults
            _smsNumberToContact.Set(context, "NoValidNumber");
            _recentSmsMatch.Set(context, false);

            var homeNumb = _homeNumber.Get(context);
            var workNumb = _workNumber.Get(context);
            var mobileNumb = _mobileNumber.Get(context);
            _duplicatePastHourCheck = _duplicateCheckHours.Get(context);

            tracer.Trace($"Execute. Home : {homeNumb} , work : {workNumb}, mobile : {mobileNumb}");

            if(string.IsNullOrEmpty(homeNumb) && string.IsNullOrEmpty(workNumb) && string.IsNullOrEmpty(mobileNumb)){
                return;
            }

            SetTargetNumber(context, homeNumb, workNumb, mobileNumb, tracer);

            if (string.IsNullOrEmpty(_targetNumber))
                return;

            QueryForARecentSmsRecord(orgService, context, tracer);
        }

        private void SetTargetNumber(CodeActivityContext context, string homeNumber, string workNumber, string mobileNumber, ITracingService tracer)
        {
            var formattedHomeNumber = FormatPhoneNumber(homeNumber);
            tracer.Trace($"formatted home : {formattedHomeNumber}");


            if (string.IsNullOrEmpty(formattedHomeNumber) == false && formattedHomeNumber.Substring(0, 2) == "07")
            {
                _targetNumber = formattedHomeNumber;
                _smsNumberToContact.Set(context, formattedHomeNumber);
            }
            else
            {
                var formattedMobileNumber = FormatPhoneNumber(mobileNumber);

                if (string.IsNullOrEmpty(formattedMobileNumber) == false && formattedMobileNumber.Substring(0, 2) == "07")
                {
                    _targetNumber = formattedMobileNumber;
                    _smsNumberToContact.Set(context, formattedMobileNumber);
                }
                else
                {
                    var formattedWorkNumber = FormatPhoneNumber(workNumber);

                    if (string.IsNullOrEmpty(formattedWorkNumber) == false && formattedWorkNumber.Substring(0, 2) == "07")
                    {
                        _targetNumber = formattedWorkNumber;
                        _smsNumberToContact.Set(context, formattedWorkNumber);
                    }
                }
            }
        }

        private void QueryForARecentSmsRecord(IOrganizationService orgService, CodeActivityContext context, ITracingService tracer)
        {
            tracer.Trace("QueryForARecentSmsRecord");

            var smsRecordQuery = $@" <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='dfe_sms'>
                    <filter type='and'>
                      <condition attribute='createdon' operator='last-x-hours' value='{_duplicatePastHourCheck}' />
                      <condition attribute='dfe_mobilenumber' operator='eq' value='{_targetNumber}' />
                      <condition attribute='dfe_smsdeliverystatus' operator='ne' value='222750006' /> 
                      </filter>
                  </entity>
                </fetch>";

            tracer.Trace($"Checking for existing SMS records in the last day for {_targetNumber}");

            var smsRecords = orgService.RetrieveMultiple(new FetchExpression(smsRecordQuery));

            tracer.Trace($"Retrieved records : {smsRecords.Entities.Count}");

            if (smsRecords.Entities.Any())
            {
                _recentSmsMatch.Set(context, true);
                return;
            }

            _recentSmsMatch.Set(context, false);
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber?.Trim().Replace(" ", "");

            if (String.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 10)
                return string.Empty;

            if (phoneNumber.Substring(0, 2) == "44")
            {
                phoneNumber = "0" + phoneNumber.Substring(2, phoneNumber.Length - 2);
            }
            else if (phoneNumber.Substring(0, 3) == "044")
            {
                phoneNumber = "0" + phoneNumber.Substring(3, phoneNumber.Length - 3);
            }
            else if (phoneNumber.Substring(0, 4) == "0044")
            {
                phoneNumber = "0" + phoneNumber.Substring(4, phoneNumber.Length - 4);
            }
            else if (phoneNumber.Substring(0, 3) == "+44")
            {
                phoneNumber = "0" + phoneNumber.Substring(3, phoneNumber.Length - 3);
            }
            else if (phoneNumber.Substring(0, 1) != "0")
            {
                phoneNumber = "0" + phoneNumber;
            }
        
           if(IsDigitsOnly(phoneNumber) && phoneNumber.Length == 11)
                return phoneNumber;

            return string.Empty;
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
