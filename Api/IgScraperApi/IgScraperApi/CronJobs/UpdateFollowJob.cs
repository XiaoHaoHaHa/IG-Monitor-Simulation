using CoreLib.DataModels;
using CoreLib.Dtos;
using CoreLib.Interfaces;
using IgScraperApi.WebSocketServices;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Quartz;

namespace IgScraperApi.CronJobs
{
    public class UpdateFollowJob : IJob
    {
        private IFetchFollowSevice _fetchFollowSevice;
        private IHubContext<BroadcastHub> _hubContext;
        private IIOService _iOService;

        public UpdateFollowJob(IFetchFollowSevice fetchFollowSevice, IIOService iOService, IHubContext<BroadcastHub> hubContext)
        {
            _fetchFollowSevice = fetchFollowSevice;
            _hubContext = hubContext;
            _iOService = iOService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tasks = IgScraper.IgScrapers.Select(async scraper =>
            {
                (bool result, CompareFollowDto compareFollowDto) = await _fetchFollowSevice.ProcessAsync(scraper.Value);

                if (result)
                {
                    if (compareFollowDto != null)
                    {
                        if (compareFollowDto.FollowerDelete.Count() > 0)
                            BroadCast("Unfollowed", scraper.Value.Ig_id, $"{JsonConvert.SerializeObject(compareFollowDto.FollowerDelete)}");
                        if (compareFollowDto.FollowerAdd.Count() > 0)
                            BroadCast("Newfollowed", scraper.Value.Ig_id, $"{JsonConvert.SerializeObject(compareFollowDto.FollowerAdd)}");
                        if (compareFollowDto.FollowingDelete.Count() > 0)
                            BroadCast("Unfollowing", scraper.Value.Ig_id, $"{JsonConvert.SerializeObject(compareFollowDto.FollowingDelete)}");
                        if (compareFollowDto.FollowingAdd.Count() > 0)
                            BroadCast("Newfollowing", scraper.Value.Ig_id, $"{JsonConvert.SerializeObject(compareFollowDto.FollowingAdd)}");
                    }
                    else
                    {
                        Console.WriteLine("第一次新增成功");
                    }
                }
            });

            await Task.WhenAll(tasks);
        }

        private void BroadCast(string method, string ig_id, string msg)
        {
            var connectors = ConnectorsManager.Connectors.Where(x => x.Value == ig_id);
            foreach (var connector in connectors)
            {
                _hubContext.Clients.Client(connector.Key).SendAsync(method, $"{msg}");
            }
        }
    }
}
