using System.Net.Http.Json;
using System.Text.Json;
using DFE.GITIS.ADS.DIRECTSYNC.DTO;
using DFE.GITIS.ADS.DIRECTSYNC.Helper;
using Microsoft.Extensions.Logging;

namespace DFE.GITIS.ADS.DIRECTSYNC.Helper;

public class FacebookAdsSyncService
{
    private static readonly HttpClient _http = new();
    private const string ApiVersion = "v19.0";
    private readonly ILogger _log;

    public FacebookAdsSyncService(ILogger log)
    {
        _log = log;
    }

    public async Task<SyncResult> SyncCustomAudienceAsync(ContactPayload contact)
    {
        try
        {
            var accessToken = Env("FB_ACCESS_TOKEN");
            var adAccountId = Env("FB_AD_ACCOUNT_ID");   // act_XXXXXXXXX
            var audienceId = Env("FB_CUSTOM_AUDIENCE_ID");

            var (schema, row) = BuildSchemaAndRow(contact);

            if (!schema.Any())
            {
                _log.LogWarning(
                    "Facebook: contact {Id} has no hashable identifiers — skipping",
                    contact.ContactId);

                return Fail("Facebook", "No hashable identifiers (email, phone, or name)");
            }

            var payload = new
            {
                schema,
                data = new[] { row }
            };

            var url = $"https://graph.facebook.com/{ApiVersion}/{audienceId}/users"
                     + $"?access_token={accessToken}";

            var body = JsonContent.Create(new { payload });

            var httpResponse = await _http.PostAsync(url, body);
            var raw = await httpResponse.Content.ReadAsStringAsync();

            _log.LogDebug("Facebook API response: {Body}", raw);

            using var json = JsonDocument.Parse(raw);
            var root = json.RootElement;

            if (root.TryGetProperty("error", out var error))
            {
                var msg = error.TryGetProperty("message", out var m) ? m.GetString() : raw;
                throw new InvalidOperationException($"Facebook API error: {msg}");
            }

            var numReceived = root.TryGetProperty("num_received", out var nr) ? nr.GetInt32() : 0;
            var numInvalidEntries = root.TryGetProperty("num_invalid_entries", out var ni) ? ni.GetInt32() : 0;
            var numMatched = numReceived - numInvalidEntries;

            _log.LogInformation(
                "Facebook: contact {Id} synced — {Received} received / {Matched} matched",
                contact.ContactId, numReceived, numMatched);

            return new SyncResult
            {
                Platform = "Facebook",
                Success = true,
                AudienceId = audienceId,
                AudienceUrl = $"https://business.facebook.com/adsmanager/audiences"
                            + $"?act={adAccountId}&audience_id={audienceId}",
                MatchStatus = $"{numReceived} received / {numMatched} matched",
                SyncedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Facebook sync failed for contact {Id}", contact.ContactId);
            return Fail("Facebook", ex.Message);
        }
    }

    // ── Schema builder ───────────────────────────────────────────────────────
    // Facebook requires parallel schema[] and data[][] arrays.
    // Both email and phone must be hashed. Name fields must be lower-case then hashed.
    // See: https://developers.facebook.com/docs/marketing-api/audiences/guides/custom-audiences

    private static (List<string> schema, List<string> row) BuildSchemaAndRow(ContactPayload contact)
    {
        var schema = new List<string>();
        var row = new List<string>();

        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            schema.Add("EMAIL");
            row.Add(HashHelper.Sha256(contact.Email.Trim().ToLowerInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(contact.Phone))
        {
            schema.Add("PHONE");
            row.Add(HashHelper.HashPhoneForFacebook(contact.Phone));
        }

        if (!string.IsNullOrWhiteSpace(contact.FirstName))
        {
            schema.Add("FN");
            row.Add(HashHelper.Sha256(contact.FirstName.Trim().ToLowerInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(contact.LastName))
        {
            schema.Add("LN");
            row.Add(HashHelper.Sha256(contact.LastName.Trim().ToLowerInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(contact.ZipCode))
        {
            schema.Add("ZIP");
            row.Add(HashHelper.Sha256(contact.ZipCode.Trim().ToLowerInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(contact.DateOfBirth) && contact.DateOfBirth.Length >= 4)
        {
            // Facebook DOBY = 4-digit birth year
            schema.Add("DOBY");
            row.Add(HashHelper.Sha256(contact.DateOfBirth[..4]));
        }

        if (!string.IsNullOrWhiteSpace(contact.CountryCode))
        {
            schema.Add("COUNTRY");
            row.Add(HashHelper.Sha256(contact.CountryCode.Trim().ToLowerInvariant()));
        }

        return (schema, row);
    }

    private static SyncResult Fail(string platform, string message) =>
        new() { Platform = platform, Success = false, ErrorMessage = message, SyncedAt = DateTime.UtcNow };

    private static string Env(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException($"Missing environment variable: {key}");
}
