using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Models.SurveyModels
{
    public class SurveyTokenData
    {
        public Guid VoteId { get; set; }
        public string Email { get; set; }
        public long Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }

        // Check if token has expired (e.g., 30 days)
        public bool IsExpired(int expirationDays = 30)
        {
            return DateTime.UtcNow > CreatedAt.AddDays(expirationDays);
        }
    }
}
