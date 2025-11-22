using AddWebsiteMvc.Models;

namespace AddWebsiteMvc.Interfaces
{
    public interface IContestantService
    {
        Task<BaseResponse<Contestant>> AddAsync(Contestant model, CancellationToken cancellationToken);
        Task<BaseResponse<Contestant>> EditAsync(Contestant model, CancellationToken cancellationToken);
        Task<GetAllContestantResponse> GetAllContestantsAsync();
        Task<BaseResponse<Contestant>> GetContestantByIdAsync(string id);
        Task<BaseResponse<VoteResponseData>> VoteAsync(VoteRequest model, CancellationToken cancellationToken);
        Task<BaseResponse> ConfirmPayment(string reference);
    }
}
