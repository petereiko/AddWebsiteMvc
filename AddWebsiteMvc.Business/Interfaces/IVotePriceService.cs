using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IVotePriceService
    {
        Task<MessageResult<VotePriceDto>> AddAsync(VotePriceDto model, CancellationToken cancellationToken);

        Task<MessageResult<VotePriceDto>> UpdateAsync(VotePriceDto model, CancellationToken cancellationToken);

        Task<MessageResult<VotePriceDto>> GetVotePrice(CancellationToken cancellationToken);
    }
}
