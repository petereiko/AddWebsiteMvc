using AddWebsiteMvc.Models;

namespace AddWebsiteMvc.Interfaces
{
    public interface ICandidateService
    {
        Task<BaseResponse<Candidate>> AddAsync(Candidate model, CancellationToken cancellationToken);
        Task<BaseResponse<Candidate>> EditAsync(Candidate model, CancellationToken cancellationToken);
        Task<GetAllCandidateResponse> GetAllCandidatesAsync();
        Task<BaseResponse<Candidate>> GetCandidateByIdAsync(string id);
        Task<BaseResponse<VoteResponseData>> VoteAsync(VoteRequest model, CancellationToken cancellationToken);
        Task<BaseResponse> ConfirmPayment(string reference);

    }
}
