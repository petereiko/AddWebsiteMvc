using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.SurveyModels;
using AddWebsiteMvc.Business.Services.SurveyModule;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Web;

namespace AddWebsiteMvc.Controllers
{

    public class SurveyController : Controller
    {
        private readonly ISurveyService _surveyService;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(
            ISurveyService surveyService,
            ILogger<SurveyController> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        //[HttpGet("survey")]
        public async Task<IActionResult> Index(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Invalid survey link.";
                    return View("SurveyNotFound");
                }

                // Decode URL-encoded token
                var decodedToken = WebUtility.UrlDecode(token);

                // Validate token format
                SurveyTokenData tokenData;
                try
                {
                    tokenData = EmailService.DecodeToken(decodedToken);
                }
                catch (InvalidOperationException)
                {
                    _logger.LogWarning($"Invalid token format: {token}");
                    TempData["ErrorMessage"] = "Invalid or corrupted survey link.";
                    return View("SurveyNotFound");
                }

                // Check if token has expired (30 days)
                if (tokenData.IsExpired(30))
                {
                    _logger.LogWarning($"Expired token for vote {tokenData.VoteId}");
                    TempData["ErrorMessage"] = "This survey link has expired.";
                    return View("SurveyNotFound");
                }

                // Get client info
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                // Track click and update response with IP/UserAgent
                await _surveyService.TrackEmailClickAsync(decodedToken, ipAddress, userAgent);

                // Get survey by token
                var survey = await _surveyService.GetSurveyByTokenAsync(decodedToken);

                if (survey == null)
                {
                    TempData["ErrorMessage"] = "Survey not found or has been closed.";
                    return View("SurveyNotFound");
                }

                // Pass token to view
                ViewBag.Token = token; // URL-encoded token
                ViewBag.UserEmail = tokenData.Email;

                return View(survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading survey");
                TempData["ErrorMessage"] = "An error occurred while loading the survey.";
                return View("Error");
            }
        }

        [HttpPost("survey/submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([FromForm] SubmitSurveyViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please answer all required questions.";
                    return RedirectToAction("Index", new { token = model.Token });
                }

                // Decode token for validation
                var decodedToken = HttpUtility.UrlDecode(model.Token);

                try
                {
                    var tokenData = EmailService.DecodeToken(decodedToken);

                    // Check expiration
                    if (tokenData.IsExpired(30))
                    {
                        TempData["ErrorMessage"] = "This survey link has expired.";
                        return View("SurveyNotFound");
                    }
                }
                catch (InvalidOperationException)
                {
                    TempData["ErrorMessage"] = "Invalid survey link.";
                    return View("SurveyNotFound");
                }

                // Convert form data to SubmitSurveyDto
                var submitDto = new SubmitSurveyDto
                {
                    Token = decodedToken, // Use decoded token
                    Answers = model.Answers.Select(a => new AnswerDto
                    {
                        QuestionId = a.QuestionId,
                        AnswerText = a.AnswerText,
                        AnswerNumeric = a.AnswerNumeric
                    }).ToList()
                };

                var success = await _surveyService.SubmitSurveyAsync(submitDto);

                if (success)
                {
                    return View("ThankYou");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to submit survey. Please try again.";
                    return RedirectToAction("Index", new { token = model.Token });
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting survey");
                TempData["ErrorMessage"] = "An error occurred while submitting your response.";
                return RedirectToAction("Index", new { token = model.Token });
            }
        }

        [HttpGet("survey/track/open")]
        public async Task<IActionResult> TrackOpen(string token)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    var decodedToken = HttpUtility.UrlDecode(token);
                    await _surveyService.TrackEmailOpenAsync(decodedToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking email open");
            }

            // Return 1x1 transparent pixel
            var pixel = new byte[] {
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
        0x01, 0x00, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
        0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44,
        0x01, 0x00, 0x3B
    };

            return File(pixel, "image/gif");
        }
    }

}
