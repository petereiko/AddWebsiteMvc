using AddWebsiteMvc.Models;

namespace AddWebsiteMvc.Interfaces
{
    public interface IElectionService
    {
        Task<GetElectionResponse> GetActiveElectionAsync();
    }
}
