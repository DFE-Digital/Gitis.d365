using System.Security.Cryptography;
using System.Text;

namespace DFE.GITIS.ADS.DIRECTSYNC.Helper;

/// <summary>
/// Provides SHA-256 hashing utilities.
/// Both Google Ads Customer Match and Facebook Custom Audience
/// require PII to be SHA-256 hashed before transmission.
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Returns a lowercase hex SHA-256 hash of the given value.
    /// Input should already be normalised (trimmed, lowercased) before calling.
    /// </summary>
    public static string Sha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Normalises a UK phone number to E.164 format (+447XXXXXXXXX)
    /// then returns its SHA-256 hash.
    /// </summary>
    public static string HashPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        string e164 = digits switch
        {
            // Already has country code
            var d when d.StartsWith("44") => $"+{d}",
            // Starts with 0 — strip and prepend +44
            var d when d.StartsWith("0") => $"+44{d[1..]}",
            // Assume UK mobile — prepend +44
            _ => $"+44{digits}"
        };

        return Sha256(e164);
    }

    /// <summary>
    /// Normalises a UK phone to a digit-only string (no +) for Facebook.
    /// Facebook expects E.164 without the leading '+'.
    /// </summary>
    public static string HashPhoneForFacebook(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        string e164digits = digits switch
        {
            var d when d.StartsWith("44") => d,
            var d when d.StartsWith("0") => $"44{d[1..]}",
            _ => $"44{digits}"
        };

        return Sha256(e164digits);
    }
}
