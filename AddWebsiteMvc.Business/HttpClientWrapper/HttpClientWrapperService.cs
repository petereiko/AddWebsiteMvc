using AddWebsiteMvc.Business.Common;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AddWebsiteMvc.Business.HttpClientWrapper
{
    public class HttpClientWrapperService : IHttpClientWrapperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientWrapperService> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public HttpClientWrapperService(HttpClient httpClient, ILogger<HttpClientWrapperService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryPolicy = GetRetryPolicy();
        }

        #region GET Methods

        public async Task<HttpResponseMessage> GetAsync(
            string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                _logger.LogInformation("Sending GET request to {Url}", url);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                LogResponse(response, url);
                return response;
            });
        }

        public async Task<T?> GetAsync<T>(
            string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var response = await GetAsync(url, headers, cancellationToken);
            return await DeserializeResponse<T>(response);
        }

        #endregion

        #region POST Methods

        public async Task<HttpResponseMessage> PostAsync(
            string url,
            object payload,
            Dictionary<string, string>? headers = null,
            string mediaType = MediaTypeConstants.APPJSON,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await ExecuteCallAsync(url, payload, headers, mediaType, cancellationToken);
            });
        }


        public async Task<HttpResponseMessage> ExecuteCallAsync(string url, object payload, Dictionary<string, string>? headers, string mediaType, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddHeaders(request, headers);

            HttpContent? content = null;

            request.Headers.Accept.Add(
    new MediaTypeWithQualityHeaderValue(mediaType));

            switch (mediaType)
            {
                case MediaTypeConstants.APPJSON:
                    content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    mediaType);
                    
                    break;
                case MediaTypeConstants.APPXML:
                    content = new StringContent(
                    payload.ToString(),
                    Encoding.UTF8,
                    mediaType);
                    break;
                case MediaTypeConstants.TXTPLAIN:
                    content = new StringContent(
                    payload.ToString(), null,
                    mediaType);


                    if (headers != null)
                        foreach (var item in headers)
                        {
                            _httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    break;
                default:
                    throw new ArgumentException("Unsupported content type");
            }

            request.Content = content;

            _logger.LogInformation("Sending POST request to {Url}", url);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            LogResponse(response, url);
            return response;
        }


        public async Task<T?> PostAsync<T>(
            string url,
            object payload,
            Dictionary<string, string>? headers = null,
            string mediaType = MediaTypeConstants.APPJSON,
            CancellationToken cancellationToken = default)
        {
            var response = await PostAsync(url, payload, headers, mediaType, cancellationToken);
            return await DeserializeResponse<T>(response);
        }


        #endregion

        #region PUT Methods

        public async Task<HttpResponseMessage> PutAsync(
            string url,
            object payload,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, url);
                AddHeaders(request, headers);

                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending PUT request to {Url}", url);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                LogResponse(response, url);
                return response;
            });
        }

        #endregion

        #region DELETE Methods

        public async Task<HttpResponseMessage> DeleteAsync(
            string url,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            ValidateUrl(url);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AddHeaders(request, headers);

                _logger.LogInformation("Sending DELETE request to {Url}", url);
                var response = await _httpClient.SendAsync(request, cancellationToken);

                LogResponse(response, url);
                return response;
            });
        }

        #endregion

        #region Private Helper Methods

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // Retry policy: 3 retries with exponential backoff
            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode &&
                    (r.StatusCode >= HttpStatusCode.InternalServerError || // 5xx
                     r.StatusCode == HttpStatusCode.RequestTimeout ||      // 408
                     r.StatusCode == HttpStatusCode.TooManyRequests))      // 429
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var statusCode = outcome.Result?.StatusCode ?? HttpStatusCode.RequestTimeout;
                        var exception = outcome.Exception?.Message ?? "No exception";

                        _logger.LogWarning(
                            "Request failed with {StatusCode} or Exception: {Exception}. Waiting {Delay}s before retry {RetryCount}",
                            statusCode,
                            exception,
                            timespan.TotalSeconds,
                            retryCount);
                    });

            // Timeout policy: 30 seconds per request
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(30),
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _logger.LogError("Request timed out after {Timeout} seconds", timespan.TotalSeconds);
                    return Task.CompletedTask;
                });

            // Combine policies: timeout wraps retry
            return Policy.WrapAsync(retryPolicy, timeoutPolicy);
        }

        private void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                throw new UriFormatException($"Invalid URL format: {url}");
            }
        }

        private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
                return;

            foreach (var header in headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Content-Type is handled by StringContent
                    continue;
                }

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private void LogResponse(HttpResponseMessage response, string url)
        {
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Request to {Url} succeeded with status code {StatusCode}",
                    url,
                    response.StatusCode);
                _logger.LogInformation($"Request to {url} succeeded with status code {response.StatusCode}");
            }
            else
            {
                _logger.LogWarning(
                    "Request to {Url} failed with status code {StatusCode}",
                    url,
                    response.StatusCode);
            }
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Request failed with status code {response.StatusCode}. Response: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response content");
                throw new InvalidOperationException("Failed to deserialize response", ex);
            }
        }

        #endregion
    }
}
