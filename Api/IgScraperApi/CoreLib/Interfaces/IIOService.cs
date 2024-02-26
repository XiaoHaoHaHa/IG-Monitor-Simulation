using CoreLib.DataModels;
using System.Net;

namespace CoreLib.Interfaces
{
    public interface IIOService
    {
        string ExtractJsonStrings(string directoryName, string username, string followType);
        (bool, string message, string[]) GetAllScrapersFiles(string directoryName);
        Task<(bool, string)> ScraperInfoToFileAsync(string ig_id, string pictureUrl, CookieContainer cookieContainer);
        Task<(bool, string)> FollowingInfoToFileAsync(User followingAccount, string ig_id, string addOrDelete, bool isFirstTime = false);
        Task<(bool, string)> FollowerInfoToFileAsync(User follower, string ig_id, string addOrDelete, bool isFirstTime = false);
    }
}
