using CoreLib.DataModels;
using System.Collections.Concurrent;

namespace CoreLib.Dtos
{
    public class CompareFollowDto
    {
        public ConcurrentBag<User> FollowerAdd { get; set; } = new ConcurrentBag<User>();  
        public ConcurrentBag<User> FollowerDelete { get; set; } = new ConcurrentBag<User>();
        public ConcurrentBag<User> FollowingAdd { get; set; } = new ConcurrentBag<User>();
        public ConcurrentBag<User> FollowingDelete { get; set; } = new ConcurrentBag<User>();
    }
}
