using DFE.GITIS.CRM.Workflows.Proxy;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using Xunit;

namespace DFE.GITIS.CRM.Workflows.Tests
{
    public class GetContentSettingsFromPipelineTests
    {
        private XrmFakedContext _context;
        private XrmFakedWorkflowContext _workflowContext;

        private Guid _contactId;
        private Guid _contentSettingId;

        private string _postImageOneWeekSmsTitle;
        private string _postImageOneWeekSmsMessage;
        private DateTime _postImageOneWeekStartBoundary;
        private DateTime _postImageOneWeekEndBoundary;

        private string _postImageOneDaySmsTitle;
        private string _postImageOneDaySmsMessage;
        private DateTime _postImageOneDayStartBoundary;
        private DateTime _postImageOneDayEndBoundary;

        private string _contactHomeNumber;
        private string _contactMobileNumber;
        private string _contactWorkNumber;

        public GetContentSettingsFromPipelineTests()
        {
            _context = new XrmFakedContext();
            _contactId = Guid.NewGuid();
            _contentSettingId = Guid.NewGuid();
            _postImageOneWeekSmsTitle = "Spring 2021 Event - Targeted Invite SMS - Sheffield on 02/03/2021";
            _postImageOneWeekSmsMessage = "Want to find out more about teaching? Join our Virtual Train to Teach event exclusively for the Sheffield area on Tuesday 2nd March 2021. Meet with our expert advisers and local training providers from 5pm - 7pm. Register here http://bit.ly/2MxgvWH Text STOP TO 61300";
            _postImageOneWeekStartBoundary = new DateTime(2021, 2, 22);
            _postImageOneWeekEndBoundary = new DateTime(2021, 2, 24);

            _postImageOneDaySmsTitle = "Spring 2021 Event - Event Reminder SMS - Sheffield on 02/03/2020";
            _postImageOneDaySmsMessage = "Want to find out more about Teaching?  – your place is reserved at the Virtual Train to Teach event for Sheffield tomorrow. Join us from 5pm. Check your email for the unique link.";
            _postImageOneDayStartBoundary = new DateTime(2021, 2, 28);
            _postImageOneDayEndBoundary = new DateTime(2021, 3, 2);

            _contactHomeNumber = "01334657489";
            _contactMobileNumber = "07983716207";
            _contactWorkNumber = "01223765485";
        }

        [Fact]
        public void InvalidPluginExceptionIsThrownWhenContentSettingRecordIdIsNotFound()
        {
            _context.Initialize(new List<Entity> { new Contact() { Id = _contactId } });
            _workflowContext = _context.GetDefaultWorkflowContext();
            _workflowContext.MessageName = "Create";
            var codeActivity = new GetContentSettingsFromPipeline();

            var entity = new msdyncrm_customerjourneycustomchannelactivity
            {
                msdyncrm_Contact = new EntityReference(Contact.EntityLogicalName, _contactId)
            };

            var postImage = new KeyValuePair<string, Entity>(msdyncrm_customerjourneycustomchannelactivity.EntityLogicalName, entity);
            _workflowContext.PostEntityImages.Add(postImage);

            try
            {
                var result = _context.ExecuteCodeActivity(_workflowContext, new Dictionary<string, object>(), codeActivity);
            }
            catch (InvalidPluginExecutionException exception)
            {
                Assert.True(exception.Message == "No context setting or contact id in pipeline.");
                return;
            }

            Assert.True(false);
        }

        [Fact]
        public void InvalidPluginExceptionIsThrownWhenContactRecordIdIsNotFound()
        {
            {
                _context.Initialize(new List<Entity> { new Contact() { Id = _contactId } });
                _workflowContext = _context.GetDefaultWorkflowContext();
                _workflowContext.MessageName = "Create";
                var codeActivity = new GetContentSettingsFromPipeline();

                var entity = new msdyncrm_customerjourneycustomchannelactivity
                {
                    msdyncrm_EntityId = _contentSettingId.ToString()
                };

                var postImage = new KeyValuePair<string, Entity>(msdyncrm_customerjourneycustomchannelactivity.EntityLogicalName, entity);
                _workflowContext.PostEntityImages.Add(postImage);

                try
                {
                    var result = _context.ExecuteCodeActivity(_workflowContext, new Dictionary<string, object>(), codeActivity);
                }
                catch (InvalidPluginExecutionException exception)
                {
                    Assert.True(exception.Message == "No context setting or contact id in pipeline.");
                    return;
                }

                Assert.True(false);
            }
        }

