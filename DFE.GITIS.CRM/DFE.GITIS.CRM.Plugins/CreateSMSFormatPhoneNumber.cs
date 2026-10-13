/*****************************************************************************************
 Author: Venkat Gannamani
 Date: 18-05-2020
 Description: This plugin runs in pre operation mode , To format the mobile number before creating SMS record.
 Create step: Pre operation and Synchronous 
 **************************************************************************************/
using System;
using Microsoft.Xrm.Sdk;

namespace DFE.GITIS.CRM.Plugins
{
    public class CreateSMSFormatPhoneNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            // Obtain the execution context service from the service provider.
            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the Organization Service factory service from the service provider
            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the Organization Service.
            var OrganizationService = ServiceFactory.CreateOrganizationService(null);

            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (!string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
            {
                return;
            }
            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
            var recordId = PluginExecutionContext.PrimaryEntityId;
            tracingService.Trace($"Record Id: {recordId}");
            var phoneNumber = TargetEntity.GetAttributeValue<string>("dfe_mobilenumber");
            tracingService.Trace($"Mobile number before format: {phoneNumber}");
            phoneNumber = FormatPhoneNumber(phoneNumber);
            tracingService.Trace($"Mobile number after format: {phoneNumber}");

            if (phoneNumber.Length != 11)
                throw new InvalidPluginExecutionException("Phone number should be 11 digits in length.");

            TargetEntity["dfe_mobilenumber"] = phoneNumber;
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            phoneNumber = phoneNumber.Replace(" ", "");

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
            return phoneNumber;
        }
    }
}