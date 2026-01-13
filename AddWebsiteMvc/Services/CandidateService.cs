using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;

namespace AddWebsiteMvc.Services
{
    public class CandidateService:ICandidateService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthUser _authUser;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CandidateService> _logger;
        private readonly IWebHostEnvironment _env;
        public CandidateService(IConfiguration configuration, HttpClient httpClient, IAuthUser authUser, ILogger<CandidateService> logger, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _authUser = authUser;
            _logger = logger;
            _env = env;
        }

        public async Task<GetAllCandidateResponse> GetAllCandidatesAsync()
        {
            GetAllCandidateResponse result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["GetAllCandidatesEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<GetAllCandidateResponse>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all Candidates by this time");
            }
            
            return result;
        }

        public async Task<BaseResponse<Candidate>> GetCandidateByIdAsync(string id)
        {
            BaseResponse<Candidate> result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["GetCandidateByIdEndpoint"].Replace("{id}", id));
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Candidate>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load Candidate by this time");
            }

            return result;
        }

        public async Task<BaseResponse<Candidate>>AddAsync(Candidate model, CancellationToken cancellationToken)
        {
            BaseResponse<Candidate> result = new() { data = new() { } };
            try
            {
                if (model == null)
                {
                    result.data.Errors.Add("Invalid request");
                    return result;
                }
                if (string.IsNullOrEmpty(model.firstName))
                {
                    result.data.Errors.Add("First Name is required");
                    return result;
                }
                if (string.IsNullOrEmpty(model.lastName))
                {
                    result.data.Errors.Add("Last Name is required");
                    return result;
                }
                //Process Image File
                if (model.passportFile == null)
                {
                    result.data.Errors.Add("Please upload the Candidate's passport");
                    return result;
                }
                string[] allowedExtensions = [".png", ".jpeg", ".jpg"];
                string ext = Path.GetExtension(model.passportFile.FileName);

                if (!allowedExtensions.Contains(ext))
                {
                    result.data.Errors.Add($"Only {string.Join(',', allowedExtensions)} video files are allowed");
                    return result;
                }
                long fileSizeInBytes = model.passportFile.Length;
                double fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);

                if (fileSizeInMB > 5)
                {
                    result.data.Errors.Add("Passport File too large.");
                    return result;
                }

                string fileName = $"{Guid.NewGuid().ToString()}{ext}";
                string filePath = Path.Combine(_env.WebRootPath, "Passports", fileName);
                FileStream fs = new FileStream(filePath, FileMode.Create);
                await model.passportFile.CopyToAsync(fs, cancellationToken);
                fs.Close();
                model.passportFileName = fileName;

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["CreateCandidateEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");


                var payload = new
                {
                    firstName = model.firstName,
                    lastName = model.lastName,
                    passportFileName = model.passportFileName,
                    order = model.order,
                    stateId = model.stateId,
                    title = model.title
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Candidate>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all Candidates by this time");
            }

            return result;
        }

        public async Task<BaseResponse<Candidate>> EditAsync(Candidate model, CancellationToken cancellationToken)
        {
            BaseResponse<Candidate> result = new() { data = new() { }, statusCode = 400 };
            try
            {
                if (model == null)
                {
                    result.data.Errors.Add("Invalid request");
                    return result;
                }
                if (string.IsNullOrEmpty(model.firstName))
                {
                    result.data.Errors.Add("First Name is required");
                    return result;
                }
                if (string.IsNullOrEmpty(model.lastName))
                {
                    result.data.Errors.Add("Last Name is required");
                    return result;
                }
                List<string> allowedExtensions = new();
                long fileSizeInBytes = 0;
                double fileSizeInMB = 0;
                string fileName = string.Empty;
                string filePath = string.Empty;
                string ext = string.Empty;
                FileStream? fs = null;
                //Process Image File
                if (model.passportFile != null)
                {
                    allowedExtensions = [".png", ".jpeg", ".jpg"];
                    ext = Path.GetExtension(model.passportFile.FileName);

                    if (!allowedExtensions.Contains(ext))
                    {
                        result.data.Errors.Add($"Only {string.Join(',', allowedExtensions)} video files are allowed");
                        return result;
                    }
                    fileSizeInBytes = model.passportFile.Length;
                    fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);

                    if (fileSizeInMB > 5)
                    {
                        result.data.Errors.Add("Passport File too large.");
                        return result;
                    }

                    fileName = $"{Guid.NewGuid().ToString()}{ext}";
                    filePath = Path.Combine(_env.WebRootPath, "Passports", fileName);
                    fs = new FileStream(filePath, FileMode.Create);
                    await model.passportFile.CopyToAsync(fs, cancellationToken);
                    fs.Close();
                    model.passportFileName = fileName;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["EditCandidateEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");


                var payload = new
                {
                    id = model.id,
                    firstName = model.firstName,
                    lastName = model.lastName,
                    passportFileName = model.passportFileName,
                    IsActive = model.IsActive,
                    order = model.order,
                    title = model.title,
                    stateId = model.stateId,
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Candidate>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all Candidates by this time");
            }

            return result;
        }

        public async Task<BaseResponse<VoteResponseData>> VoteAsync(VoteRequest model, CancellationToken cancellationToken)
        {
            BaseResponse<VoteResponseData> result = new() { data = new() { }, statusCode = 400 };
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["VoteEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");

                var payload = new
                {
                    firstName = model.firstName,
                    lastName = model.lastName,
                    candidateId = model.candidateId,
                    count = model.count,
                    email = model.email
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<BaseResponse<VoteResponseData>>(json)!;
                }
                else
                {
                    ErrorResponse error = JsonConvert.DeserializeObject<ErrorResponse>(json)!;
                    result.errors = error.errors;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all Candidates by this time");
            }

            return result;
        }

        public async Task<BaseResponse> ConfirmPayment(string reference)
        {
            BaseResponse result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["VerifyTransaction"].Replace("{reference}", reference));
                //request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load Candidate by this time");
            }

            return result;
        }


    }
}
