using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;

namespace VoteApp.Application.Services.Election
{
    public class VotePriceService : IVotePriceService
    {
        private readonly IGenericRepository<VotePrice> _votePriceRepository;
        private readonly IElectionService _electionService;
        private readonly IAuthUser _authUser;

        public VotePriceService(IGenericRepository<VotePrice> votePriceRepository, IAuthUser authUser, IElectionService electionService)
        {
            _votePriceRepository = votePriceRepository;
            _authUser = authUser;
            _electionService = electionService;
        }

        public async Task<MessageResult<VotePriceDto>> AddAsync(VotePriceDto model, CancellationToken cancellationToken)
        {
            MessageResult<VotePriceDto> result = new();


            var electionResult = await _electionService.GetActiveAsync();
            if (electionResult.Data == null)
            {
                result.Message="No active election";
                return result;
            }

            VotePrice votePrice = new()
            {
                CreatedAt = DateTime.Now,
                CreatedBy = _authUser.UserId,
                Price = model.Price,
                Id = Guid.NewGuid(),
                IsActive = true,
                ElectionId = electionResult.Data.Id
            };
            await _votePriceRepository.AddAsync(votePrice, cancellationToken);

            //Update Previous Election Profile to Inactive
            await DeactivatePreviousPrice(votePrice, cancellationToken);

            result.Success = true;
            result.Data = new()
            {
                Price = votePrice.Price,
                Id = votePrice.Id
            };
            return result;
        }

        public async Task<MessageResult<VotePriceDto>> GetVotePrice(CancellationToken cancellationToken)
        {
            MessageResult<VotePriceDto> result = new()
            {
                Data = (await _votePriceRepository.FilterAsync(x => x.IsActive, false, x => x.Election))
                .Select(x => new VotePriceDto
                {
                    Id = x.Id,
                    Price = x.Price,
                    Election = new()
                    {
                        Description = x.Election.Description,
                        EndDate = x.Election.EndDate,
                        Id = x.ElectionId,
                        IsActive = x.Election.IsActive,
                        StartDate = x.Election.StartDate,
                        Title = x.Election.Title
                    },
                    IsActive = true
                }).FirstOrDefault(),
                Success = true
            };

            return result;
        }

        public async Task<MessageResult<VotePriceDto>> UpdateAsync(VotePriceDto model, CancellationToken cancellationToken)
        {
            MessageResult<VotePriceDto> result = new() { Data = new() };

            
            VotePrice? price = await _votePriceRepository.GetSingleAsync(x=>x.Id==model.Id, true, x=>x.Election);
            if (price == null)
            {
                result.Message = "Price not found";
                return result;
            }
            price.IsActive = model.IsActive;
            price.Price = model.Price;
            price.UpdatedAt = DateTime.UtcNow;
            price.UpdatedBy = _authUser.UserId;
           

            await _votePriceRepository.UpdateAsync(price, cancellationToken);

            if (model.IsActive)
                await DeactivatePreviousPrice(price, cancellationToken);

            result.Data = new()
            {
                Price = model.Price,
                Election = new()
                {
                    Description = price.Election.Description,
                    EndDate = price.Election.EndDate,
                    IsActive = price.Election.IsActive,
                    StartDate = price.Election.StartDate,
                    Title = price.Election.Title,
                    
                },
                IsActive = price.IsActive,
                Id = model.Id
            };
            result.Message = "Update was successful";
            result.Success = true;

            return result;
        }


        public async Task DeactivatePreviousPrice(VotePrice votePrice, CancellationToken cancellationToken)
        {
            //Update Previous Election Profile to Inactive
            IQueryable<VotePrice> otherElectionQuery = await _votePriceRepository.FilterAsync(x => x.Id != votePrice.Id);
            otherElectionQuery = otherElectionQuery.Select(x => new VotePrice
            {
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Id = x.Id,
                Price=x.Price,
                UpdatedAt = DateTime.Now,
                UpdatedBy = _authUser.UserId,
                IsActive = false
            });

            await _votePriceRepository.UpdateRangeAsync(otherElectionQuery, cancellationToken);
        }
    }
}
