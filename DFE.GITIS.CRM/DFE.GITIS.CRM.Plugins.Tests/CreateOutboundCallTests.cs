using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using DFE.GITIS.CRM.ProxyClasses;

namespace DFE.GITIS.CRM.Plugins.Tests
{
    public class CreateOutboundCallTests
    {
        private const string RmpTalkingPoints = "* On Campus registrant: assess 1 to 1 adviser eligibility";
        private const string WebRegTalkingPoints = "* New registrant: assess 1 to 1 adviser eligibility";
        private const string CallBackTalkingPoints = "* Candidate requested call at this time: assess 1 to 1 adviser eligibility";
        private const string UcasTalkingPoints = "* New candidate, already registered with UCAS. Assess 1 to 1 adviser eligibility";
        private const string SesTalkingPoints = @"* Triage for TTA or EEA support
* Check candidate’s progress in gaining school experience";
        private const string TeachFirstTalkingPoints = "* Candidate couldn't get on a Teach First programme: assess 1 to 1 adviser eligibility";
        private const string SummerCampaignTalkingPoints = "* Summer campaign - Call to follow up registrant’s response to a Get Into Teaching email where they have told us they will be starting their UCAS application. The caller should ask if they need any advice on the application and offer TTA support (if eligible).";

        private const int RmpCallWeight = 90;
        private const int RmpEeaCallWeight = 80;
        private const int WebRegCallWeight = 90;
        private const int WebRegEeaCallWeight = 80;
        private const int UcasCallWeight = 70;
        private const int TeachFirstCallWeight = 60;
        private const int SummerCampaignCallWeight = 65;
        private const int SesCallWeight = 85;
        private const int CallBackCallWeight = 100;

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_CampusReg_2020()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750004; //University Campus Registrations-RMP
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22302);//2020
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(RmpTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(RmpCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_CampusReg_2022()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750004; //University Campus Registrations-RMP
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22304);//2022
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(RmpTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(RmpEeaCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_FreshersFair_2020()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750023;//Freshers Fair
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber,contactName, 22302);//2020
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(RmpTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(RmpCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());           

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_FreshersFair_2022()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750023;//Freshers Fair
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22304);//2022
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(RmpTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(RmpEeaCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_Ucas_2021()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750006;//Ucas
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22303);//2021
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(UcasTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(UcasCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_Ucas_2023()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750006;//Ucas
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22305);//2023
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(UcasTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(UcasCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_WebReg_2020()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750007;//WebReg
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22302);//2020
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(WebRegTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(WebRegCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_WebReg_2022()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750007;//WebReg
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22304);//2022
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(WebRegTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(WebRegEeaCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_SES_2021()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750021;//School Experience
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22303);//2021
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(SesTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(SesCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_SES_2023()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750021;//School Experience
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22305);//2023
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(SesTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(SesCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }

        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_TeachFirst_2021()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750014; // TeachFirst
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22303);//2021
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(TeachFirstTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(TeachFirstCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        [Fact]
        public void ShouldCreatePhoneCallWhenContactCreated_With_Channel_TeachFirst_2023()
        {
            Guid topicId = new Guid("946D8690-D9D3-E911-A981-000D3A23443B");
            var channelCreation = 222750014; // TeachFirst
            var ctx = new XrmFakedContext();

            var plugCtx = ctx.GetDefaultPluginContext();
            PopulateTestConfigValues(ctx);
            PopulateTestTopicRecord(ctx, channelCreation, topicId);
            var phoneNumber = "0121545856";
            var contactName = "Venkat UnitTest";
            PopulateContactDetails(plugCtx, channelCreation, phoneNumber, contactName, 22305);//2023
            ctx.ExecutePluginWith<CreateOutboundCall>(plugCtx);

            var phoneCalls = ctx.CreateQuery("phonecall").ToList();
            Assert.Single(phoneCalls);
            var phoneCall = phoneCalls[0];
            Assert.Equal(TeachFirstTalkingPoints, phoneCall["dfe_talkingpoints"].ToString());
            Assert.Equal(TeachFirstCallWeight, Convert.ToInt32(phoneCall["dfe_callweighting"]));
            Assert.Equal(phoneNumber, phoneCall["phonenumber"].ToString());
            Assert.Equal(1, Convert.ToInt32(phoneCall["dfe_callattemptnumber"]));
            Assert.True(Convert.ToBoolean(phoneCall["directioncode"]));
            Assert.False(Convert.ToBoolean(phoneCall["dfe_appointmentrequired"]));
            Assert.Equal(string.Format("Phone call to {0}", contactName), phoneCall["subject"].ToString());

            var callTopics = ctx.CreateQuery("dfe_calltopics").ToList();
            Assert.Single(callTopics);
            var callTopic = callTopics[0];
            Assert.Equal(topicId, ((EntityReference)callTopic["dfe_topic"]).Id);
            Assert.Equal(phoneCall.Id, ((EntityReference)callTopic["dfe_phonecall"]).Id);
        }
        private void PopulateContactDetails(XrmFakedPluginExecutionContext plugCtx, int channel,string phoneNumber, string contactName, int ittYear)
        {
            var contact = new Entity("contact");
            contact.Attributes.Add("dfe_channelcreation", new OptionSetValue(channel));
            contact.Attributes.Add("donotphone", false);
            contact.Attributes.Add("dfe_preferredphonenumbertype", new OptionSetValue(222750001));//Home Phone
            contact.Attributes.Add("address1_telephone1", phoneNumber);
            contact.Attributes.Add("fullname", contactName);
            contact.Attributes.Add("dfe_ittyear", new OptionSetValue(ittYear));

            plugCtx.InputParameters.Add("Target", contact);
        }

        private void PopulateTestConfigValues(XrmFakedContext context)
        {
            context.ProxyTypesAssembly = Assembly.GetAssembly(typeof(dfe_config));
            var service = context.GetOrganizationService();

            var config = new dfe_config()
            {
                dfe_name = "GITIS",
                dfe_latestprivacypolicyid = 1,                
                dfe_MaxNumberOfOutboundCallAttempts = 3,
                dfe_weightreductionforfollowups = "20",
                dfe_OutboundCallQueueGuid = "2488efef-4f10-e911-a99a-000d3a2065c5",
                dfe_outboundteamid = "2388EFEF-4F10-E911-A99A-000D3A2065C5",
                dfe_outcomecodesforfollowup = "222750002,222750007,222750008,222750009,222750010,222750011",

                dfe_WebRegistrationCallWeighting = WebRegCallWeight,
                dfe_webregistrationeeacallweighting = WebRegEeaCallWeight,
                dfe_rmpcallweigting = RmpCallWeight,
                dfe_rmpeeacallweigting = RmpEeaCallWeight,
                dfe_teachfirstcallweighting = TeachFirstCallWeight,
                dfe_UCASRegistrationCallWeighting = UcasCallWeight,
                dfe_CallbackRequestCallWeighting = CallBackCallWeight,
                dfe_SummerCampaignCallWeighting = SummerCampaignCallWeight,
                dfe_sescallweighting = SesCallWeight,

                dfe_TalkingPointsWebReg = WebRegTalkingPoints,
                dfe_talkingpointsrmp = RmpTalkingPoints,
                dfe_teachfirsttalkingpoints = TeachFirstTalkingPoints,
                dfe_TalkingPointsUCAS = UcasTalkingPoints,
                dfe_TalkingPointsCallbackRequest = CallBackTalkingPoints,
                dfe_talkingpointssummercampaign = SummerCampaignTalkingPoints,
                dfe_talkingpointsses = SesTalkingPoints,

                dfe_adviserclosedreasons= "222750000,222750004,222750005,222750006,222750007,222750008,222750009,222750010,222750011, 222750012,222750013,222750014,222750015,222750016,222750017,222750018,222750019,222750020,222750021,222750022,222750023,222750024,222750025,222750026,222750027,222750028,222750029,222750030,222750031,222750034,222750035",
                dfe_adviseropenreasons= "222750001,222750002,222750003,222750032,222750033,222750036",                
                dfe_ittadvisorstatuses= "222750000,222750005,222750027,222750010,222750034,222750013,222750014,222750021,222750022,222750035,222750029,222750025,222750026,222750030",
                dfe_outboundwebsitequeueguid= "1d3b4e34-87f2-ea11-a815-000d3a44afcc",
                dfe_outboundwebsiteteamguid= "2488efef-4f10-e911-a99a-000d3a2065c5",
                dfe_govnotifyapikey = "abc-4017adbd-d608-4eb6-ad5d-adb268faa23a-11664a21-62ff-4bcc-8219-c00815100fa7",
                dfe_govnotifyapiurl = "https://api.notifications.service.gov.uk/v2/notifications/email",
                dfe_websiteinboundprivacypolicyId = 12,
                dfe_websitecallbackrequestcallweighting = 100,
                dfe_websitebookacallbackcallweighting = 100,
                dfe_talkingpointswebsite = "* Candidate has commenced Get an Adviser form in Private Beta- clarify degree eligibility, complete NARIC check if necessary and complete remainder of triage to hand to adviser using new form",
                dfe_talkingpointsscholarships = "* Check still interested in teaching, attempt to triage for TTA or other transactions",
                dfe_sarsupportemail ="venkat.gannamani@education.gov.uk",
                dfe_scholashipsregistrationcallweighting=70                

            };

            var configId = service.Create(config);
            config.Id = configId;
            config.StateCode = dfe_configState.Active;
            config.StatusCode = new OptionSetValue(1);
            service.Update(config);
        }

        private void PopulateTestTopicRecord(XrmFakedContext context, int channel, Guid topicId)
        {
            context.ProxyTypesAssembly = Assembly.GetAssembly(typeof(dfe_topic));
            var service = context.GetOrganizationService();

            var topic = new dfe_topic()
            {
                dfe_sourcechannelvalue = channel,
                dfe_topicId = topicId
            };

            var topicGuid = service.Create(topic);
            topic.Id = topicGuid;
            topic.StateCode = dfe_topicState.Active;
            topic.StatusCode = new OptionSetValue(1);
            service.Update(topic);
        }
    }
}
