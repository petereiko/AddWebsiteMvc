using AddWebsiteMvc.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.HttpClientWrapper
{
    public interface IHttpClientWrapperService
    {
        Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync(string url, object payload, Dictionary<string, string>? headers = null, string acceptType=MediaTypeConstants.APPJSON, CancellationToken cancellationToken = default);
        Task<T?> PostAsync<T>(string url, object payload, Dictionary<string, string>? headers = null, string mediaType = MediaTypeConstants.APPJSON, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PutAsync(string url, object payload, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAsync(string url, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    }
}
