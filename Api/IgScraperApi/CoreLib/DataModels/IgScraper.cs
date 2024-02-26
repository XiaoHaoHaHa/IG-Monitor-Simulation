using System.Collections.Concurrent;
using System.Net;
using System.Runtime.Serialization;

namespace CoreLib.DataModels
{
    public class IgScraper
    {
        [IgnoreDataMember]
        public static ConcurrentDictionary<string, IgScraper> IgScrapers { get; set; } = new ConcurrentDictionary<string, IgScraper>();


        public string DsUserId { get; set; }
        public string Ig_id { get; set; }
        public string Sessionid { get; set; }
        public string PictureUrl { get; set; }

        public bool IsFirstAdd { get; set; } = true;

        public List<User> Followers { get; set; } = new List<User>();
        public List<User> Followings { get; set; } = new List<User>();

        [IgnoreDataMember]
        public CookieContainer cookieContainer { get; set; } = new CookieContainer();
    }
}
