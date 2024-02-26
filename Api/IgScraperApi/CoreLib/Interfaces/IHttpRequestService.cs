using System.Net;

namespace CoreLib.Interfaces
{
    public interface IHttpRequestService
    {
        Task<(bool, string)> GetUrlPictureAsync(string url);
        Task<(bool, string)> GetAsync(string url, CookieContainer cookieContainer = null);
        Task<(bool, string)> PostAsync(string url, string jsonContent, CookieContainer cookieContainer = null);
    }
}
