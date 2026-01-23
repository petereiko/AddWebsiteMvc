using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.SurveyModels.AdminDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AddWebsiteMvc.Controllers
{
    //[Area("Admin")]
    //[Authorize(Roles = "Admin")] // Ensure only admins can access
    //[Route("adminsurvey")]
    public class AdminSurveyController : Controller
    {
        private readonly ISurveyService _surveyService;
        private readonly ISurveyAdminService _surveyAdminService;
        private readonly ILogger<AdminSurveyController> _logger;

        public AdminSurveyController(
            ISurveyService surveyService,
            ISurveyAdminService surveyAdminService,
            ILogger<AdminSurveyController> logger)
        {
            _surveyService = surveyService;
            _surveyAdminService = surveyAdminService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard - Overview of all surveys
        /// GET: /admin/surveys
        /// </summary>
        //[HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var surveys = await _surveyAdminService.GetAllSurveysAsync();
                return View(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading surveys dashboard");
                TempData["ErrorMessage"] = "Error loading surveys";
                return View(new List<SurveyOverviewDto>());
            }
        }

        /// <summary>
        /// Survey Analytics - Detailed stats and charts
        /// GET: /admin/surveys/{surveyId}/analytics
        /// </summary>
        [HttpGet("{surveyId}/analytics")]
        public async Task<IActionResult> Analytics(Guid surveyId)
        {
            try
            {
                var analytics = await _surveyAdminService.GetSurveyAnalyticsAsync(surveyId);

                if (analytics == null)
                {
                    TempData["ErrorMessage"] = "Survey not found";
                    return RedirectToAction("Index");
                }

                return View(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading analytics for survey {surveyId}");
                TempData["ErrorMessage"] = "Error loading analytics";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Survey Responses - List all responses
        /// GET: /admin/surveys/{surveyId}/responses
        /// </summary>
        [HttpGet("{surveyId}/responses")]
        public async Task<IActionResult> Responses(Guid surveyId, int page = 1, int pageSize = 20)
        {
            try
            {
                var responses = await _surveyAdminService.GetSurveyResponsesAsync(
                    surveyId, page, pageSize);

                if (responses == null)
                {
                    TempData["ErrorMessage"] = "Survey not found";
                    return RedirectToAction("Index");
                }

                return View(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading responses for survey {surveyId}");
                TempData["ErrorMessage"] = "Error loading responses";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Individual Response Details
        /// GET: /admin/surveys/response/{responseId}
        /// </summary>
        [HttpGet("response/{responseId}")]
        public async Task<IActionResult> ResponseDetail(Guid responseId)
        {
            try
            {
                var response = await _surveyAdminService.GetResponseDetailAsync(responseId);

                if (response == null)
                {
                    TempData["ErrorMessage"] = "Response not found";
                    return RedirectToAction("Index");
                }

                return View(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading response {responseId}");
                TempData["ErrorMessage"] = "Error loading response";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Export Survey Data to Excel
        /// GET: /admin/surveys/{surveyId}/export
        /// </summary>
        [HttpGet("{surveyId}/export")]
        public async Task<IActionResult> Export(Guid surveyId)
        {
            try
            {
                var fileData = await _surveyAdminService.ExportSurveyDataAsync(surveyId);

                if (fileData == null)
                {
                    TempData["ErrorMessage"] = "Survey not found or no data to export";
                    return RedirectToAction("Analytics", new { surveyId });
                }

                return File(
                    fileData.FileContents,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileData.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting survey {surveyId}");
                TempData["ErrorMessage"] = "Error exporting data";
                return RedirectToAction("Analytics", new { surveyId });
            }
        }

        /// <summary>
        /// API endpoint for chart data
        /// GET: /admin/surveys/{surveyId}/chart-data
        /// </summary>
        [HttpGet("{surveyId}/chart-data")]
        public async Task<IActionResult> GetChartData(Guid surveyId)
        {
            try
            {
                var chartData = await _surveyAdminService.GetChartDataAsync(surveyId);
                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading chart data for survey {surveyId}");
                return Json(new { error = "Error loading chart data" });
            }
        }
    }
}
