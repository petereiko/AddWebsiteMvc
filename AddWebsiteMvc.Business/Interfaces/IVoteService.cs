using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using AddWebsiteMvc.Business.Models.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Interfaces
{
    public interface IVoteService
    {
        Task<MessageResult<bool>> VoteIsOpen();
        Task<MessageResult<int>> GetVoteCountAsync(decimal amount);
        Task<MessageResult<InitiateVoteResponse>> InitiateVote(InitiateVoteDto model, CancellationToken cancellationToken);
        Task<MessageResult> Verify(string reference, CancellationToken cancellationToken);
    }
}
