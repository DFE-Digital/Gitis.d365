using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Xunit;

namespace DFE.GITIS.CRM.Plugins.Tests
{
    public class UpdateContactOwnerTests
    {
        [Fact]
        public void ShouldUpdateContactDetails()
        {
            var fakedContext = new XrmFakedContext();
            var plugCtx = fakedContext.GetDefaultPluginContext();
            var service = fakedContext.GetOrganizationService();

            var createContact = new Entity("contact");
            createContact["firstname"] = "UnittestContactOwnerUpdatefirstname";
            createContact["lastname"] = "UnittestContactOwnerUpdatelastname";
            createContact["dfe_dateassignedtoadvisor"] = new DateTime(2019, 11, 1);
            createContact["ownerid"] = new EntityReference("systemuser", new Guid("D29FE355-A9F9-E911-A814-000D3A44A6C8"));
            createContact["dfe_isadvisorrequiredos"] = new OptionSetValue(222750001);
            createContact["dfe_candidatestatus"] = new OptionSetValue(222750002);
            createContact["dfe_previousowner"] = new EntityReference("systemuser", new Guid("9BF31D96-FEE7-E811-A97F-000D3A276620"));
            createContact["dfe_dateassignedtopreviousowner"] = new DateTime(2019, 10, 1);

            var contact = service.Create(createContact);

            fakedContext.ExecutePluginWith<UpdateContactOwner>(plugCtx);

            var newContacts = fakedContext.CreateQuery("contact").ToList();
            Assert.Single(newContacts);
            var newContact = newContacts[0];

            Assert.Equal("UnittestContactOwnerUpdatefirstname", newContact["firstname"].ToString());
            Assert.Equal("UnittestContactOwnerUpdatelastname", newContact["lastname"].ToString());
            Assert.Equal(new Guid("D29FE355-A9F9-E911-A814-000D3A44A6C8"), ((EntityReference)newContact["ownerid"]).Id);
            Assert.Equal(222750002, ((OptionSetValue)newContact["dfe_candidatestatus"]).Value);
            Assert.Equal(222750001, ((OptionSetValue)newContact["dfe_isadvisorrequiredos"]).Value);           
            Assert.Equal(new DateTime(2019, 10, 1), newContact["dfe_dateassignedtopreviousowner"]);
            Assert.Equal(new DateTime(2019, 11, 1), newContact["dfe_dateassignedtoadvisor"]);
            Assert.Equal(new Guid("9BF31D96-FEE7-E811-A97F-000D3A276620"), ((EntityReference)newContact["dfe_previousowner"]).Id);
        }
    }
}
