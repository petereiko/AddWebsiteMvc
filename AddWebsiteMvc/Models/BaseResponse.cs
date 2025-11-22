namespace AddWebsiteMvc.Models
{
    public class BaseResponse
    {
        public bool succeeded { get; set; }
        public int statusCode { get; set; }
        public List<string> errors { get; set; } = new();
    }
    
    public class BaseResponse<T>
    {
        public bool succeeded { get; set; }
        public int statusCode { get; set; }
        public List<string> errors { get; set; } = new();
        public T data { get; set; } = default!;
    }
}
