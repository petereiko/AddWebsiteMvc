using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IElectionService
    {
        Task<MessageResult<ElectionDto>> AddAsync(ElectionDto request, CancellationToken cancellation);
        Task<MessageResult<ElectionDto>> UpdateAsync(ElectionDto request, CancellationToken cancellation);
        Task<MessageResult<ElectionDto>> GetActiveAsync();
        Task<MessageResult<IEnumerable<StateDto>>> GetAllStatesAsync(CancellationToken cancellationToken);
    }
}
