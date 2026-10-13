/*****************************************************************************************
 Author: Srikanth Nadella
 Date: 06-11-2025
 Description: This plugin is to check and merge contacts with gmail.com or googlemail.com domain.
 Create step: Post operation and Synchronous 
 **************************************************************************************/
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace DFE.GITIS.CRM.Plugins
{

    public class ContactGmailGooglemailCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target))
                return;

            // Only run when primary email is provided or updated
            string email = null;

            if (target.Attributes.Contains("emailaddress1"))
                email = target.GetAttributeValue<string>("emailaddress1");           

            email = email?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(email))
                return;

            // Identify domain type
            bool isGmail = email.EndsWith("@gmail.com");
            bool isGooglemail = email.EndsWith("@googlemail.com");

            if (!isGmail && !isGooglemail)
                return;

            var local = email.Split('@')[0];
            var gmailEmail = local + "@gmail.com";
            var googlemailEmail = local + "@googlemail.com";

            // CASE 1: Creating/Updating @gmail.com contact
            if (isGmail)
            {
                // Set secondary email
                target["emailaddress2"] = googlemailEmail;
                return;
            }

            // CASE 2: Creating/Updating @googlemail.com contact
            if (isGooglemail)
            {
                // Look for an existing gmail contact
                var query = new QueryExpression("contact")
                {
                    ColumnSet = new ColumnSet("contactid", "emailaddress1", "emailaddress2")
                };

                query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, gmailEmail);
                var results = service.RetrieveMultiple(query);

                if (results.Entities.Count > 0)
                {
                    // Gmail version already exists → update it
                    var gmailContact = results.Entities.First();                    
                    
                    var update = new Entity("contact", gmailContact.Id)
                    {
                        ["emailaddress1"] = gmailEmail,
                        ["emailaddress2"] = googlemailEmail
                    };                    
                    service.Update(update);

                    return;
                }
                else
                {
                    target["emailaddress1"] = gmailEmail;
                    target["emailaddress2"] = googlemailEmail;

                    return;
                }
            }
        }
    }
}
