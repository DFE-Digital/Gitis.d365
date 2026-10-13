using DFE.GITIS.ADS.DIRECTSYNC.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using static Grpc.Core.Metadata;

namespace DFE.GITIS.ADS.DIRECTSYNC.Helper
{


    public class CrmWritebackService
    {
        private readonly ILogger _log;

        public CrmWritebackService(ILogger log)
        {
            _log = log;
        }

        public async Task UpdateContactAsync(
            string contactId,
            SyncResult googleResult,
            SyncResult facebookResult)
        {
            try
            {
                var connectionString = Env("DATAVERSE_CONNECTION_STRING");

                using var client = new ServiceClient(connectionString);

                if (!client.IsReady)
                    throw new InvalidOperationException(
                        $"Dataverse connection not ready: {client.LastError}");

                var update = new Entity("contact", Guid.Parse(contactId));

                // ── Google fields ──────────────────────────────────────────────
                update["cr_google_sync_status"] = googleResult.Success
                    ? "Synced"
                    : $"Failed: {Truncate(googleResult.ErrorMessage, 250)}";

                update["cr_google_audience_id"] = googleResult.AudienceId ?? string.Empty;
                update["cr_google_audience_url"] = googleResult.AudienceUrl ?? string.Empty;
                update["cr_google_match_status"] = googleResult.MatchStatus ?? string.Empty;
                update["cr_google_last_synced"] = googleResult.SyncedAt;

                // ── Facebook fields ────────────────────────────────────────────
                update["cr_facebook_sync_status"] = facebookResult.Success
                    ? "Synced"
                    : $"Failed: {Truncate(facebookResult.ErrorMessage, 250)}";

                update["cr_facebook_audience_id"] = facebookResult.AudienceId ?? string.Empty;
                update["cr_facebook_audience_url"] = facebookResult.AudienceUrl ?? string.Empty;
                update["cr_facebook_match_status"] = facebookResult.MatchStatus ?? string.Empty;
                update["cr_facebook_last_synced"] = facebookResult.SyncedAt;

                // ── Summary ────────────────────────────────────────────────────
                update["cr_last_ad_sync"] = DateTime.UtcNow;

                // Use ExecuteAsync for non-blocking update
                await Task.Run(() => client.Update(update));

                _log.LogInformation(
                    "CRM write-back complete for contact {Id} — Google: {G}, Facebook: {F}",
                    contactId,
                    googleResult.Success ? "OK" : "FAIL",
                    facebookResult.Success ? "OK" : "FAIL");
            }
            catch (Exception ex)
            {
                // Write-back failure must NOT throw — the sync already happened.
                // Log the error; the dead-letter handler will surface it.
                _log.LogError(ex,
                    "CRM write-back failed for contact {Id} — sync results may not be visible in CRM",
                    contactId);
            }
        }

        private static string? Truncate(string? value, int maxLength) =>
            value is null ? null
            : value.Length <= maxLength ? value
            : value[..maxLength];

        private static string Env(string key) =>
            Environment.GetEnvironmentVariable(key)
            ?? throw new InvalidOperationException($"Missing environment variable: {key}");
    }
}