namespace AddWebsiteMvc.Models
{


    public class CandidateCategoryResponseModel
    {
        public Datum[] data { get; set; }
        public DateTime startDate { get; set; }
        public DateTime closeDate { get; set; }
        public int unitVotePrice { get; set; }
        public int votesCastToday { get; set; }
        public object electionTitle { get; set; }
        public object message { get; set; }
        public bool succeeded { get; set; }
        public int statusCode { get; set; }
        public object[] errors { get; set; }
    }

    public class Datum
    {
        public int categoryId { get; set; }
        public string name { get; set; }
        public bool isChecked { get; set; }
    }

}
