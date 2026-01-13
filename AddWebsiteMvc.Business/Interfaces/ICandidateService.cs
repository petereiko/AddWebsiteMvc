using AddWebsiteMvc.Business.Common;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface ICandidateService
    {
        Task<MessageResult<CandidateDto>> AddAsync(CandidateDto model, CancellationToken cancellationToken);

        Task<MessageResult<CandidateDto>> UpdateAsync(CandidateDto contestantDto, CancellationToken cancellationToken);

        Task<MessageResult> DeleteAsync(Guid candidateId, CancellationToken cancellationToken);

        Task<MessageResult<IEnumerable<CandidateDto>>> GetAllAsync(CancellationToken cancellationToken);

        Task<MessageResult<List<CandidateCategoryViewModel>>> FetchCandidateCategoriesAsync(Guid candidateId);

        Task<MessageResult> UpdateCandidateCategoriesAsync(CandidateCategoryListViewModel model, CancellationToken cancellationToken);

        Task<MessageResult<CandidateGridViewModel>> FetchCandidatesAsync(CancellationToken cancellationToken);

        Task<MessageResult<CandidateDto>> GetByIdAsync(Guid contestantId, CancellationToken cancellationToken);
    }
}
