namespace IgScraperApi.WebSocketServices
{
    public static class ConnectorsManager
    {
        /// <summary>
        /// Key, Value: ConnectionId, UserId
        /// </summary>
        public static Dictionary<string, string> Connectors { get; set; } = new Dictionary<string, string>();
    }
}
