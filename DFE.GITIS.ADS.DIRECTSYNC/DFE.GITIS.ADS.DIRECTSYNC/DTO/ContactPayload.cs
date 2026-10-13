using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.ADS.DIRECTSYNC.DTO
{
    public class ContactPayload
    {
        /// <summary>Dynamics 365 Contact GUID</summary>
        public string ContactId { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }

        /// <summary>ISO 3166-1 alpha-2 country code, e.g. "GB"</summary>
        public string? CountryCode { get; set; } = "GB";

        public string? ZipCode { get; set; }

        /// <summary>Format: yyyyMMdd</summary>
        public string? DateOfBirth { get; set; }

        /// <summary>Dataverse message that triggered this: "Create" or "Update"</summary>
        public string TriggerSource { get; set; } = "Unknown";

        /// <summary>UTC timestamp when this message was enqueued</summary>
        public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    }

}
