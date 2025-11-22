using AddWebsiteMvc.Interfaces;
using AddWebsiteMvc.Models;
using Newtonsoft.Json;

namespace AddWebsiteMvc.Services
{
    public class ElectionService:IElectionService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthUser _authUser;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ElectionService> _logger;
        public ElectionService(IConfiguration configuration, HttpClient httpClient, IAuthUser authUser, ILogger<ElectionService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _authUser = authUser;
            _logger = logger;
        }

        public async Task<GetElectionResponse> GetActiveElectionAsync()
        {
            GetElectionResponse result = new();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["GetElectionEndpoint"]);
                request.Headers.Add("Authorization", $"Bearer {_authUser.Token}");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<GetElectionResponse>(json)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                result.errors.Add("Failed to load all contestants by this time");
            }

            return result;
        }
    }
}
