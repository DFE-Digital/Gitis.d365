using DFE.GITIS.ADS.DIRECTSYNC.DTO;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V23.Common;
using Google.Ads.GoogleAds.V23.Enums;
using Google.Ads.GoogleAds.V23.Resources;
using Google.Ads.GoogleAds.V23.Services;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using static Google.Ads.GoogleAds.V23.Enums.OfflineUserDataJobStatusEnum.Types;
using static Google.Ads.GoogleAds.V23.Enums.OfflineUserDataJobTypeEnum.Types;

namespace DFE.GITIS.ADS.DIRECTSYNC.Helper;

public class GoogleAdsSyncService
{
    private readonly ILogger _log;

    public GoogleAdsSyncService(ILogger log)
    {
        _log = log;
    }

    public async Task<SyncResult> SyncCustomerMatchAsync(ContactPayload contact)
    {
        try
        {
            var client = new GoogleAdsClient(BuildConfig());
            var customerId = Env("GOOGLE_CUSTOMER_ID");
            var userListId = Env("GOOGLE_USER_LIST_ID");

            var userListResourceName =
                $"customers/{customerId}/userLists/{userListId}";

            // Build the hashed user data operations
            var operations = BuildOperations(contact);

            if (!operations.Any())
            {
                _log.LogWarning(
                    "Google Ads: contact {Id} has no hashable identifiers — skipping",
                    contact.ContactId);
                return Fail("Google", "No hashable identifiers (email, phone, or name)");
            }

            var jobService = client.GetService(Services.V23.OfflineUserDataJobService);

            // 1. Create the offline user data job
            var createJobRequest = new CreateOfflineUserDataJobRequest
            {
                CustomerId = customerId,
                Job = new OfflineUserDataJob
                {
                    Type = OfflineUserDataJobType.CustomerMatchUserList,
                    CustomerMatchUserListMetadata = new CustomerMatchUserListMetadata
                    {
                        UserList = userListResourceName
                    }
                }
            };

            var createResponse = await jobService.CreateOfflineUserDataJobAsync(createJobRequest);
            var jobResourceName = createResponse.ResourceName;

            _log.LogDebug("Google Ads: created job {Job} for contact {Id}",
                jobResourceName, contact.ContactId);

            // 2. Add operations to the job
            var addOpsRequest = new AddOfflineUserDataJobOperationsRequest
            {
                ResourceName = jobResourceName,
                Operations = { operations },
                EnableWarnings = true
            };

            await jobService.AddOfflineUserDataJobOperationsAsync(addOpsRequest);

            // 3. Run the job (async — Google processes it in the background)
            await jobService.RunOfflineUserDataJobAsync(jobResourceName);

            _log.LogInformation(
                "Google Ads: job {Job} submitted for contact {Id} — {Count} operation(s)",
                jobResourceName, contact.ContactId, operations.Count);

            return new SyncResult
            {
                Platform = "Google",
                Success = true,
                AudienceId = userListId,
                AudienceUrl = $"https://ads.google.com/aw/audiences/details?userListId={userListId}",
                MatchStatus = $"{operations.Count} identifier(s) submitted",
                SyncedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Google Ads sync failed for contact {Id}", contact.ContactId);
            return Fail("Google", ex.Message);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static GoogleAdsConfig BuildConfig() => new()
    {
        DeveloperToken = Env("GOOGLE_DEVELOPER_TOKEN"),
        OAuth2ClientId = Env("GOOGLE_CLIENT_ID"),
        OAuth2ClientSecret = Env("GOOGLE_CLIENT_SECRET"),
        OAuth2RefreshToken = Env("GOOGLE_REFRESH_TOKEN"),
        LoginCustomerId = Env("GOOGLE_LOGIN_CUSTOMER_ID")
    };

    private static List<OfflineUserDataJobOperation> BuildOperations(ContactPayload contact)
    {
        var userIdentifiers = new List<UserIdentifier>();

        // Email — highest match rate
        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            userIdentifiers.Add(new UserIdentifier
            {
                HashedEmail = HashHelper.Sha256(contact.Email.Trim().ToLowerInvariant())
            });
        }

        // Phone — normalised to E.164 before hashing
        if (!string.IsNullOrWhiteSpace(contact.Phone))
        {
            userIdentifiers.Add(new UserIdentifier
            {
                HashedPhoneNumber = HashHelper.HashPhone(contact.Phone)
            });
        }

        // Name + address — lower match rate but increases coverage
        if (!string.IsNullOrWhiteSpace(contact.FirstName) &&
            !string.IsNullOrWhiteSpace(contact.LastName))
        {
            userIdentifiers.Add(new UserIdentifier
            {
                AddressInfo = new OfflineUserAddressInfo
                {
                    HashedFirstName = HashHelper.Sha256(contact.FirstName.Trim().ToLowerInvariant()),
                    HashedLastName = HashHelper.Sha256(contact.LastName.Trim().ToLowerInvariant()),
                    PostalCode = contact.ZipCode ?? string.Empty,
                    CountryCode = contact.CountryCode ?? "GB"
                }
            });
        }

        if (!userIdentifiers.Any())
            return new List<OfflineUserDataJobOperation>();

        return new List<OfflineUserDataJobOperation>
        {
            new()
            {
                Create = new UserData
                {
                    UserIdentifiers = { userIdentifiers }
                }
            }
        };
    }

    private static SyncResult Fail(string platform, string message) =>
        new() { Platform = platform, Success = false, ErrorMessage = message, SyncedAt = DateTime.UtcNow };

    private static string Env(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException($"Missing environment variable: {key}");
}