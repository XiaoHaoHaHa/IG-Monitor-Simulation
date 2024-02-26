using CoreLib.Interfaces;
using System.Net;
using System.Text;

namespace CoreLib
{
    public class HttpRequestService : IHttpRequestService
    {
        public async Task<(bool, string)> GetUrlPictureAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {                  
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                    string base64String = Convert.ToBase64String(imageData);

                    return (true, base64String);
                }
                else
                {
                    return (false, $"Http請求失敗url:{url}");
                }            
            }
        }

        public async Task<(bool, string)> GetAsync(string url, CookieContainer cookieContainer = null)
        {
            // 建立 HttpClient 並指定 CookieContainer
            using (var handler = new HttpClientHandler())
            {
                handler.CookieContainer = cookieContainer?? new CookieContainer();

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Instagram 85.0.0.21.100 Android (23/6.0.1; 538dpi; 1440x2560; LGE; LG-E425f; vee3e; en_US)");
                    
                    // 做 GET 請求
                    var response = await client.GetAsync(url);

                    // 檢查回應是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 處理回應內容，這裡使用字串輸出
                        var responseBody = await response.Content.ReadAsStringAsync();
                        return (true, responseBody);
                    }
                    else
                    {
                        return (false, $"Http請求失敗url:{url}");
                    }
                }
            }
        }

        public async Task<(bool, string)> PostAsync(string url, string jsonContent, CookieContainer cookieContainer = null)
        {
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.CookieContainer = cookieContainer?? new CookieContainer();

                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "Instagram 85.0.0.21.100 Android (23/6.0.1; 538dpi; 1440x2560; LGE; LG-E425f; vee3e; en_US)");

                        // 設定 HTTP 方法為 POST，並傳送 JSON 內容
                        HttpResponseMessage response = await client.PostAsync(url, content);

                        // 檢查回應是否成功
                        if (response.IsSuccessStatusCode)
                        {
                            // 處理回應內容，這裡使用字串輸出
                            string responseBody = await response.Content.ReadAsStringAsync();
                            return (true, responseBody);
                        }
                        else
                        {
                            return (false, $"Http請求失敗url:{url}");
                        }
                    }
                }
            }
        }
    }
}
