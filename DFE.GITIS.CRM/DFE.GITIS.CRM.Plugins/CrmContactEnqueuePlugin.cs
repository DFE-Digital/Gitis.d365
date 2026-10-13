using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.CRM.Plugins
{
    /// <summary>
    /// Dynamics 365 / Dataverse plugin registered on Contact Create and Update.
    /// Post-operation, Asynchronous.
    ///
    /// Sends a message to Azure Service Bus using the REST API (HTTP) only —
    /// no Azure.Messaging.ServiceBus assembly required. The only dependency
    /// is Microsoft.CrmSdk.CoreAssemblies.
    ///
    /// Service Bus REST API:
    ///   POST https://{namespace}.servicebus.windows.net/{queue}/messages
    ///   Authorization: SharedAccessSignature (SAS token generated inline)
    ///
    /// Plugin Registration Tool settings:
    ///   Entity:     contact
    ///   Messages:   Create, Update
    ///   Stage:      Post-operation (40)
    ///   Execution:  Asynchronous
    ///   Deployment: Server
    ///
    /// Unsecure configuration — paste the connection string exactly as shown in
    /// Azure Portal > Service Bus > Shared Access Policies > Connection String:
    ///
    ///   Endpoint=sb://yournamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=yourkey==;EntityPath=crm-contact-sync
    ///
    /// EntityPath is optional — defaults to "crm-contact-sync" if omitted.
    /// </summary>
    public class CrmContactEnqueuePlugin : IPlugin
    {
        private readonly string _namespace;
        private readonly string _queueName;
        private readonly string _sasKeyName;
        private readonly string _sasKey;

        public CrmContactEnqueuePlugin(string unsecureConfig, string secureConfig)
        {
            if (!string.IsNullOrWhiteSpace(unsecureConfig))
            {
                var pairs = ParseConfig(unsecureConfig);

                // Extract namespace from Endpoint=sb://yournamespace.servicebus.windows.net/
                string endpoint;
                if (pairs.TryGetValue("Endpoint", out endpoint) && !string.IsNullOrWhiteSpace(endpoint))
                {
                    // Strip "sb://" scheme and trailing slash, then take the first segment
                    var host = endpoint
                        .Replace("sb://", "")
                        .Replace("Sb://", "")
                        .Replace("SB://", "")
                        .TrimEnd('/');
                    var dotIndex = host.IndexOf('.');
                    _namespace = dotIndex > 0 ? host.Substring(0, dotIndex) : host;
                }

                pairs.TryGetValue("SharedAccessKeyName", out _sasKeyName);
                pairs.TryGetValue("SharedAccessKey", out _sasKey);

                // EntityPath in the connection string carries the queue name
                string entityPath;
                _queueName = (pairs.TryGetValue("EntityPath", out entityPath) && !string.IsNullOrWhiteSpace(entityPath))
                    ? entityPath
                    : "crm-contact-sync";
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var serviceFactory = (IOrganizationServiceFactory)
                serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var tracer = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            try
            {
                // C# 7.3: use 'is' pattern without negation
                var entity = context.InputParameters["Target"] as Entity;
                if (entity == null)
                {
                    tracer.Trace("Target is not an Entity — skipping.");
                    return;
                }

                var orgService = serviceFactory.CreateOrganizationService(context.UserId);

                var contact = orgService.Retrieve(
                    "contact",
                    entity.Id,
                    new Microsoft.Xrm.Sdk.Query.ColumnSet(
                        "emailaddress1",
                        "firstname",
                        "lastname",
                        "mobilephone",
                        "address1_postalcode",
                        "address1_country",
                        "birthdate"
                    )
                );

                var email = contact.GetAttributeValue<string>("emailaddress1");
                var phone = contact.GetAttributeValue<string>("mobilephone");

                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                {
                    tracer.Trace("Contact " + contact.Id + " has no email or phone — not enqueuing.");
                    return;
                }

                var json = BuildJson(contact, context.MessageName);

                SendToServiceBus(json, contact.Id.ToString(), tracer)
                    .GetAwaiter()
                    .GetResult();

                tracer.Trace("Contact " + contact.Id + " enqueued successfully.");
            }
            catch (Exception ex)
            {
                tracer.Trace("CrmEnqueuePlugin error: " + ex);
                throw new InvalidPluginExecutionException(
                    OperationStatus.Failed,1,
                    "CrmAdSync.Plugin failed to enqueue contact: " + ex.Message
                    );
            }
        }

        // ── JSON ──────────────────────────────────────────────────────────────
        // Hand-rolled to avoid any JSON assembly dependency in the plugin sandbox.

        private static string BuildJson(Entity contact, string messageName)
        {
            var contactId = contact.Id.ToString();
            var email = EscapeJson(contact.GetAttributeValue<string>("emailaddress1"));
            var firstName = EscapeJson(contact.GetAttributeValue<string>("firstname"));
            var lastName = EscapeJson(contact.GetAttributeValue<string>("lastname"));
            var phone = EscapeJson(contact.GetAttributeValue<string>("mobilephone"));
            var zipCode = EscapeJson(contact.GetAttributeValue<string>("address1_postalcode"));
            var rawCountry = contact.GetAttributeValue<string>("address1_country");
            var country = EscapeJson(rawCountry != null ? rawCountry : "GB");
            var trigger = EscapeJson(messageName);
            var enqueuedAt = DateTime.UtcNow.ToString("o");

            var rawDob = contact.GetAttributeValue<DateTime?>("birthdate");
            var dob = rawDob.HasValue ? rawDob.Value.ToString("yyyyMMdd") : "";

            return "{"
                 + "\"ContactId\":\"" + contactId + "\","
                 + "\"Email\":" + JsonString(email) + ","
                 + "\"FirstName\":" + JsonString(firstName) + ","
                 + "\"LastName\":" + JsonString(lastName) + ","
                 + "\"Phone\":" + JsonString(phone) + ","
                 + "\"ZipCode\":" + JsonString(zipCode) + ","
                 + "\"CountryCode\":\"" + country + "\","
                 + "\"DateOfBirth\":\"" + dob + "\","
                 + "\"TriggerSource\":\"" + trigger + "\","
                 + "\"EnqueuedAt\":\"" + enqueuedAt + "\""
                 + "}";
        }

        private static string JsonString(string value)
        {
            return value == null ? "null" : "\"" + value + "\"";
        }

        private static string EscapeJson(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        // ── Service Bus HTTP REST ─────────────────────────────────────────────
        // Docs: https://learn.microsoft.com/en-us/rest/api/servicebus/send-message-to-queue

        private async Task SendToServiceBus(string json, string contactId, ITracingService tracer)
        {
            if (string.IsNullOrWhiteSpace(_namespace))
                throw new InvalidOperationException("Endpoint not found in unsecure config connection string.");
            if (string.IsNullOrWhiteSpace(_sasKeyName))
                throw new InvalidOperationException("SharedAccessKeyName not found in unsecure config connection string.");
            if (string.IsNullOrWhiteSpace(_sasKey))
                throw new InvalidOperationException("SharedAccessKey not found in unsecure config connection string.");

            var queue = string.IsNullOrWhiteSpace(_queueName) ? "crm-contact-sync" : _queueName;
            var uri = "https://" + _namespace + ".servicebus.windows.net/" + queue + "/messages";
            var sasToken = GenerateSasToken(uri, _sasKeyName, _sasKey, 300);

            using (var http = new HttpClient())
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("SharedAccessSignature", sasToken);

                // BrokerProperties: MessageId enables duplicate detection, TimeToLiveTimeSpan sets TTL
                var ttlSeconds = (long)TimeSpan.FromHours(24).TotalSeconds;
                http.DefaultRequestHeaders.TryAddWithoutValidation(
                    "BrokerProperties",
                    "{\"MessageId\":\"" + contactId + "\",\"TimeToLiveTimeSpan\":\"" + ttlSeconds + "\"}");

                var response = await http.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException(
                        "Service Bus returned " + (int)response.StatusCode + ": " + body);
                }
            }

            tracer.Trace("Service Bus HTTP 201. MessageId=" + contactId + ", Queue=" + queue);
        }

        // ── SAS token (HMAC-SHA256, BCL only) ────────────────────────────────
        // https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-sas

        private static string GenerateSasToken(
            string resourceUri,
            string keyName,
            string key,
            int expirySeconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var sinceEpoch = DateTime.UtcNow - epoch;
            var expiry = Convert.ToUInt32(sinceEpoch.TotalSeconds) + expirySeconds;

            var encodedUri = Uri.EscapeDataString(resourceUri);
            var stringToSign = encodedUri + "\n" + expiry;

            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                signature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            }

            return "sr=" + encodedUri
                 + "&sig=" + Uri.EscapeDataString(signature)
                 + "&se=" + expiry
                 + "&skn=" + keyName;
        }

        // ── Config parser ─────────────────────────────────────────────────────

        private static Dictionary<string, string> ParseConfig(string config)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var part in config.Split(';'))
            {
                var idx = part.IndexOf('=');
                if (idx > 0)
                {
                    var k = part.Substring(0, idx).Trim();
                    var v = part.Substring(idx + 1).Trim();
                    result[k] = v;
                }
            }

            return result;
        }
    }
}