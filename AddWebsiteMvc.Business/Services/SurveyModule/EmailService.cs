using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Entities.SurveyEntity;
using AddWebsiteMvc.Business.Enums;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Models.SurveyModels;
using AddWebsiteMvc.Business.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Services.SurveyModule
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IAuthUser _authUser;
        private readonly VoteDbContext _context;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            VoteDbContext context,
            IWebHostEnvironment environment,
            IAuthUser authUser)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _environment = environment;
            _authUser = authUser;
        }


        public async Task<bool> SendSurveyInvitationAsync(string userEmail, string userName, Guid voteId)
        {
            try
            {
                // Generate unique survey token for this voter
                var token = GenerateSurveyToken(voteId, userEmail);

                var encodedToken = WebUtility.UrlEncode(token);

                // Build the survey URL with token
                var baseUrl = _authUser.BaseUrl;
                var surveyUrl = $"{baseUrl}/survey?token={encodedToken}";

                var subject = "NIGERIA GOVERNORS SCORECARD, We'd Love Your Feedback";
                var htmlBody = await GetSurveyInvitationTemplate(userName, surveyUrl);

                var sent = await SendEmailAsync(userEmail, subject, htmlBody);

                if (sent)
                {
                    _logger.LogInformation($"Survey invitation sent to {userEmail}");

                    // Track the email in database
                    await TrackSurveyEmailAsync(voteId, userEmail, token);
                }

                return sent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending survey invitation to {userEmail}");
                return false;
            }
        }

        private string GenerateSurveyToken(Guid voteId, string email)
        {
            // Generate a unique token combining voteId and timestamp
            var payload = $"{voteId}|{email}|{DateTime.UtcNow.Ticks}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
       
        public static SurveyTokenData DecodeToken(string token)
                {
                    try
                    {
                        // Reverse the URL-safe replacements
                        var base64 = token.Replace('-', '+').Replace('_', '/');

                        // Add padding if needed
                        switch (base64.Length % 4)
                        {
                            case 2: base64 += "=="; break;
                            case 3: base64 += "="; break;
                            case 1: // invalid base64 length; fall through to throw below
                                break;
                        }

                        var bytes = Convert.FromBase64String(base64);
                        var payload = System.Text.Encoding.UTF8.GetString(bytes);

                        // Split the payload: voteId|email|timestamp
                        var parts = payload.Split('|');

                        if (parts.Length != 3)
                        {
                            throw new InvalidOperationException("Invalid token format");
                        }

                        var voteId = Guid.Parse(parts[0]);
                        var email = parts[1];
                        var ticks = long.Parse(parts[2]);

                        return new SurveyTokenData
                        {
                            VoteId = voteId,
                            Email = email,
                            Timestamp = ticks,
                            // Keep CreatedAt in UTC to match generation using DateTime.UtcNow.Ticks
                            CreatedAt = new DateTime(ticks, DateTimeKind.Utc)
                        };
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("Invalid or corrupted token");
                    }
                }

        private async Task TrackSurveyEmailAsync(Guid voteId, string email, string token)
        {
            try
            {
                // Get the active survey
                var activeSurvey = await _context.Surveys
                    .FirstOrDefaultAsync(s => s.IsActive);

                if (activeSurvey == null)
                {
                    _logger.LogWarning("No active survey found to track email");
                    return;
                }

                // Create SurveyResponse
                var response = new SurveyResponse
                {
                    SurveyId = activeSurvey.Id,
                    UserEmail = email,
                    VoteId = voteId,
                    ResponseToken = Guid.NewGuid(),
                    StartedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    IpAddress = null, // Will be captured when they click the link
                    UserAgent = null  // Will be captured when they click the link
                };

                _context.SurveyResponses.Add(response);
                await _context.SaveChangesAsync();

                // Create email tracking
                var tracking = new SurveyEmailTracking
                {
                    ResponseId = response.Id,
                    UserEmail = email,
                    Token = token,
                    SentAt = DateTime.UtcNow,
                    EmailStatus = EmailStatus.Sent
                };

                _context.SurveyEmailTrackings.Add(tracking);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Survey response and email tracking created for {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error tracking survey email for {email}");
            }
        }

        //private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        //{
        //    try
        //    {
        //        var smtpServer = _configuration["Email:SmtpServer"];
        //        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        //        var smtpUsername = _configuration["Email:SmtpUsername"];
        //        var smtpPassword = _configuration["Email:SmtpPassword"];
        //        var fromEmail = _configuration["Email:FromEmail"];
        //        var fromName = _configuration["Email:FromName"] ?? "NIGERIA GOVERNORS SCORECARD AWARDS by Adda Consults";

        //        using var client = new SmtpClient(smtpServer, smtpPort)
        //        {
        //            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        //            EnableSsl = true
        //        };

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(fromEmail, fromName),
        //            Subject = subject,
        //            Body = htmlBody,
        //            IsBodyHtml = true
        //        };

        //        mailMessage.To.Add(toEmail);

        //        await client.SendMailAsync(mailMessage);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Failed to send email to {toEmail}");
        //        return false;
        //    }
        //}

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            // Create email log entry
            var emailLog = new EmailLog
            {
                Email = toEmail,
                Subject = subject,
                Message = htmlBody,
                CreatedDate = DateTime.UtcNow,
                IsSent = false,
                SentDate = null
            };

            try
            {
                // Add to database before sending (in case of failure)
                _context.EmailLogs.Add(emailLog);
                await _context.SaveChangesAsync();

                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"] ?? "NIGERIA GOVERNORS SCORECARD AWARDS by Adda Consults";

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                // Update log as sent
                emailLog.IsSent = true;
                emailLog.SentDate = DateTime.UtcNow;
                _context.EmailLogs.Update(emailLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Email sent successfully to {toEmail}. Subject: {subject}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}. Subject: {subject}");

                // Log is already in database with IsSent = false
                // Optionally, you could add an error message field to EmailLog
                return false;
            }
        }

        private async Task<string> GetSurveyInvitationTemplate(string userName, string surveyUrl)
        {
            string template = await File.ReadAllTextAsync(Path.Combine(_environment.WebRootPath, "EmailTemplates", "sendInvitation.html"));
            template = template.Replace("{userName}", userName)
                .Replace("{surveyUrl}", surveyUrl);

            return template;
        }

    }
}
