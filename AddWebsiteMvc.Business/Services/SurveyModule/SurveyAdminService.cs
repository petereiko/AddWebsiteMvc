using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.SurveyModels.AdminDto;
using AddWebsiteMvc.Business.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Services.SurveyModule
{
    public class SurveyAdminService : ISurveyAdminService
    {
        private readonly VoteDbContext _context;
        private readonly ILogger<SurveyAdminService> _logger;

        public SurveyAdminService(
            VoteDbContext context,
            ILogger<SurveyAdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SurveyOverviewDto>> GetAllSurveysAsync()
        {
            try
            {
                var surveys = await _context.Surveys
                    .Include(s => s.Responses)
                    .AsNoTracking()
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                return surveys.Select(s => new SurveyOverviewDto
                {
                    SurveyId = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    TotalResponses = s.Responses.Count,
                    CompletedResponses = s.Responses.Count(r => r.IsCompleted),
                    PendingResponses = s.Responses.Count(r => !r.IsCompleted),
                    CompletionRate = s.Responses.Count > 0
                        ? Math.Round((decimal)s.Responses.Count(r => r.IsCompleted) / s.Responses.Count * 100, 2)
                        : 0,
                    CreatedAt = s.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting survey overview");
                throw;
            }
        }

        public async Task<SurveyAnalyticsDto> GetSurveyAnalyticsAsync(Guid surveyId)
        {
            try
            {
                var survey = await _context.Surveys
                    .Include(s => s.Responses)
                    .Include(s => s.Questions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == surveyId);

                if (survey == null) return null;

                var emailTracking = await _context.SurveyEmailTrackings
                    .Where(t => t.Response.SurveyId == surveyId)
                    .AsNoTracking()
                    .ToListAsync();

                var totalInvites = emailTracking.Count;
                var emailsOpened = emailTracking.Count(t => t.OpenedAt.HasValue);
                var linksClicked = emailTracking.Count(t => t.ClickedAt.HasValue);
                var surveysStarted = survey.Responses.Count;
                var surveysCompleted = survey.Responses.Count(r => r.IsCompleted);

                var completedResponses = survey.Responses
                    .Where(r => r.IsCompleted && r.CompletedAt.HasValue)
                    .ToList();

                var avgCompletionTime = completedResponses.Any()
                    ? completedResponses
                        .Select(r => (r.CompletedAt.Value - r.StartedAt).TotalMinutes)
                        .Average()
                    : 0;

                // Question-level analytics
                var questionAnalytics = new List<QuestionAnalyticsDto>();
                foreach (var question in survey.Questions.OrderBy(q => q.DisplayOrder))
                {
                    var answers = await _context.SurveyAnswers
                        .Where(a => a.QuestionId == question.Id)
                        .ToListAsync();

                    var analytics = new QuestionAnalyticsDto
                    {
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType,
                        TotalAnswers = answers.Count
                    };

                    if (question.QuestionType == QuestionType.MultipleChoice ||
                        question.QuestionType == QuestionType.YesNo)
                    {
                        analytics.OptionCounts = answers
                            .GroupBy(a => a.AnswerText)
                            .ToDictionary(g => g.Key ?? "No Answer", g => g.Count());
                    }
                    else if (question.QuestionType == QuestionType.Scale ||
                             question.QuestionType == QuestionType.Rating)
                    {
                        var numericAnswers = answers.Where(a => a.AnswerNumeric.HasValue).ToList();
                        analytics.AverageScore = numericAnswers.Any()
                            ? numericAnswers.Average(a => a.AnswerNumeric.Value)
                            : null;
                        analytics.ScoreDistribution = numericAnswers
                            .GroupBy(a => a.AnswerNumeric.Value)
                            .ToDictionary(g => g.Key, g => g.Count());
                    }
                    else if (question.QuestionType == QuestionType.Text)
                    {
                        analytics.TextResponses = answers
                            .Where(a => !string.IsNullOrEmpty(a.AnswerText))
                            .Select(a => a.AnswerText)
                            .ToList();
                    }

                    questionAnalytics.Add(analytics);
                }

                return new SurveyAnalyticsDto
                {
                    SurveyId = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    TotalInvites = totalInvites,
                    EmailsOpened = emailsOpened,
                    LinksClicked = linksClicked,
                    SurveysStarted = surveysStarted,
                    SurveysCompleted = surveysCompleted,
                    OpenRate = totalInvites > 0 ? Math.Round((decimal)emailsOpened / totalInvites * 100, 2) : 0,
                    ClickRate = totalInvites > 0 ? Math.Round((decimal)linksClicked / totalInvites * 100, 2) : 0,
                    StartRate = linksClicked > 0 ? Math.Round((decimal)surveysStarted / linksClicked * 100, 2) : 0,
                    CompletionRate = surveysStarted > 0 ? Math.Round((decimal)surveysCompleted / surveysStarted * 100, 2) : 0,
                    AvgCompletionTimeMinutes = avgCompletionTime,
                    FirstResponseAt = completedResponses.Any() ? completedResponses.Min(r => r.CompletedAt) : null,
                    LastResponseAt = completedResponses.Any() ? completedResponses.Max(r => r.CompletedAt) : null,
                    Questions = questionAnalytics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting analytics for survey {surveyId}");
                throw;
            }
        }

        public async Task<SurveyResponsesPagedDto> GetSurveyResponsesAsync(Guid surveyId, int page, int pageSize)
        {
            try
            {
                var survey = await _context.Surveys
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == surveyId);

                if (survey == null) return null;

                var query = _context.SurveyResponses
                    .Include(r => r.EmailTracking)
                    .Where(r => r.SurveyId == surveyId)
                    .OrderByDescending(r => r.StartedAt);

                var totalResponses = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalResponses / pageSize);

                var responses = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new SurveyResponsesPagedDto
                {
                    SurveyId = surveyId,
                    SurveyTitle = survey.Title,
                    Responses = responses.Select(r => new ResponseSummaryDto
                    {
                        ResponseId = r.Id,
                        UserEmail = r.UserEmail,
                        VoteId = r.VoteId,
                        IsCompleted = r.IsCompleted,
                        StartedAt = r.StartedAt,
                        CompletedAt = r.CompletedAt,
                        CompletionTime = r.CompletedAt.HasValue
                            ? r.CompletedAt.Value - r.StartedAt
                            : null,
                        IpAddress = r.IpAddress,
                        UserAgent = r.UserAgent,
                        EmailStatus = r.EmailTracking?.EmailStatus ?? EmailStatus.Sent
                    }).ToList(),
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalResponses = totalResponses,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting responses for survey {surveyId}");
                throw;
            }
        }

        public async Task<ResponseDetailDto> GetResponseDetailAsync(Guid responseId)
        {
            try
            {
                var response = await _context.SurveyResponses
                    .Include(r => r.Survey)
                    .Include(r => r.EmailTracking)
                    .Include(r => r.Answers)
                        .ThenInclude(a => a.Question)
                        .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == responseId);

                if (response == null) return null;

                return new ResponseDetailDto
                {
                    ResponseId = response.Id,
                    SurveyId = response.SurveyId,
                    SurveyTitle = response.Survey.Title,
                    UserEmail = response.UserEmail,
                    VoteId = response.VoteId,
                    IsCompleted = response.IsCompleted,
                    StartedAt = response.StartedAt,
                    CompletedAt = response.CompletedAt,
                    CompletionTime = response.CompletedAt.HasValue
                        ? response.CompletedAt.Value - response.StartedAt
                        : null,
                    IpAddress = response.IpAddress,
                    UserAgent = response.UserAgent,
                    EmailSentAt = response.EmailTracking?.SentAt,
                    EmailOpenedAt = response.EmailTracking?.OpenedAt,
                    EmailClickedAt = response.EmailTracking?.ClickedAt,
                    EmailStatus = response.EmailTracking?.EmailStatus ?? EmailStatus.Sent,
                    Answers = response.Answers
                        .OrderBy(a => a.Question.DisplayOrder)
                        .Select(a => new AnswerDetailDto
                        {
                            QuestionId = a.QuestionId,
                            QuestionText = a.Question.QuestionText,
                            QuestionType = a.Question.QuestionType,
                            AnswerText = a.AnswerText,
                            AnswerNumeric = a.AnswerNumeric,
                            AnsweredAt = a.AnsweredAt
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting response detail {responseId}");
                throw;
            }
        }

        public async Task<ExportFileDto> ExportSurveyDataAsync(Guid surveyId)
        {
            try
            {
                var survey = await _context.Surveys
                    .Include(s => s.Questions)
                    .Include(s => s.Responses)
                        .ThenInclude(r => r.Answers)
                            .ThenInclude(a => a.Question)
                            .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == surveyId);

                if (survey == null) return null;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();

                // Sheet 1: Summary
                var summarySheet = package.Workbook.Worksheets.Add("Summary");
                summarySheet.Cells["A1"].Value = "Survey Title";
                summarySheet.Cells["B1"].Value = survey.Title;
                summarySheet.Cells["A2"].Value = "Total Responses";
                summarySheet.Cells["B2"].Value = survey.Responses.Count;
                summarySheet.Cells["A3"].Value = "Completed";
                summarySheet.Cells["B3"].Value = survey.Responses.Count(r => r.IsCompleted);
                summarySheet.Cells["A4"].Value = "Completion Rate";
                summarySheet.Cells["B4"].Value = survey.Responses.Count > 0
                    ? $"{Math.Round((decimal)survey.Responses.Count(r => r.IsCompleted) / survey.Responses.Count * 100, 2)}%"
                    : "0%";

                summarySheet.Cells["A1:A4"].Style.Font.Bold = true;
                summarySheet.Columns[1, 2].AutoFit();

                // Sheet 2: All Responses
                var responsesSheet = package.Workbook.Worksheets.Add("All Responses");
                int row = 1;
                int col = 1;

                // Headers
                responsesSheet.Cells[row, col++].Value = "Response ID";
                responsesSheet.Cells[row, col++].Value = "Email";
                responsesSheet.Cells[row, col++].Value = "Vote ID";
                responsesSheet.Cells[row, col++].Value = "Started At";
                responsesSheet.Cells[row, col++].Value = "Completed At";
                responsesSheet.Cells[row, col++].Value = "Completion Time";
                responsesSheet.Cells[row, col++].Value = "IP Address";
                responsesSheet.Cells[row, col++].Value = "Status";

                // Add question columns
                var questions = survey.Questions.OrderBy(q => q.DisplayOrder).ToList();
                foreach (var question in questions)
                {
                    responsesSheet.Cells[row, col++].Value = $"Q{question.DisplayOrder}: {question.QuestionText}";
                }

                responsesSheet.Cells[1, 1, 1, col - 1].Style.Font.Bold = true;
                responsesSheet.Cells[1, 1, 1, col - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                responsesSheet.Cells[1, 1, 1, col - 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Data rows
                row = 2;
                foreach (var response in survey.Responses.OrderByDescending(r => r.StartedAt))
                {
                    col = 1;
                    responsesSheet.Cells[row, col++].Value = response.Id.ToString();
                    responsesSheet.Cells[row, col++].Value = response.UserEmail;
                    responsesSheet.Cells[row, col++].Value = response.VoteId?.ToString() ?? "";
                    responsesSheet.Cells[row, col++].Value = response.StartedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    responsesSheet.Cells[row, col++].Value = response.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    responsesSheet.Cells[row, col++].Value = response.CompletedAt.HasValue
                        ? $"{(response.CompletedAt.Value - response.StartedAt).TotalMinutes:F2} min"
                        : "";
                    responsesSheet.Cells[row, col++].Value = response.IpAddress ?? "";
                    responsesSheet.Cells[row, col++].Value = response.IsCompleted ? "Completed" : "Incomplete";

                    // Add answers
                    foreach (var question in questions)
                    {
                        var answer = response.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                        if (answer != null)
                        {
                            responsesSheet.Cells[row, col++].Value = answer.AnswerNumeric.HasValue
                                ? answer.AnswerNumeric.Value.ToString()
                                : answer.AnswerText ?? "";
                        }
                        else
                        {
                            col++;
                        }
                    }

                    row++;
                }

                responsesSheet.Cells.AutoFitColumns();

                var fileName = $"Survey_{survey.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileContents = package.GetAsByteArray();

                return new ExportFileDto
                {
                    FileContents = fileContents,
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting survey {surveyId}");
                throw;
            }
        }

        public async Task<SurveyChartDataDto> GetChartDataAsync(Guid surveyId)
        {
            try
            {
                var survey = await _context.Surveys
                    .Include(s => s.Responses)
                    .Include(s => s.Questions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == surveyId);

                if (survey == null) return null;

                var emailTracking = await _context.SurveyEmailTrackings
                    .Where(t => t.Response.SurveyId == surveyId)
                    .AsNoTracking()
                    .ToListAsync();

                // Responses over time
                var responsesOverTime = survey.Responses
                    .Where(r => r.IsCompleted)
                    .GroupBy(r => r.CompletedAt.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new ChartDataPoint
                    {
                        Label = g.Key.ToString("MMM dd"),
                        Value = g.Count(),
                        Color = "#198754"
                    })
                    .ToList();

                // Completion funnel
                var totalInvites = emailTracking.Count;
                var emailsOpened = emailTracking.Count(t => t.OpenedAt.HasValue);
                var linksClicked = emailTracking.Count(t => t.ClickedAt.HasValue);
                var surveysStarted = survey.Responses.Count;
                var surveysCompleted = survey.Responses.Count(r => r.IsCompleted);

                var completionFunnel = new List<ChartDataPoint>
                {
                    new ChartDataPoint { Label = "Invites Sent", Value = totalInvites, Color = "#6c757d" },
                    new ChartDataPoint { Label = "Emails Opened", Value = emailsOpened, Color = "#0d6efd" },
                    new ChartDataPoint { Label = "Links Clicked", Value = linksClicked, Color = "#fd7e14" },
                    new ChartDataPoint { Label = "Surveys Started", Value = surveysStarted, Color = "#ffc107" },
                    new ChartDataPoint { Label = "Surveys Completed", Value = surveysCompleted, Color = "#198754" }
                };

                return new SurveyChartDataDto
                {
                    ResponsesOverTime = responsesOverTime,
                    CompletionFunnel = completionFunnel,
                    QuestionCharts = new Dictionary<string, List<ChartDataPoint>>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chart data for survey {surveyId}");
                throw;
            }
        }
    }
}
