using CoreLib.DataModels;
using CoreLib.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace CoreLib
{
    public class IOService : IIOService
    {
        public string ExtractJsonStrings(string directoryName, string username, string followType)
        {
            var filePath =  Path.Combine(Environment.CurrentDirectory, directoryName, $"{username}", $"{username}_{followType}.txt");
            var jsonObj = new List<object>();
            string currentJson = "";

            foreach (string line in File.ReadLines(filePath))
            {
                if (line.StartsWith("Task Start"))
                {
                    // 如果當前JSON字串不為空，表示已經讀取完整一個JSON部分
                    if (!string.IsNullOrEmpty(currentJson))
                    {
                        if(currentJson != "[")
                        {
                            var tempList = JsonConvert.DeserializeObject<List<object>>($"{currentJson}]");
                            jsonObj.AddRange(tempList);
                        }
                            
                        currentJson = "";
                    }
                }
                else
                {
                    // 將每行非"Task Start"的部分加到當前JSON字串中
                    currentJson += line;
                }
            }

            // 確保最後一個JSON字串也被加入
            if (!string.IsNullOrEmpty(currentJson))
            {
                if (currentJson != "[")
                {
                    var tempList = JsonConvert.DeserializeObject<List<object>>($"{currentJson}]");
                    jsonObj.AddRange(tempList);
                }
            }

            return JsonConvert.SerializeObject(jsonObj);
        }

        public (bool, string message, string[]) GetAllScrapersFiles(string directoryName)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, directoryName);

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            else
            {
                var files = Directory.GetFiles(filePath, "*_Info.txt", SearchOption.AllDirectories);

                if (files.Length > 0)
                {
                    return (true, null, files);
                }
                else
                {
                    return (false, "沒有任何登記的使用者", null);
                }
            }
            return (false, "找不到目錄",null);
        }

        public async Task<(bool, string)> ScraperInfoToFileAsync(string ig_id, string pictureUrl, CookieContainer cookieContainer)
        {
            try
            {            
                var filePath = Path.Combine(Environment.CurrentDirectory, "IgData", $"{ig_id}");
                var fileName = $"{ig_id}_Info.txt";

                Directory.CreateDirectory(filePath);

                using (var file = new FileStream(Path.Combine(filePath, fileName), FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var streamWriter = new StreamWriter(file))
                    {
                        await streamWriter.WriteLineAsync(JsonConvert.SerializeObject(new
                        {
                            DsUserId = cookieContainer.GetAllCookies()["ds_user_id"].Value,
                            Ig_id = ig_id,
                            Sessionid = cookieContainer.GetAllCookies()["sessionid"].Value,
                            PictureUrl = pictureUrl
                        }
                        , Formatting.Indented));
                    }
                }
                return (true, "輸出成功");
            }
            catch (Exception e)
            {
                return (false, $"IO處理失敗ScraperInfoToFileAsync, {e.Message}");
            }
        }

        public async Task<(bool, string)> FollowingInfoToFileAsync(User followingAccount, string ig_id, string addOrDelete, bool isFirstTime = false)
        {
            try
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, "IgData", $"{ig_id}");
                Directory.CreateDirectory(filePath);

                var followingtxt = $"{ig_id}_Following.txt";

                using (var file = new FileStream(Path.Combine(filePath, followingtxt), FileMode.Append, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(file))
                    {
                        if (isFirstTime)
                        {
                            await streamWriter.WriteLineAsync($"Task Start: {DateTime.Now}\n[\n");
                            return (true, "輸出成功");
                        }
                        await streamWriter.WriteLineAsync(JsonConvert.SerializeObject(new
                        {
                            DateTime = DateTime.Now.ToString(),
                            Status = addOrDelete,
                            User = followingAccount
                        }, Formatting.Indented
                        ) + ", ");
                    }
                }
                return (true, "輸出成功");
            }
            catch (Exception e)
            {
                return (false, $"IO處理失敗FollowingInfoToFileAsync, {e.Message}");
            }      
        }

        public async Task<(bool, string)> FollowerInfoToFileAsync(User follower, string ig_id, string addOrDelete, bool isFirstTime = false)
        {
            try
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, "IgData", $"{ig_id}");
                Directory.CreateDirectory(filePath);

                var followertxt = $"{ig_id}_Follower.txt";

                using (var file = new FileStream(Path.Combine(filePath, followertxt), FileMode.Append, FileAccess.Write))
                {
                    using (var streamWriter = new StreamWriter(file))
                    {
                        if (isFirstTime)
                        {
                            await streamWriter.WriteLineAsync($"Task Start: {DateTime.Now}\n[\n");
                            return (true, "輸出成功");
                        }
                        await streamWriter.WriteLineAsync(JsonConvert.SerializeObject(new
                        {
                            DateTime = DateTime.Now.ToString(),
                            Status = addOrDelete,
                            User = follower
                        }, Formatting.Indented
                        ) + ", ");
                    }
                }
                return (true, "輸出成功");
            }
            catch (Exception e)
            {
                return (false, $"IO處理失敗FollowerInfoToFileAsync, {e.Message}");
            }
        }
    }
}
