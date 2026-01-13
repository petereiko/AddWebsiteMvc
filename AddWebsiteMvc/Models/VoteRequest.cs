using System.Drawing;

namespace AddWebsiteMvc.Models
{
    public class VoteRequest
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string candidateId { get; set; }
        public int count { get; set; }
        public int categoryId { get; set; }
    }
}
