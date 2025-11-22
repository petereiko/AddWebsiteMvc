namespace AddWebsiteMvc.Models
{
    public class AuthData
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public DateTime expiresAt { get; set; }
        public string tokenType { get; set; }
        public UserData user { get; set; }
        public Election election { get; set; }
    }
}
