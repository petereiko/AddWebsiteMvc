using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;

namespace AddWebsiteMvc.Business.Services.Election
{
    public class ElectionService : IElectionService
    {
        private readonly IGenericRepository<Business.Entities.Election> _electionRepository;
        private readonly IAuthUser _authUser;
        private readonly IGenericRepository<State> _stateRepository;

        public ElectionService(IGenericRepository<Business.Entities.Election> electionRepository, IAuthUser authUser, IGenericRepository<State> stateRepository)
        {
            _electionRepository = electionRepository;
            _authUser = authUser;
            _stateRepository = stateRepository;
        }

        public async Task<MessageResult<ElectionDto>> AddAsync(ElectionDto request, CancellationToken cancellation)
        {
            MessageResult<ElectionDto> result = new();

            Entities.Election election = new()
            {
                CreatedAt = DateTime.Now,
                CreatedBy = _authUser.UserId,
                Description = request.Description!.Trim(),
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                Title = request.Title!.Trim(),
                Id = Guid.NewGuid(),
                IsActive = true
            };
            await _electionRepository.AddAsync(election, cancellation);

            //Update Previous Election Profile to Inactive
            await DeactivatePreviousElections(election, cancellation);

            result.Success = true;
            result.Data = new()
            {
                Description = election.Description,
                EndDate = election.EndDate,
                StartDate = election.StartDate,
                Title = election.Title,
                Id = election.Id
            };

            return result;
        }

        public async Task<MessageResult<ElectionDto>> GetActiveAsync()
        {
            DateTime today = DateTime.Now;
            MessageResult<ElectionDto> result = new()
            {
                Data = (await _electionRepository.FilterAsync(x => x.IsActive, false))
                .Select(x => new ElectionDto
                {
                    Description = x.Description,
                    EndDate = x.EndDate,
                    Id = x.Id,
                    Title = x.Title,
                    StartDate = x.StartDate,
                    IsActive=true
                }).FirstOrDefault(),
                 Message = "Ok",
                Success = true
            };

            return result;
        }

        public async Task<MessageResult<ElectionDto>> UpdateAsync(ElectionDto request, CancellationToken cancellation)
        {
            MessageResult<ElectionDto> result = new();

            Entities.Election? election = await _electionRepository.GetByIdAsync(request.Id);
            if(election == null) 
            {
                result.Message = "Not found";
                return result;
            }
            election.IsActive = request.IsActive;
            election.EndDate = request.EndDate;
            election.Description = request.Description!.Trim();
            election.Title = request.Title!.Trim();
            election.StartDate = request.StartDate;
            election.UpdatedAt=DateTime.UtcNow;
            election.UpdatedBy = _authUser.UserId;

            await _electionRepository.UpdateAsync(election, cancellation);

            if (request.IsActive)
                await DeactivatePreviousElections(election, cancellation);
            
            result.Data = result.Data = new()
            {
                Description = request.Description,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
                Title = request.Title,
                Id = request.Id,
                IsActive=request.IsActive
            };
            result.Message = "Found";
            result.Success = true;
             
            return result;
        }

        public async Task DeactivatePreviousElections(Entities.Election election, CancellationToken cancellationToken)
        {
            //Update Previous Election Profile to Inactive
            IQueryable<Entities.Election> otherElectionQuery = await _electionRepository.FilterAsync(x => x.Id != election.Id);
            otherElectionQuery = otherElectionQuery.Select(x => new Entities.Election
            {
                CreatedAt = x.CreatedAt,
                Description = x.Description,
                CreatedBy = x.CreatedBy,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Id = x.Id,
                Title = x.Title,
                UpdatedAt = DateTime.Now,
                UpdatedBy = _authUser.UserId,
                IsActive = false
            });

            await _electionRepository.UpdateRangeAsync(otherElectionQuery, cancellationToken);
        }


        public async Task<MessageResult<IEnumerable<StateDto>>> GetAllStatesAsync(CancellationToken cancellationToken)
        {
            MessageResult<IEnumerable<StateDto>> result = new();

            IQueryable<State> stateQuery = await _stateRepository.GetAllAsync(false);

            result.Data = stateQuery.Select(x => new StateDto
            {
                Id = x.Id,
                Name = x.Name
            }).OrderBy(x => x.Name).ToList();

            result.Message = "Ok";
            result.Success = true;
            return result;
        }

    }
}
