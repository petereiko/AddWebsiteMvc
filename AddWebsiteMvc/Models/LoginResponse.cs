namespace AddWebsiteMvc.Models
{
    public class LoginResponse:BaseResponse
    {
        public AuthData data { get; set; }
        public bool succeeded { get; set; }
        
    }
}
