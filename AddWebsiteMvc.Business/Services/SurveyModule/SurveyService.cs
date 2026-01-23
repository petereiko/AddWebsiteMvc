using AddWebsiteMvc.Business.Entities.SurveyEntity;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.SurveyModels;
using AddWebsiteMvc.Business.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Services.SurveyModule
{
    public class SurveyService : ISurveyService
    {
        private readonly VoteDbContext _context;
        private readonly ILogger<SurveyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthUser _authUser;

        public SurveyService(
            VoteDbContext context,
            ILogger<SurveyService> logger,
            IConfiguration configuration,
            IAuthUser authUser)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _authUser = authUser;
        }

        /// <summary>
        /// Get survey with all questions
        /// </summary>
        public async Task<SurveyDto> GetSurveyAsync(Guid surveyId)
        {
            try
            {
                var survey = await _context.Surveys
                    .AsNoTracking()
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.Id == surveyId && s.IsActive);

                if (survey == null)
                {
                    _logger.LogWarning($"Survey {surveyId} not found or not active");
                    return null;
                }

                // Check if survey is within date range
                if (survey.StartDate.HasValue && DateTime.UtcNow < survey.StartDate.Value)
                {
                    _logger.LogWarning($"Survey {surveyId} has not started yet");
                    return null;
                }

                if (survey.EndDate.HasValue && DateTime.UtcNow > survey.EndDate.Value)
                {
                    _logger.LogWarning($"Survey {surveyId} has ended");
                    return null;
                }

                return new SurveyDto
                {
                    SurveyId = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    ThankYouMessage = survey.ThankYouMessage,
                    Questions = survey.Questions
                        .OrderBy(q => q.DisplayOrder)
                        .Select(q => new SurveyQuestionDto
                        {
                            QuestionId = q.Id,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            IsRequired = q.IsRequired,
                            DisplayOrder = q.DisplayOrder,
                            Options = string.IsNullOrEmpty(q.Options)
                                ? null
                                : JsonConvert.DeserializeObject<List<string>>(q.Options),
                            MinValue = q.MinValue,
                            MaxValue = q.MaxValue
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving survey {surveyId}");
                throw;
            }
        }

        /// <summary>
        /// Get survey by response token
        /// </summary>
        public async Task<SurveyDto> GetSurveyByTokenAsync(string token)
        {
            try
            {
                var tracking = await _context.SurveyEmailTrackings
                    .Include(t => t.Response)
                    .ThenInclude(r => r.Survey)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (tracking == null)
                {
                    _logger.LogWarning($"Token not found: {token}");
                    return null;
                }

                var survey = tracking.Response.Survey;

                if (survey == null || !survey.IsActive)
                {
                    _logger.LogWarning($"Survey not found or not active for token");
                    return null;
                }

                // Get questions
                var questions = await _context.SurveyQuestions
                    .AsNoTracking()
                    .Where(q => q.SurveyId == survey.Id)
                    .OrderBy(q => q.DisplayOrder)
                    .ToListAsync();

                return new SurveyDto
                {
                    SurveyId = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    ThankYouMessage = survey.ThankYouMessage,
                    Questions = questions.Select(q => new SurveyQuestionDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        IsRequired = q.IsRequired,
                        DisplayOrder = q.DisplayOrder,
                        Options = string.IsNullOrEmpty(q.Options)
                            ? null
                            : JsonConvert.DeserializeObject<List<string>>(q.Options),
                        MinValue = q.MinValue,
                        MaxValue = q.MaxValue
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving survey by token");
                throw;
            }
        }
        
        /// <summary>
        /// Check if user has already responded to survey
        /// </summary>
        public async Task<bool> HasUserRespondedAsync(Guid surveyId, string userEmail)
        {
            try
            {
                return await _context.SurveyResponses
                    .AnyAsync(r => r.SurveyId == surveyId
                        && r.UserEmail == userEmail
                        && r.IsCompleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking user response for {userEmail}");
                throw;
            }
        }

        /// <summary>
        /// Submit survey answers
        /// </summary>
        /// 
        public async Task<bool> SubmitSurveyAsync(SubmitSurveyDto submitDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the tracking record by token
                var tracking = await _context.SurveyEmailTrackings
                    .Include(t => t.Response)
                    .ThenInclude(r => r.Survey)
                    .FirstOrDefaultAsync(t => t.Token == submitDto.Token);

                if (tracking == null)
                {
                    _logger.LogWarning($"Token not found: {submitDto.Token}");
                    return false;
                }

                var response = tracking.Response;

                if (response.IsCompleted)
                {
                    _logger.LogWarning($"Response {response.Id} already completed");
                    return false;
                }

                // Get survey questions
                var questions = await _context.SurveyQuestions
                    .AsNoTracking()
                    .Where(q => q.SurveyId == response.SurveyId)
                    .ToListAsync();

                // Validate required questions
                var requiredQuestions = questions.Where(q => q.IsRequired).ToList();
                var answeredQuestionIds = submitDto.Answers.Select(a => a.QuestionId).ToHashSet();

                foreach (var requiredQuestion in requiredQuestions)
                {
                    if (!answeredQuestionIds.Contains(requiredQuestion.Id))
                    {
                        throw new InvalidOperationException($"Required question '{requiredQuestion.QuestionText}' must be answered");
                    }
                }

                // Save answers
                foreach (var answerDto in submitDto.Answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                    if (question == null) continue;

                    var answer = new SurveyAnswer
                    {
                        ResponseId = response.Id,
                        QuestionId = answerDto.QuestionId,
                        AnswerText = answerDto.AnswerText,
                        AnswerNumeric = answerDto.AnswerNumeric,
                        AnsweredAt = DateTime.UtcNow
                    };

                    _context.SurveyAnswers.Add(answer);
                }

                // Mark response as completed
                response.IsCompleted = true;
                response.CompletedAt = DateTime.UtcNow;
                _context.SurveyResponses.Update(response);

                // Update email tracking
                tracking.EmailStatus = EmailStatus.Completed;
                _context.SurveyEmailTrackings.Update(tracking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Survey response {response.Id} completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error submitting survey");
                throw;
            }
        }

        public async Task<SurveyDto> GetSurveyByVoteIdAsync(Guid voteId)
        {
            try
            {
                // Get the default active survey
                // Or you could have a mapping table VoteId -> SurveyId
                var survey = await _context.Surveys
                    .AsNoTracking()
                    .Include(s => s.Questions)
                    .FirstOrDefaultAsync(s => s.IsActive);

                if (survey == null)
                {
                    _logger.LogWarning($"No active survey found for vote {voteId}");
                    return null;
                }

                return new SurveyDto
                {
                    SurveyId = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    ThankYouMessage = survey.ThankYouMessage,
                    Questions = survey.Questions
                        .OrderBy(q => q.DisplayOrder)
                        .Select(q => new SurveyQuestionDto
                        {
                            QuestionId = q.Id,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            IsRequired = q.IsRequired,
                            DisplayOrder = q.DisplayOrder,
                            Options = string.IsNullOrEmpty(q.Options)
                                ? null
                                : JsonConvert.DeserializeObject<List<string>>(q.Options),
                            MinValue = q.MinValue,
                            MaxValue = q.MaxValue
                        })
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving survey for vote {voteId}");
                throw;
            }
        }

        /// <summary>
        /// Get survey statistics
        /// </summary>
        public async Task<SurveyStatisticsDto> GetSurveyStatisticsAsync(Guid surveyId)
        {
            try
            {
                var survey = await _context.Surveys
                    .Include(s => s.Responses)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == surveyId);

                if (survey == null) return null;

                var responses = survey.Responses.ToList();
                var completedResponses = responses.Where(r => r.IsCompleted).ToList();

                var avgCompletionTime = completedResponses
                    .Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt.Value - r.StartedAt).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average();

                return new SurveyStatisticsDto
                {
                    SurveyId = survey.Id,
                    Title = survey.Title,
                    TotalResponses = responses.Count,
                    CompletedResponses = completedResponses.Count,
                    IncompleteResponses = responses.Count - completedResponses.Count,
                    CompletionRate = responses.Count > 0
                        ? Math.Round((decimal)completedResponses.Count / responses.Count * 100, 2)
                        : 0,
                    AvgCompletionTimeSeconds = (int)avgCompletionTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting statistics for survey {surveyId}");
                throw;
            }
        }

        /// <summary>
        /// Get all answers for a response
        /// </summary>
        public async Task<List<SurveyAnswer>> GetResponseAnswersAsync(Guid responseId)
        {
            try
            {
                return await _context.SurveyAnswers
                    .Include(a => a.Question)
                    .AsNoTracking()
                    .Where(a => a.ResponseId == responseId)
                    .OrderBy(a => a.Question.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting answers for response {responseId}");
                throw;
            }
        }

        /// <summary>
        /// Track email open event
        /// </summary>
        public async Task TrackEmailOpenAsync(string token)
        {
            try
            {
                var tracking = await _context.SurveyEmailTrackings
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (tracking != null && !tracking.OpenedAt.HasValue)
                {
                    tracking.OpenedAt = DateTime.UtcNow;
                    tracking.EmailStatus = EmailStatus.Opened;
                    _context.SurveyEmailTrackings.Update(tracking);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Email opened for token {token.Substring(0, 10)}...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking email open");
            }
        }
        /// <summary>
        /// Track email click event
        /// </summary>
        public async Task TrackEmailClickAsync(string token, string ipAddress, string userAgent)
        {
            try
            {
                var tracking = await _context.SurveyEmailTrackings
                    .Include(t => t.Response)
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (tracking != null && !tracking.ClickedAt.HasValue)
                {
                    // Update tracking
                    tracking.ClickedAt = DateTime.UtcNow;
                    tracking.EmailStatus = EmailStatus.Clicked;
                    _context.SurveyEmailTrackings.Update(tracking);

                    // Update response with IP and UserAgent (capture when they first click)
                    if (tracking.Response != null)
                    {
                        tracking.Response.IpAddress = ipAddress;
                        tracking.Response.UserAgent = userAgent;
                        _context.SurveyResponses.Update(tracking.Response);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Email clicked for token, IP: {ipAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking email click");
            }
        }
    }
}
