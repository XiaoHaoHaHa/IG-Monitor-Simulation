using CoreLib.DataModels;

namespace CoreLib.Interfaces
{
    public interface ILoginService
    {
        Task LoadAllScrapersToTaskAsync(string directoryName);
        Task<(bool, string, IgScraper)> InitializeDataAsync(string sessionId);
    }
}
