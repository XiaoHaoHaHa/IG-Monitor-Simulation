using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IgScraperApi.WebSocketServices
{
    [Authorize]
    public sealed class BroadcastHub : Hub
    {
        private JwtHelper _jwt;

        public BroadcastHub(JwtHelper jwt)
        {
            _jwt = jwt;
        }

        public override async Task OnConnectedAsync()
        {
            var ig_id = Context.User.FindFirst("Id").Value;
            ConnectorsManager.Connectors.Add(Context.ConnectionId, ig_id);
            await Clients.All.SendAsync("Init", $"{ig_id}已建立HUB連線 ConnectionId: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectorsManager.Connectors.Remove(Context.ConnectionId);
            var connectionId = Context.ConnectionId;
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}