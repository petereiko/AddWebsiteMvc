using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoteApp.Application.Services
{
    public class BackgroundJobService
    {
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<PaymentLog> _paymentLogRepository;
        private readonly IVoteService _voteService;
        private readonly ILogger<BackgroundJobService> _logger;
        public BackgroundJobService(IConfiguration configuration, IGenericRepository<PaymentLog> paymentLogRepository, IVoteService voteService, ILogger<BackgroundJobService> logger)
        {
            _configuration = configuration;
            _paymentLogRepository = paymentLogRepository;
            _voteService = voteService;
            _logger = logger;
        }

        [DisableConcurrentExecution(60)]
        public void VerifyPayments()
        {
            try
            {
                List<PaymentLog> paymentLogs = _paymentLogRepository.Filter(x => x.Status != PaymentStatus.Success && x.RetryCount == null || x.RetryCount <= 10).Take(10).ToList();
                foreach (var item in paymentLogs)
                {
                    try
                    {
                        var result = _voteService.Verify(item.Reference, CancellationToken.None).GetAwaiter().GetResult();
                        if (!result.Success)
                        {
                            item.RetryCount = item.RetryCount == null ? 1 : item.RetryCount + 1;
                            item.Status = PaymentStatus.Failed;
                            _paymentLogRepository.Update(item);
                        }
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex.Message, ex);
                        continue;
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
