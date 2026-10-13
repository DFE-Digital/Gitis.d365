using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.GITIS.ADS.DIRECTSYNC.DTO
{
    // SyncResult.cs
    public class SyncResult
    {
        public string Platform { get; set; }
        public bool Success { get; set; }
        public string AudienceId { get; set; }
        public string AudienceUrl { get; set; }
        public string MatchStatus { get; set; }
        public DateTime SyncedAt { get; set; }
        public string ErrorMessage { get; set; }
    }
}
