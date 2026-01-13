namespace AddWebsiteMvc.Models
{

    public class ErrorResponse
    {
        public string message { get; set; }
        public List<string> errors { get; set; } = new();
    }

}
