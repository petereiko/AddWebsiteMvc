using System.Drawing;

namespace AddWebsiteMvc.Models
{
    public class VoteRequest
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string contestantId { get; set; }
        public int count { get; set; }
    }
}
