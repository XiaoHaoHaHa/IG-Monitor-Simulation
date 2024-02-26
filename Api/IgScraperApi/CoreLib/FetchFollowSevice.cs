using CoreLib.DataModels;
using CoreLib.Dtos;
using CoreLib.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;

namespace CoreLib
{
    public class FetchFollowSevice : IFetchFollowSevice
    {
        private IHttpRequestService _httpRequestService;
        private IIOService _iOService;
        private string _nextPage;

        public FetchFollowSevice(IHttpRequestService httpRequestService, IIOService iOService)
        {
            _httpRequestService = httpRequestService;
            _iOService = iOService;
        }

        public async Task<(bool, CompareFollowDto)> ProcessAsync(IgScraper igScraper)
        {
            //更新個人資料，這裡取消更新https://i.instagram.com/會鎖太多請求的用戶
            //var dsUserId = igScraper.DsUserId;
            //(bool infoResult, string userData) = await _httpRequestService.GetAsync($"https://i.instagram.com/api/v1/users/{dsUserId}/info/", igScraper.cookieContainer);//有時候要帶cookie
            //if (infoResult)
            //{
            //    var user = JsonConvert.DeserializeObject<dynamic>(userData);
            //    var username = (string)user.user.username;
            //    if (!string.IsNullOrEmpty(username))
            //    {
            //        igScraper.Ig_id = username;
            //        igScraper.PictureUrl = (string)user.user.profile_pic_url;
            //        //放一線程寫入個人資料
            //        _iOService.ScraperInfoToFileAsync(igScraper.Ig_id, igScraper.PictureUrl, igScraper.cookieContainer);
            //    }
            //}



            (bool fetchFollowerResult, string fetchFollowerMsg, List<User> newFollowers) = await FetchDataAsync(0, igScraper.cookieContainer);
            (bool fetchFollowingResult, string fetchFollowingMsg, List<User> newFollowings) = await FetchDataAsync(1, igScraper.cookieContainer);

            //第一次新增不比對
            if (igScraper.IsFirstAdd)
            {
                (bool followingResult, string followingMsg) = await _iOService.FollowingInfoToFileAsync(null, igScraper.Ig_id, null, true);
                if (!followingResult)
                {
                    Console.WriteLine(followingMsg);
                    return (false, null);
                }

                (bool followerResult, string followerMsg) = await _iOService.FollowerInfoToFileAsync(null, igScraper.Ig_id, null, true);
                if (!followerResult)
                {
                    Console.WriteLine(followerMsg);
                    return (false, null);
                }

                if (fetchFollowerResult && fetchFollowingResult)
                {
                    igScraper.Followers = newFollowers;
                    igScraper.Followings = newFollowings;
                    igScraper.IsFirstAdd = false;
                    return (true, null);
                }

                Console.WriteLine($"錯誤! Follower:{fetchFollowerMsg}, Following:{fetchFollowingMsg}");
                return (false, null);
            }

            var compareFollowDto = new CompareFollowDto();

            #region 比對粉絲

            if (fetchFollowerResult)
            {
                var deleteList = igScraper.Followers.Where(origin => !newFollowers.Any(next => next.Id == origin.Id)); // 查找origin中有而next中沒有的User
                compareFollowDto.FollowerDelete = new ConcurrentBag<User>(deleteList);
                if (compareFollowDto.FollowerDelete.Count() > 0)
                {
                    var tasks = compareFollowDto.FollowerDelete.Select(async followerDelete =>
                    {
                        await _iOService.FollowerInfoToFileAsync(followerDelete, igScraper.Ig_id, "粉絲退追");
                    });

                    await Task.WhenAll(tasks);
                }

                var addList = newFollowers.Where(next => !igScraper.Followers.Any(origin => next.Id == origin.Id)); // 查找next中有而origin中沒有的User
                compareFollowDto.FollowerAdd = new ConcurrentBag<User>(addList);
                if (compareFollowDto.FollowerAdd.Count() > 0)
                {
                    var tasks = compareFollowDto.FollowerAdd.Select(async followerAdd =>
                    {
                        await _iOService.FollowerInfoToFileAsync(followerAdd, igScraper.Ig_id, "新增粉絲");
                    });

                    await Task.WhenAll(tasks);
                }

                igScraper.Followers = newFollowers;
            }
            else
            {
                Console.WriteLine($"錯誤: {fetchFollowerMsg}");
            }
            #endregion

            #region 比對追蹤中
            
            if (fetchFollowingResult)
            {
                var deleteList = igScraper.Followings.Where(origin => !newFollowings.Any(next => next.Id == origin.Id)); // 查找origin中有而next中沒有的User
                compareFollowDto.FollowingDelete = new ConcurrentBag<User>(deleteList);
                if (compareFollowDto.FollowingDelete.Count() > 0)
                {
                    var tasks = compareFollowDto.FollowingDelete.Select(async followingDelete =>
                    {
                        await _iOService.FollowingInfoToFileAsync(followingDelete, igScraper.Ig_id, "刪除追蹤");
                    });

                    await Task.WhenAll(tasks);
                }

                var addList = newFollowings.Where(next => !igScraper.Followings.Any(origin => next.Id == origin.Id)); // 查找next中有而origin中沒有的User
                compareFollowDto.FollowingAdd = new ConcurrentBag<User>(addList);
                if (compareFollowDto.FollowingAdd.Count() > 0)
                {
                    var tasks = compareFollowDto.FollowingAdd.Select(async followingAdd =>
                    {
                        await _iOService.FollowingInfoToFileAsync(followingAdd, igScraper.Ig_id, "新增追蹤");
                    });

                    await Task.WhenAll(tasks);
                }

                igScraper.Followings = newFollowings;
            }
            else
            {
                Console.WriteLine($"錯誤: {fetchFollowingMsg}");
            }
            #endregion

            return (true, compareFollowDto);           
        }

