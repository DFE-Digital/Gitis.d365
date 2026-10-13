using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Web.Script.Serialization;
using System.Runtime.Remoting;
using Newtonsoft.Json;
using Microsoft.Xrm.Sdk;
using System.Runtime.Remoting.Services;

namespace DFE.GITIS.CRM.CustomApis.GovNotify
{
    internal class SMS
    {
        public string phone_number { get; set; }

        public string template_id { get; set; }

        public Dictionary<string, string> personalisation { get; set; }

    }
    public class Content
    {
        public string body { get; set; }
        public string from_number { get; set; }
    }

    public class SMSResponse
    {
        public string id { get; set; }
        public string reference { get; set; }
        public Content content { get; set; }
        public string uri { get; set; }
        public Template template { get; set; }
    }

    public class Template
    {
        public string id { get; set; }
        public int version { get; set; }
        public string uri { get; set; }
    }

    public class GovNotifySMS
    {
        private ITracingService _tracingService { get; set; }
        public GovNotifySMS(ITracingService tracingService)
        {
            _tracingService = tracingService;
        }
        internal static SMS CreateRequestParams(string templateId, Dictionary<string, string> personalisation, string mobileNumber)
        {

            var req = new SMS

            {

                template_id = templateId,

                personalisation = personalisation,

                phone_number = mobileNumber,

            };

            return req;

        }


        public SMSResponse Send(string mobileNumber, string msgContent, string templateid, string[] keys)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.notifications.service.gov.uk")
            };

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var content = new Dictionary<string, string>(){
                {"message", msgContent }
            };

            var httpResponse =  SendSmsAsync(client, mobileNumber, templateid, content, keys);

            _tracingService.Trace($"httpResponse: {httpResponse.Content.ReadAsStringAsync().Result}");

            return JsonConvert.DeserializeObject<SMSResponse>(httpResponse.Content.ReadAsStringAsync().Result);

        }

        internal HttpResponseMessage SendSmsAsync(HttpClient client, string mobileNumber, string templateId, Dictionary<string, string> personalisation, string[] keys)
        {

            var param = GovNotifySMS.CreateRequestParams(templateId, personalisation, mobileNumber);

            var jsonParams = new JavaScriptSerializer().Serialize(param);

            var request = new HttpRequestMessage(HttpMethod.Post, "/v2/notifications/sms");

            var content = new StringContent(jsonParams, Encoding.UTF8, "application/json");

            var token = new JWT().GenerateToken(keys[1], keys[0]);

            _tracingService.Trace(token);

            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);

            request.Content = content;

            return client.SendAsync(request).Result;

        }
    }
}
