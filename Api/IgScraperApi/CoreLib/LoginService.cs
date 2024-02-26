using CoreLib.DataModels;
using CoreLib.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CoreLib
{
    public class LoginService : ILoginService
    {
        private IHttpRequestService _httpRequest;
        private IIOService _iOService;

        public LoginService(IHttpRequestService httpRequest, IIOService iOService)
        {
            _httpRequest = httpRequest;
            _iOService = iOService;
        }

        public async Task LoadAllScrapersToTaskAsync(string directoryName)
        {           
            // 使用 loginService 執行初始化邏輯
            // ...
            (var fileResult, var fileMsg, var files) = _iOService.GetAllScrapersFiles(directoryName);
            if (fileResult)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var text = File.ReadAllText(file);
                        var scraper = JsonConvert.DeserializeObject<IgScraper>(text);

                        // 在這裡使用 'scraper' 物件進行後續處理
                        (bool result, string msg, IgScraper IgScraper) = await InitializeDataAsync(scraper.Sessionid);
                        Console.WriteLine($"{result}: {msg}");
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine($"讀取{file} 時發生錯誤: {ex.Message}");
                        continue;
                    }
                }
            }
            Console.WriteLine($"{fileMsg}");
        }

        public async Task<(bool, string, IgScraper)> InitializeDataAsync(string sessionId)
        {
            //驗證sessionId是否正確，若成功取得回傳的Cookie資訊
            var url = "https://www.instagram.com/graphql/query/?query_hash=60b755363b5c230111347a7a4e242001";
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("https://www.instagram.com"), new Cookie()
            {
                Name = "sessionid",
                Value = sessionId
            });
            cookieContainer.Add(new Uri("https://i.instagram.com"), new Cookie()
            {
                Name = "sessionid",
                Value = sessionId
            });

            (bool requestResult, string data) = await _httpRequest.GetAsync(url, cookieContainer);
            if (!requestResult)
                return (false, $"HttpError:{data}", null);

            var jobj = JsonConvert.DeserializeObject<JObject>(data)["data"]["user"];
            if(jobj.Count() == 0)
                return (false, "SessionId錯誤或者過期", null);

            //取得username
            var dsUserId = cookieContainer.GetAllCookies()["ds_user_id"].Value;
            (bool infoResult, string userData) = await _httpRequest.GetAsync($"https://i.instagram.com/api/v1/users/{dsUserId}/info/");//有時候要帶cookie
            if (!infoResult)
                return (false, $"HttpError:{userData}", null);

            var user = JsonConvert.DeserializeObject<dynamic>(userData);
            var username = (string)user.user.username;
            var pictureUrl = (string)user.user.profile_pic_url;

            if (string.IsNullOrEmpty(username))
                return (false, "找不到用戶名稱Id", null);

            var igScraper = new IgScraper()
            {
                DsUserId = dsUserId,
                Sessionid = sessionId,
                Ig_id = username,
                PictureUrl = pictureUrl,
                cookieContainer = cookieContainer
            };

            //放一線程寫入個人資料
            _iOService.ScraperInfoToFileAsync(igScraper.Ig_id, igScraper.PictureUrl, igScraper.cookieContainer);

            //取得成功後加入IgScrapers開始排程工作
            var addResult = IgScraper.IgScrapers.TryAdd(username, igScraper);
            if (!addResult)
            {
                return (true, "已經加入執行程序", IgScraper.IgScrapers.GetValueOrDefault(username));
            }

            return (true, $"已新增使用者{username}加入排程", igScraper);
        }
    }
}
