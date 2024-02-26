using CoreLib.DataModels;
using CoreLib.Dtos;
using System.Net;

namespace CoreLib.Interfaces
{
    public interface IFetchFollowSevice
    {
        Task<(bool, CompareFollowDto)> ProcessAsync(IgScraper igScraper);
        Task<(bool, string, List<User>)> FetchDataAsync(int queryType, CookieContainer cookieContainer);
    }
}
