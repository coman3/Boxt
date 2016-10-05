using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace TextIt.Hubs
{
    public class LobbyHub : Hub<ILobbyHub>
    {
        public LobbyHub()
        {
            
        }

        public override Task OnConnected()
        {
            Clients.All.ServerMessage("Client '" + Context.ConnectionId + "' Connected!");
            return base.OnConnected();
        }
    }

    public interface ILobbyHub
    {
        void ServerMessage(string message);
    }
}