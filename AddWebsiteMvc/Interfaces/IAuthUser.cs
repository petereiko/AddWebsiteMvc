namespace AddWebsiteMvc.Interfaces
{
    public interface IAuthUser
    {
        string UserId { get; }
        string Email { get; }
        string FullName { get; }
        string Token { get; }

        List<string> GetRoles();

        bool IsInRole(string role);

        bool IsAuthenticated();

        string GetClaim(string claimType);

    }
}
