/*****************************************************************************************
 Author: Monjurul Naim
 Date: 07-09-2020
 Revised: Removed Microsoft.IdentityModel.Tokens dependency (sandbox assembly load failure),
          fixed base64url signature encoding and null-deref in catch block.
 Description: This Action outputs a JWT which can be used from an http request to GovNotify
 **************************************************************************************/

using System;
using System.Activities;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;


namespace DFE.GITIS.CRM.Workflows
{
    public class SendEmailToNotify : CodeActivity
    {

        [Input("PostData")]
        public InArgument<string> postData { get; set; }

        [Output("EmailSendResponse")]
        public OutArgument<string> emailSendResponse { get; set; }

        string responseText = "";
        string token = "";
        string requestBody = null;

        string responseMsg = "";
        bool success = false;

        GovNotifyAPISettings gnApiSettings = null;

        protected override void Execute(CodeActivityContext context)
        {
            // Getting OrganizationService from Context  
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            ITracingService tracer = context.GetExtension<ITracingService>();

            try
            {
                requestBody = postData.Get(context);

                gnApiSettings = GetGovNotifyKeyFromConfig(orgService);

                if (gnApiSettings == null)
                {
                    responseMsg = "No Gov.Notify Config settings found";
                    success = false;
                }
                else
                {
                    token = GetJWTToken(gnApiSettings);
                    responseMsg = SendRequestGovNotify_Email(requestBody, token);

                    if (success)
                    {
                        responseMsg = "Email sent successfully";
                    }
                    else
                    {
                        success = false;
                        responseMsg = "Error sending email";
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                responseMsg = " \"" + ex.ToString() + "\"";
                responseMsg += gnApiSettings?.Url;
            }
            finally
            {
                responseText = "{\"ResponseMessage\":\"" + responseMsg + "\",\"Success\":\"" + success.ToString() + "\"}";
                emailSendResponse.Set(context, responseText);
            }

        }

        private GovNotifyAPISettings GetGovNotifyKeyFromConfig(IOrganizationService orgService)
        {
            GovNotifyAPISettings govNotifiyApi = null;

            string fetchString = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='dfe_config'>
                                        <attribute name='dfe_configid' />
                                        <attribute name='dfe_name' />
                                        <attribute name='dfe_govnotifyapiurl' />
                                        <attribute name='dfe_govnotifyapikey' />
                                        <order attribute='dfe_name' descending='false' />
                                        <filter type='and'>
                                          <condition attribute='dfe_name' operator='eq' value='GITIS' />
                                        </filter>
                                      </entity>
                                    </fetch>";

            var eCol = orgService.RetrieveMultiple(new FetchExpression(fetchString));

            if (eCol == null || eCol?.Entities == null || eCol.Entities.Count == 0) return null;

            var configEntity = eCol.Entities[0];

            if (configEntity.Attributes.Contains("dfe_govnotifyapikey"))
            {
                string apiKey = configEntity["dfe_govnotifyapikey"].ToString();

                if (apiKey.Length > 75)
                {
                    govNotifiyApi = new GovNotifyAPISettings(apiKey);
                }
                else return null;
            }
            if (govNotifiyApi != null && configEntity.Attributes.Contains("dfe_govnotifyapiurl"))
            {
                govNotifiyApi.Url = configEntity["dfe_govnotifyapiurl"].ToString();
            }
            else return null;

            return govNotifiyApi;
        }

        private string GetJWTToken(GovNotifyAPISettings govNotifyAPISettings)
        {
            var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(govNotifyAPISettings.Header));
            var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(govNotifyAPISettings.Payload));

            var encodedHeaderPayload = encodedHeader + "." + encodedPayload;
            var signature = GetSignature(encodedHeaderPayload, govNotifyAPISettings.Key);

            return encodedHeaderPayload + "." + signature;
        }

        private string GetSignature(string encodedHeaderPayload, string Key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(Key);
            using (HMACSHA256 hmacSha256 = new HMACSHA256(keyByte))
            {
                byte[] dataToHmac = Encoding.UTF8.GetBytes(encodedHeaderPayload);
                return Base64UrlEncode(hmacSha256.ComputeHash(dataToHmac));
            }
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private class GovNotifyAPISettings
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Key { get; set; }
            public string Url { get; set; }
            public string Header { get; private set; }
            public string Payload { get; private set; }

            public GovNotifyAPISettings(string GovNotifyApiLongKeyString)
            {
                if (GovNotifyApiLongKeyString.Length < 75) return;

                Header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";

                Name = GovNotifyApiLongKeyString.Substring(0, GovNotifyApiLongKeyString.Length - 74);
                Key = GovNotifyApiLongKeyString.Substring(GovNotifyApiLongKeyString.Length - 36);
                Id = GovNotifyApiLongKeyString.Substring(GovNotifyApiLongKeyString.Length - 73).Replace(Key, "");
                Id = Id.Substring(0, Id.Length - 1);

                Int32 timeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                Payload = "{\"iss\":\"" + Id + "\",\"iat\":" + timeStamp + "}";
            }
        }

        private string SendRequestGovNotify_Email(string requestBody, string jwtToken)
        {
            string responseText = "";

            var url = gnApiSettings.Url;

            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.UTF8.GetBytes(requestBody);
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            request.Proxy = null;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                responseText = new StreamReader(response.GetResponseStream()).ReadToEnd();
                success = true;
            }
            catch (WebException webex)
            {
                responseText = new StreamReader(webex.Response.GetResponseStream()).ReadToEnd();
                responseText += webex.ToString();
                success = false;
            }

            return responseText;
        }
    }
}