        public async Task<(bool, string, List<User>)> FetchDataAsync(int queryType, CookieContainer cookieContainer)
        {
            var users = new List<User>();
            _nextPage = "";
            int status;
            do
            {
                string variables = "{\"id\":\"" + "\",\"include_reel\":true,\"fetch_mutual\":true,\"first\":50";
                variables += _nextPage == "" ? "}" : ",\"after\":" + "\"" + _nextPage + "\"}";
                string encoded = Uri.EscapeDataString(variables);
                string queryHash = queryType == 0 ? "37479f2b8209594dde7facb0d904896a" : "58712303d941c6855d4e888c5f0cd22f";


                string url = $"https://www.instagram.com/graphql/query/?query_hash={queryHash}&variables={encoded}";
                (bool result, string jsonResp) = await _httpRequestService.GetAsync(url, cookieContainer);
                if (!result)
                {
                    return (false, "Http讀取失敗", null);
                }


                status = Analyze(queryType, jsonResp, users);
                if (status == -1)
                {
                    return (false, "Failed To Parse User Data", null);
                }
            } while (status != 0);

            return (true, "成功", users);
        }

        private int Analyze(int queryType, string json, List<User> users)
        {
            try
            {
                var token = queryType == 0 ? "edge_followed_by" : "edge_follow";
                var jsonObj = JObject.Parse(json);

                var totalFollowers = jsonObj["data"]["user"][token]["count"].Value<int>();
                var hasMoreInfo = jsonObj["data"]["user"][token]["page_info"]["has_next_page"].Value<bool>();
                var nextPage = jsonObj["data"]["user"][token]["page_info"]["end_cursor"].Value<string>();

                _nextPage = nextPage;

                var followersToken = jsonObj["data"]["user"][token]["edges"].ToString();
                var nodes = JArray.Parse(followersToken.ToString()).ToList();

                foreach (var node in nodes)
                {
                    users.Add(new User
                    {
                        Id = node["node"]["id"].Value<string>(),
                        Username = node["node"]["username"].Value<string>(),
                        FullName = node["node"]["full_name"].Value<string>(),
                        ProfilePicUrl = node["node"]["profile_pic_url"].Value<string>()
                    });
                }

                return hasMoreInfo ? 1 : 0;
            }
            catch { return -1; }
        }
    }
}
