using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Common;
using AddWebsiteMvc.Business.Configurations;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.HttpClientWrapper;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models;
using AddWebsiteMvc.Business.Models.Election;
using AddWebsiteMvc.Business.Models.Payment;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace AddWebsiteMvc.Business.Services.Election
{
    public class VoteService : IVoteService
    {
        private readonly IGenericRepository<Voter> _voterRepository;
        private readonly IGenericRepository<VotePrice> _votePriceRepository;
        private readonly IGenericRepository<Ballot> _ballotRepository;
        private readonly IGenericRepository<PaymentLog> _paymentLogRepository;
        private readonly IGenericRepository<AddWebsiteMvc.Business.Entities.Election> _electionRepository;
        private readonly IHttpClientWrapperService _httpClientWrapperService;
        private readonly AppSettings _appSettings;

        public VoteService(IOptions<AppSettings> options, IGenericRepository<Voter> voterRepository, IGenericRepository<VotePrice> votePriceRepository, IGenericRepository<Ballot> ballotRepository, IGenericRepository<PaymentLog> paymentLogRepository, IHttpClientWrapperService httpClientWrapperService, IGenericRepository<Entities.Election> electionRepository)
        {
            _voterRepository = voterRepository;
            _votePriceRepository = votePriceRepository;
            _ballotRepository = ballotRepository;
            _paymentLogRepository = paymentLogRepository;
            _httpClientWrapperService = httpClientWrapperService;
            _appSettings = options.Value;
            _electionRepository = electionRepository;
        }

        public async Task<MessageResult<int>> GetVoteCountAsync(decimal amount)
        {
            MessageResult<int> result = new();
            var votePrice = await _votePriceRepository.GetSingleAsync(x => x.IsActive, false);
            if (votePrice == null) throw new Exception("Vote Price not set");

            decimal unitPrice = votePrice.Price;

            int count = (int)(amount / unitPrice);

            result.Data = count;
            result.Success=true;
            return result;
        }

        public async Task<MessageResult<bool>> VoteIsOpen()
        {
            MessageResult<bool> result = new();
            var election = await _electionRepository.GetSingleAsync(x => x.IsActive, false);
            if (election == null)
            {
                result.Message = "No active election";
                return result;

            }
            if (election.StartDate > DateTime.Now)
            {
                result.Message = "Voting has not started";
                return result;
            }
            if (election.EndDate < DateTime.Now)
            {
                result.Message = "Voting has closed";
                return result;
            }
            result.Success = true;
            result.Data = true;
            result.Message = "Voting is ongoing";
            return result;
        }

        public async Task<MessageResult<InitiateVoteResponse>> InitiateVote(InitiateVoteDto model, CancellationToken cancellationToken)
        {
            MessageResult<InitiateVoteResponse> result = new();


            var election = await _electionRepository.GetSingleAsync(x => x.IsActive, false);
            if(election == null)
            {
                result.Message = "Election has not been set up";
                return result;
            }

            if (election.StartDate > DateTime.UtcNow)
            {
                result.Message = "Voting has not started";
                return result;
            }

            if (election.EndDate < DateTime.UtcNow)
            {
                result.Message = "Voting has closed";
                return result;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {_appSettings.PayStack.Secret}");

            string reference = DateTime.Now.Ticks.ToString();

            var votePrice = await _votePriceRepository.GetSingleAsync(x => x.IsActive);

            decimal amount = model.CategoryItems.Sum(x => x.VoteCount * votePrice!.Price);


            var payload = new
            {
                email = model.Email,
                amount = amount * 100,
                reference = reference,
                subaccount = _appSettings.PayStack.SubAccount,
                bearer = "account"
            };

            HttpResponseMessage initResult = await _httpClientWrapperService.PostAsync(_appSettings.PayStack.InitUrl, payload, headers, MediaTypeConstants.APPJSON, cancellationToken);
            if (initResult.StatusCode != System.Net.HttpStatusCode.OK) 
            {
                result.Message = "Could not initiate payment process";
                return result;
            }

            var json = await initResult.Content.ReadAsStringAsync();
            InitPaymentResponse? initPaymentResult = JsonConvert.DeserializeObject<InitPaymentResponse>(json);
            if (initPaymentResult == null)
            {
                result.Message = "Could not initiate payment process";
                return result;
            }

            //Insert into Voter's Table
            await _voterRepository.BeginTransactionAsync();

            Voter voter = new()
            {
                Id = Guid.NewGuid(),
                FullName = $"{model.FirstName} {model.LastName}",
                Email = model.Email,
                Reference = reference
            };
            await _voterRepository.AddAsync(voter, cancellationToken);

            //Insert into Ballot Table

            foreach (var categoryItem in model.CategoryItems)
            {
                Ballot ballotItem = new()
                {
                    Id = Guid.NewGuid(),
                    CastTime = DateTime.Now,
                    CandidateId = model.CandidateId,
                    Count = categoryItem.VoteCount,
                    VoteDate = DateTime.Now,
                    VoterId = voter.Id,
                    CategoryId = categoryItem.CategoryId,
                    Reference = reference
                };
                await _ballotRepository.AddAsync(ballotItem, cancellationToken);
            }

                

            //Insert into Payment Log Table
            PaymentLog paymentLog = new()
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                CreatedDate = DateTime.Now,
                Reference = reference,
                AccessCode = initPaymentResult.data.access_code,
                AuthorizationUrl = initPaymentResult.data.authorization_url,
            };
            await _paymentLogRepository.AddAsync(paymentLog, cancellationToken);

            await _voterRepository.CommitTransactionAsync();

            result.Success= true;
            result.Data = new()
            {
                AuthorizationUrl = initPaymentResult.data.authorization_url,
                Reference = reference
            };

            return result;
        }

        public async Task<MessageResult> Verify(string reference, CancellationToken cancellationToken)
        {
            MessageResult result = new();

            PaymentLog? log = await _paymentLogRepository.GetSingleAsync(x => x.Reference == reference);
            if(log == null)
            {
                result.Message = "Invalid request";
                return result;
            }
            if (log.Status == Enums.PaymentStatus.Success)
            {
                result.Message = "This transaction has already been verified before";
                return result;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", $"Bearer {_appSettings.PayStack.Secret}");

            HttpResponseMessage responseMessage = await _httpClientWrapperService.GetAsync(_appSettings.PayStack.VerifyUrl + reference, headers, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                result.Message = "Could not verify transaction status at this time. We will retry and give value accordingly to your vote automatically";
                return result;
            }
            var json = await responseMessage.Content.ReadAsStringAsync();

            VerifyTransactionResponse? response = JsonConvert.DeserializeObject<VerifyTransactionResponse>(json);

            if(response?.data?.status== "success")
            {
                //Approve Transaction
                log = await _paymentLogRepository.GetSingleAsync(x => x.Reference == reference);
                log.ConfirmedDate = DateTime.Now;
                log.Status= PaymentStatus.Success;
                await _paymentLogRepository.UpdateAsync(log, cancellationToken);

                //Approve Ballots

                IQueryable<Ballot> ballots = await _ballotRepository.FilterAsync(x => x.Reference == reference);

                ballots = ballots.ToList().Select(x =>
                {
                    x.Status = BallotStatus.Approved;
                    x.VoteDate = DateTime.Now;
                    return x;
                }).AsQueryable();

                await _ballotRepository.UpdateRangeAsync(ballots, cancellationToken);

                //Approve Voter
                Voter? voter = await _voterRepository.GetSingleAsync(x => x.Reference == reference);
                if (voter != null)
                {
                    voter.BallotStatus = BallotStatus.Approved;
                    await _voterRepository.UpdateAsync(voter, cancellationToken);
                }

                result.Success = true;
                result.Message = "Verification successful";
                return result;
            }
            else
            {
                log = await _paymentLogRepository.GetSingleAsync(x => x.Reference == reference);
                log.Status = PaymentStatus.Failed;
                await _paymentLogRepository.UpdateAsync(log, cancellationToken);
            }

            result.Message=response.data?.message;
            return result;
        }
    }
}
