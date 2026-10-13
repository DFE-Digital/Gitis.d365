///*****************************************************************************************
// Author: Venkat Gannamani
// Date: 24-02-2020
// Description: This plugin runs in post operation mode , To send a JSON message to smsqueue in Azure.
// Create step: Post operation and Asynchronous 
// **************************************************************************************/
//using System;
//using System.Collections.Generic;
//using Microsoft.Crm.Sdk.Messages;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Queue;
//using Microsoft.Xrm.Sdk;
//using Newtonsoft.Json;

//namespace DFE.GITIS.CRM.Plugins
//{
//    public class CreateSMSQueueItem : IPlugin
//    {
//        public void Execute(IServiceProvider serviceProvider)
//        {
//            if (serviceProvider == null)
//                throw new ArgumentNullException("serviceProvider");

//            // Obtain the execution context service from the service provider.
//            var PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

//            // Obtain the Organization Service factory service from the service provider
//            var ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

//            // Use the factory to generate the Organization Service.
//            var OrganizationService = ServiceFactory.CreateOrganizationService(null);

//            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

//            if (!string.Equals(PluginExecutionContext.MessageName.ToLower(), "create", StringComparison.InvariantCultureIgnoreCase))
//            {
//                return;
//            }
//            if (!PluginExecutionContext.InputParameters.Contains("Target") || PluginExecutionContext.InputParameters["Target"] as Entity == null)
//            {
//                return;
//            }
//            var TargetEntity = PluginExecutionContext.InputParameters["Target"] as Entity;
//            var recordId = PluginExecutionContext.PrimaryEntityId;
//            tracingService.Trace($"Record Id: {recordId}");
//            SendMessageToQueue(TargetEntity, recordId);
//        }

//        private void SendMessageToQueue(Entity smsRecord, Guid recordId)
//        {
//            var phoneNumber = smsRecord.GetAttributeValue<string>("dfe_mobilenumber");
//            if (string.IsNullOrEmpty(phoneNumber))
//            {
//                return;
//            }
//            phoneNumber = FormatPhoneNumber(phoneNumber);

//            Sms sms = GetSmsObject(smsRecord, phoneNumber, recordId);
//            //Serialise the object
//            var objText = JsonConvert.SerializeObject(sms);

//            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storageaccountrgt1d9e42;AccountKey=****;EndpointSuffix=core.windows.net");
//            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
//            var registrationQueue = queueClient.GetQueueReference("smsqueue");
//            try
//            {
//                registrationQueue.AddMessageAsync(new CloudQueueMessage(objText));
//            }
//            catch (Exception exception)
//            {
//                throw new InvalidPluginExecutionException($"{DateTime.Now} :: Exception: {exception.Message + objText}");
//            }
//        }

//        private Sms GetSmsObject(Entity smsRecord, string phoneNumber, Guid recordId)
//        {
//            return new Sms
//            {
//                PhoneNumber = phoneNumber,
//                Message = smsRecord.GetAttributeValue<string>("description"),
//                Id = recordId
//            };
//        }

//        private string FormatPhoneNumber(string phoneNumber)
//        {
//            if (phoneNumber.Substring(0, 2) == "44")
//            {
//                phoneNumber = "0" + phoneNumber.Substring(2, phoneNumber.Length - 2);
//            }
//            else if (phoneNumber.Substring(0, 3) == "044")
//            {
//                phoneNumber = "0" + phoneNumber.Substring(3, phoneNumber.Length - 3);
//            }
//            else if (phoneNumber.Substring(0, 4) == "0044")
//            {
//                phoneNumber = "0" + phoneNumber.Substring(4, phoneNumber.Length - 4);
//            }
//            else if (phoneNumber.Substring(0, 3) == "+44")
//            {
//                phoneNumber = "0" + phoneNumber.Substring(3, phoneNumber.Length - 3);
//            }
//            else if (phoneNumber.Substring(0, 1) != "0")
//            {
//                phoneNumber = "0" + phoneNumber;
//            }
//            return phoneNumber;
//        }
        
//    }

//    public class Sms
//    {
//        public Guid Id { get; set; }

//        public string PhoneNumber { get; set; }

//        public string Message { get; set; }

//        public string NotifyId { get; set; }

//        public string SmsDeliveryStatus { get; set; }
//    }
//}