        [Fact]
        public void ValuesAreCorrectlySetAsOutParametersFromRetrievedContentSettingRecord()
        {
            var contentSettingRecord = new msdyncrm_contentsettings()
            {
                Id = _contentSettingId,
                dfe_1WeekReminderSMSTitle = _postImageOneWeekSmsTitle,
                dfe_1WeekReminderSMSMessage = _postImageOneWeekSmsMessage,
                dfe_1WeekReminderStartBoundary = _postImageOneWeekStartBoundary,
                dfe_1WeekReminderEndBoundary = _postImageOneWeekEndBoundary,
                dfe_1DayReminderSMSTitle = _postImageOneDaySmsTitle,
                dfe_1DayReminderSMSMessage = _postImageOneDaySmsMessage,
                dfe_1DayReminderStartBoundary = _postImageOneDayStartBoundary,
                dfe_1DayReminderEndBoundary = _postImageOneDayEndBoundary
            };

            _context.Initialize(new List<Entity> { contentSettingRecord, new Contact() { Id = _contactId } });
            _workflowContext = _context.GetDefaultWorkflowContext();
            _workflowContext.MessageName = "Create";
            var codeActivity = new GetContentSettingsFromPipeline();

            var entity = new msdyncrm_customerjourneycustomchannelactivity
            {
                msdyncrm_EntityId = _contentSettingId.ToString(),
                msdyncrm_Contact = new EntityReference(Contact.EntityLogicalName, _contactId)
            };

            var postImage = new KeyValuePair<string, Entity>(msdyncrm_customerjourneycustomchannelactivity.EntityLogicalName, entity);
            _workflowContext.PostEntityImages.Add(postImage);

            var result = _context.ExecuteCodeActivity(_workflowContext, new Dictionary<string, object>(), codeActivity);

            Assert.True((string)result["_oneWeekReminderTitle"] == _postImageOneWeekSmsTitle);
            Assert.True((string)result["_oneWeekReminderMessage"] == _postImageOneWeekSmsMessage);
            Assert.True((DateTime)result["_oneWeekReminderStartBoundary"] == _postImageOneWeekStartBoundary);
            Assert.True((DateTime)result["_oneWeekReminderEndBoundary"] == _postImageOneWeekEndBoundary);
            Assert.True((string)result["_oneDayReminderTitle"] == _postImageOneDaySmsTitle);
            Assert.True((string)result["_oneDayReminderMessage"] == _postImageOneDaySmsMessage);
            Assert.True((DateTime)result["_oneDayReminderStartBoundary"] == _postImageOneDayStartBoundary);
            Assert.True((DateTime)result["_oneDayReminderEndBoundary"] == _postImageOneDayEndBoundary);
        }

        [Fact]
        public void ValuesAreCorrectlySetAsOutParametersFromRetrievedContactRecord()
        {
            var contentSettingRecord = new msdyncrm_contentsettings()
            {
                Id = _contentSettingId,
                dfe_1WeekReminderSMSTitle = _postImageOneWeekSmsTitle,
                dfe_1WeekReminderSMSMessage = _postImageOneWeekSmsMessage,
                dfe_1WeekReminderStartBoundary = _postImageOneWeekStartBoundary,
                dfe_1WeekReminderEndBoundary = _postImageOneWeekEndBoundary,
                dfe_1DayReminderSMSTitle = _postImageOneDaySmsTitle,
                dfe_1DayReminderSMSMessage = _postImageOneDaySmsMessage,
                dfe_1DayReminderStartBoundary = _postImageOneDayStartBoundary,
                dfe_1DayReminderEndBoundary = _postImageOneDayEndBoundary
            };

            var contactRecord = new Contact()
            {
                Id = _contactId,
                Address1_Telephone1 = _contactHomeNumber,
                MobilePhone = _contactMobileNumber,
                Telephone1 = _contactWorkNumber
            };

            _context.Initialize(new List<Entity> { contentSettingRecord, contactRecord });
            _workflowContext = _context.GetDefaultWorkflowContext();
            _workflowContext.MessageName = "Create";
            var codeActivity = new GetContentSettingsFromPipeline();

            var entity = new msdyncrm_customerjourneycustomchannelactivity
            {
                msdyncrm_EntityId = _contentSettingId.ToString(),
                msdyncrm_Contact = new EntityReference(Contact.EntityLogicalName, _contactId)
            };

            var postImage = new KeyValuePair<string, Entity>(msdyncrm_customerjourneycustomchannelactivity.EntityLogicalName, entity);
            _workflowContext.PostEntityImages.Add(postImage);

            var result = _context.ExecuteCodeActivity(_workflowContext, new Dictionary<string, object>(), codeActivity);

            Assert.True((string)result["_homePhone"] == _contactHomeNumber);
            Assert.True((string)result["_mobilePhone"] == _contactMobileNumber);
            Assert.True((string)result["_workPhone"] == _contactWorkNumber);
        }

    }
    
}
