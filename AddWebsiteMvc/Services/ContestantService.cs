using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;

namespace AddWebsiteMvc.Services
{
    public class ContestantService:IContestantService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthUser _authUser;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContestantService> _logger;
        private readonly IWebHostEnvironment _env;
        public ContestantService(IConfiguration configuration, HttpClient httpClient, IAuthUser authUser, ILogger<ContestantService> logger, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _authUser = authUser;
            _logger = logger;
            _env = env;
        }

        public async Task<GetAllContestantResponse> GetAllContestantsAsync()
        {
            GetAllContestantResponse result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["GetAllContestantsEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<GetAllContestantResponse>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all contestants by this time");
            }
            
            return result;
        }

        public async Task<BaseResponse<Contestant>> GetContestantByIdAsync(string id)
        {
            BaseResponse<Contestant> result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["GetContestantByIdEndpoint"].Replace("{id}", id));
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Contestant>>(json)!;
                result.data.dob = Convert.ToDateTime(result.data.dob).ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load contestant by this time");
            }

            return result;
        }

        public async Task<BaseResponse<Contestant>>AddAsync(Contestant model, CancellationToken cancellationToken)
        {
            BaseResponse<Contestant> result = new() { data = new() { } };
            try
            {
                if (model == null)
                {
                    result.data.Errors.Add("Invalid request");
                    return result;
                }
                if (string.IsNullOrEmpty(model.shortNote))
                {
                    result.data.Errors.Add("Short note is required");
                    return result;
                }
                if (string.IsNullOrEmpty(model.talent))
                {
                    result.data.Errors.Add("talent is required");
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
                if (string.IsNullOrEmpty(model.dob))
                {
                    result.data.Errors.Add("DOB is required");
                    return result;
                }
                //Process Image File
                if (model.passportFile == null)
                {
                    result.data.Errors.Add("Please upload the contestant's passport");
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

                //Process Video File
                if (model.videoFile == null)
                {
                    result.data.Errors.Add("Please upload the contestant's video");
                    return result;
                }

                allowedExtensions = [".mp4", ".mov", ".avi", ".wmv", ".mpeg", ".flv", ".mpg"];
                ext = Path.GetExtension(model.videoFile.FileName);
                if (!allowedExtensions.Contains(ext))
                {
                    result.data.Errors.Add($"Only {string.Join(',', allowedExtensions)} video files are allowed");
                    return result;
                }

                fileSizeInBytes = model.videoFile.Length;
                fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);
                if (fileSizeInMB > 50)
                {
                    result.data.Errors.Add("Video File too large.");
                    return result;
                }

                fileName = $"{Guid.NewGuid().ToString()}{ext}";
                filePath = Path.Combine(_env.WebRootPath, "Videos", fileName);
                fs = new FileStream(filePath, FileMode.Create);
                await model.videoFile.CopyToAsync(fs, cancellationToken);
                fs.Close();
                model.videoFileName = fileName;

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["CreateContestantsEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");

                DateTime dobDate = Convert.ToDateTime(model.dob);

                var payload = new
                {
                    firstName = model.firstName,
                    lastName = model.lastName,
                    dob = dobDate,
                    passportFileName = model.passportFileName,
                    videoFileName = model.videoFileName,
                    shortNote = model.shortNote,
                    talent = model.talent,
                    order = model.order
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Contestant>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all contestants by this time");
            }

            return result;
        }

        public async Task<BaseResponse<Contestant>> EditAsync(Contestant model, CancellationToken cancellationToken)
        {
            BaseResponse<Contestant> result = new() { data = new() { }, statusCode = 400 };
            try
            {
                if (model == null)
                {
                    result.data.Errors.Add("Invalid request");
                    return result;
                }
                if (string.IsNullOrEmpty(model.shortNote))
                {
                    result.data.Errors.Add("Short note is required");
                    return result;
                }
                if (string.IsNullOrEmpty(model.talent))
                {
                    result.data.Errors.Add("talent is required");
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
                if (string.IsNullOrEmpty(model.dob))
                {
                    result.data.Errors.Add("DOB is required");
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

                //Process Video File
                if (model.videoFile != null)
                {
                    allowedExtensions = [".mp4"];
                    ext = Path.GetExtension(model.videoFile.FileName);
                    if (!allowedExtensions.Contains(ext))
                    {
                        result.data.Errors.Add($"Only {string.Join(',', allowedExtensions)} video files are allowed");
                        return result;
                    }

                    fileSizeInBytes = model.videoFile.Length;
                    fileSizeInMB = fileSizeInBytes / (1024.0 * 1024.0);
                    if (fileSizeInMB > 50)
                    {
                        result.data.Errors.Add("Video File too large.");
                        return result;
                    }

                    fileName = $"{Guid.NewGuid().ToString()}{ext}";
                    filePath = Path.Combine(_env.WebRootPath, "Videos", fileName);
                    fs = new FileStream(filePath, FileMode.Create);
                    await model.videoFile.CopyToAsync(fs, cancellationToken);
                    fs.Close();
                    model.videoFileName = fileName;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["EditContestantsEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");

                DateTime dobDate = Convert.ToDateTime(model.dob);

                var payload = new
                {
                    id = model.id,
                    firstName = model.firstName,
                    lastName = model.lastName,
                    dob = dobDate,
                    passportFileName = model.passportFileName,
                    videoFileName = model.videoFileName,
                    shortNote = model.shortNote,
                    talent = model.talent,
                    IsActive = model.IsActive,
                    order = model.order
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<Contestant>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all contestants by this time");
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
                    contestantId = model.contestantId,
                    count = model.count,
                    email = model.email
                };
                var content = new StringContent(JsonConvert.SerializeObject(payload), null, "application/json");
                request.Content = content;
                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<BaseResponse<VoteResponseData>>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all contestants by this time");
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
                result.errors.Add("Failed to load contestant by this time");
            }

            return result;
        }


    }
}
