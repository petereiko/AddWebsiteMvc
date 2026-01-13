using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using AddWebsiteMvc.Business.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace AddWebsiteMvc.Business.Services.Election
{
    public class CandidateService : ICandidateService
    {
        private readonly IGenericRepository<Candidate> _candidateRepository;
        private readonly IGenericRepository<Ballot> _ballotRepository;
        private readonly IGenericRepository<VotePrice> _votePriceRepository;
        private readonly IGenericRepository<Entities.Election> _electionRepository;
        private readonly IGenericRepository<CandidateCategory> _candidateCategoryRepository;
        private readonly IElectionService _electionService;
        private readonly IAuthUser _authUser;
        private readonly VoteDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CandidateService(IGenericRepository<Candidate> candidateRepository, IElectionService electionService, IAuthUser authUser, IGenericRepository<Ballot> ballotRepository, IGenericRepository<VotePrice> votePriceRepository, IGenericRepository<Entities.Election> electionRepository, VoteDbContext context, IGenericRepository<CandidateCategory> candidateCategoryRepository, IWebHostEnvironment env)
        {
            _candidateRepository = candidateRepository;
            _electionService = electionService;
            _authUser = authUser;
            _ballotRepository = ballotRepository;
            _votePriceRepository = votePriceRepository;
            _electionRepository = electionRepository;
            _context = context;
            _candidateCategoryRepository = candidateCategoryRepository;
            _env = env;
        }


        public async Task<MessageResult<CandidateDto>> AddAsync(CandidateDto model, CancellationToken cancellationToken)
        {
            MessageResult<CandidateDto> result = new();
            var activeElectionResult = await _electionService.GetActiveAsync();
            if (activeElectionResult.Data == null)
            {
                result.Message = "There is no active election ongoing";
                return result;
            }
            if(activeElectionResult.Data.EndDate< DateTime.Now)
            {
                result.Message = "The active election has ended, you cannot add Candidate";
                return result;
            }

            //Save Passport to Folder
            if(model.PassportFile != null && model.PassportFile.Length > 0)
            {
                var passportFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.PassportFile.FileName)}";
                var passportPath = Path.Combine(_env.WebRootPath, "Passports", passportFileName);
                using (var stream = new FileStream(passportPath, FileMode.Create))
                {
                    await model.PassportFile.CopyToAsync(stream, cancellationToken);
                }
                model.PassportFileName = passportFileName;
            }

            IQueryable<Candidate> candidateQuery = await _candidateRepository.GetAllAsync(false);
            model.Order = candidateQuery.Count() == 0 ? 0 : await candidateQuery.MaxAsync(x => x.Order);
            model.Order = model.Order + 1;

            Candidate candidate = new()
            {
                Id = Guid.NewGuid(),
                Title = string.IsNullOrEmpty(model.Title)?"":model.Title,
                CreatedAt = DateTime.Now,
                CreatedBy = _authUser.UserId,
                //DOB = model.DOB,
                ElectionId = activeElectionResult!.Data!.Id,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                IsActive = true,
                PassportFileName = model.PassportFileName?.Trim(),
                Order = model.Order,
                StateId = model.StateId
            };
            candidate = await _candidateRepository.AddAsync(candidate, cancellationToken);

            model.Id = candidate.Id;
            result.Data = model;
            result.Success = true;
            return result;
        }

        public async Task<MessageResult<IEnumerable<CandidateDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            MessageResult<IEnumerable<CandidateDto>> result = new();
            var grid = await FetchCandidatesAsync(cancellationToken);
            result.Data = grid.Data.Candidates.OrderByDescending(x => x.VoteCount).ToList();
            result.Success = true;
            return result;
        }

        public async Task<MessageResult<List<CandidateCategoryViewModel>>> FetchCandidateCategoriesAsync(Guid candidateId)
        {
            MessageResult<List<CandidateCategoryViewModel>> result = new() { Data = new() };
            var connection = _context.Database.GetDbConnection();
            await using (connection)
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "dbo.FetchCandidateCategories";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@CandidateId", candidateId));
                using var reader = command.ExecuteReader();
                // Read Candidate Categories (First Result Set)
                while (reader.Read())
                {
                    result.Data.Add(new CandidateCategoryViewModel
                    {
                        CategoryId = reader.GetInt32("CategoryId"),
                        Name = reader.GetString("Name"),
                        IsChecked = reader.GetBoolean("IsChecked")
                    });
                }
            }
            result.Message = "Ok";
            result.Success = true;
            return result;
        }

        public async Task<MessageResult> UpdateCandidateCategoriesAsync(CandidateCategoryListViewModel model, CancellationToken cancellationToken)
        {
            MessageResult result = new();

            if (model.Categories.Count(x => x.IsChecked) == 0)
            {
                
                var existingCategories = await _candidateCategoryRepository.FilterAsync(x => x.CandidateId == model.CandidateId);
                await _candidateCategoryRepository.RemoveRangeAsync(existingCategories, cancellationToken);

            }
            else {
                //First, delete existing categories for the candidate
                var existingCategories = await _candidateCategoryRepository
                    .FilterAsync(x => x.CandidateId == model.CandidateId);

                await _candidateCategoryRepository.RemoveRangeAsync(existingCategories, cancellationToken);
                //Then, add the selected categories
                var selectedCategories = model.Categories
                    .Where(x => x.IsChecked)
                    .Select(x => new CandidateCategory
                    {
                        CandidateId = model.CandidateId,
                        CategoryId = x.Value
                    }).ToList();
                await _candidateCategoryRepository.AddRangeAsync(selectedCategories, cancellationToken);
            }

            result.Message = "Categories updated successfully";
            result.Success = true;
            return result;
        }

        public async Task<MessageResult<CandidateGridViewModel>> FetchCandidatesAsync(CancellationToken cancellationToken)
        {
            MessageResult<CandidateGridViewModel> result = new() { Data = new() };

            var connection = _context.Database.GetDbConnection();

            await using (connection)
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "dbo.FetchCandidates";
                command.CommandType = CommandType.StoredProcedure;
                //command.Parameters.Add(new SqlParameter("@SchoolId", schoolId));

                using var reader = command.ExecuteReader();

                // Read School Operator (First Result Set)
                while (reader.Read())
                {
                    result.Data.Candidates.Add(new()
                    {
                        Id = reader.GetGuid("Id"),
                        ElectionId = reader.GetGuid("ElectionId"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        IsActive = reader.GetBoolean("IsActive"),
                        Order = reader.GetInt32("Order"),
                        PassportFileName = reader.GetString("PassportFileName"),
                        StateId = reader.GetInt32("StateId"),
                        StateName = reader.GetString("StateName"),
                        Title = reader.GetString("Title"),
                        VoteCount = reader.GetInt32("VoteCount"),
                        CandidateCategoryCount = reader.GetInt32("CandidateCategoryCount")
                    });
                }

                // Move to School (Second Result Set)
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            result.Data.CandidateCategories.Add(new CandidateCategoryModel()
                            {
                                CandidateId = reader.GetGuid("CandidateId"),
                                Category = reader.GetString("Category"),
                                CategoryId = reader.GetInt32("CategoryId")
                            });
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        result.Data.CandidateCategoryVotes.Add(new CandidateCategoryVoteModel
                        {
                            CandidateId = reader.GetGuid("CandidateId"),
                            CategoryId = reader.GetInt32("CategoryId"),
                            VoteCount = reader.GetInt32("VoteCount")
                        });
                    }
                }

                // Move to School Owner (Fourth Result Set)
                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        result.Data.Election = new ElectionDto
                        {
                            Description = reader.GetString("Description"),
                            EndDate = reader.GetDateTime("EndDate"),
                            StartDate = reader.GetDateTime("StartDate"),
                            Id = reader.GetGuid("Id"),
                            IsActive = reader.GetBoolean("IsActive"),
                            Title = reader.GetString("Title")
                        };
                    }
                }

                // Move to Documents (Fifth Result Set - can have multiple records)
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        result.Data.VotesCastToday = reader.GetInt32("VotesCastToday");
                    }
                }

                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        result.Data.VotePrice = new VotePriceDto
                        {
                            Id = reader.GetGuid("Id"),
                            IsActive = reader.GetBoolean("IsActive"),
                            Price = reader.GetDecimal("Price")
                        };
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        result.Data.Categories.Add(new CategoryDto
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name")
                        });
                    }
                }
            }
            result.Message = "Ok";
            result.Success = true;
            return result;
        }



        public async Task<MessageResult<CandidateDto>> GetByIdAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            MessageResult<CandidateDto> result = new();

            IQueryable<Ballot> ballotQuery = await _ballotRepository.FilterAsync(x => x.CandidateId == candidateId && x.Status == BallotStatus.Approved, false);

            int voteCount = await ballotQuery.SumAsync(x => x.Count);

            decimal? votePrice = (await _votePriceRepository.GetSingleAsync(x => x.IsActive, false))?.Price;


            IQueryable<Candidate> query = await _candidateRepository.FilterAsync(x => x.Id == candidateId, false, x => x.State);
            result.Data = await query.Select(x => new CandidateDto
            {
                Id = x.Id,
                //DOB = x.DOB,
                ElectionId = x.ElectionId,
                FirstName = x.FirstName.Trim(),
                LastName = x.LastName.Trim(),
                PassportFileName = x.PassportFileName,
                //VideoFileName = x.VideoFileName,
                //Talent = x.Talent,
                //ShortNote = x.ShortNote,
                IsActive = x.IsActive,
                VoteCount = voteCount,
                votePrice = votePrice,
                Order = x.Order,
                Election = new()
                {
                    Id = x.Election.Id,
                    Description = x.Election.Description,
                    EndDate = x.Election.EndDate,
                    IsActive = x.Election.IsActive,
                    StartDate = x.Election.StartDate,
                    Title = x.Election.Title
                },
                StateId = x.StateId,
                StateName = x.State.Name,
                Title = x.Title
            }).FirstOrDefaultAsync();

            result.Success = result.Data != null;
            return result;
        }

        public async Task<MessageResult<CandidateDto>> UpdateAsync(CandidateDto model, CancellationToken cancellationToken)
        {
            MessageResult<CandidateDto> result = new();

            var activeElectionResult = await _electionService.GetActiveAsync();
            if (activeElectionResult.Data == null)
            {
                result.Message = "There is no active election ongoing";
                return result;
            }

            Candidate? candidate = await _candidateRepository.GetByIdAsync(model.Id);
            if (candidate == null)
            {
                result.Message = "Candidate not found";
                return result;
            }

            candidate.Title= model.Title;
            candidate.StateId = model.StateId;
            candidate.FirstName= model.FirstName;
            candidate.LastName= model.LastName;
            if (!string.IsNullOrEmpty(model.PassportFileName))
                candidate.PassportFileName = model.PassportFileName;
            candidate.IsActive = model.IsActive;
            candidate.Order = model.Order;

            await _candidateRepository.UpdateAsync(candidate, cancellationToken);

            result.Message = "Success";
            result.Success = true;
            return result;
        }

        public async Task<MessageResult> DeleteAsync(Guid candidateId, CancellationToken cancellationToken)
        {
            MessageResult result = new();
            var candidate = await _candidateRepository.GetByIdAsync(candidateId);
            if (candidate == null)
            {
                result.Message = "Candidate not found";
                return result;
            }

            var ballots = await _ballotRepository.FilterAsync(x => x.CandidateId == candidateId && x.Status == BallotStatus.Approved, false);
            if(ballots.Any())
            {
                result.Message = "You cannot delete a candidate with approved ballots";
                return result;
            }

            await _candidateRepository.RemoveAsync(candidate, cancellationToken);
            result.Message = "Success";
            result.Success = true;
            return result;
        }
    }
}
