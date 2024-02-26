using CoreLib.Interfaces;

namespace IgScraperApi.Middleware
{
    public class InitializationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public InitializationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 在這裡執行您的初始化代碼
            using (var scope = _serviceProvider.CreateScope())
            {
                var loginService = scope.ServiceProvider.GetRequiredService<ILoginService>();
                await loginService.LoadAllScrapersToTaskAsync("IgData");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 在這裡執行停止代碼，如果有需要的話
            return Task.CompletedTask;
        }
    }
}
