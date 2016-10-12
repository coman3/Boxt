using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using TextIt.Games;
using TextIt.Models;
using WebGrease.Configuration;

namespace TextIt.Hubs
{
    public class LobbyHub : Hub<ILobbyHub>
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();
        public static Dictionary<string, string> UserMatches = new Dictionary<string, string>();
        public static Dictionary<string, GameState> GameStates = new Dictionary<string, GameState>();
        public static Dictionary<string, List<string>> UserGames = new Dictionary<string, List<string>>();
        public LobbyHub()
        {
            
        }

        public void UpdateGame(string gameId, dynamic state)
        {
            if (GameStates.ContainsKey(gameId) && UserGames.ContainsKey(GetUserId()) &&
                UserGames[GetUserId()].Contains(gameId))
            {
                GameStates[gameId].UpdateState(state, Context);
            }
            
        }

        private string GetUserId()
        {
            return UserMatches[Context.ConnectionId];
        }
        public void JoinGame(string gameId)
        {
            if (IsNotLoggedIn())
                return;
            var game = _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefault(x => x.Id == gameId);
            if (game == null || game.Players.All(x => x.Id != UserMatches[Context.ConnectionId]))
                return;
            if (!GameStates.ContainsKey(game.Id))
            {
                GameStates[game.Id] = GameState.GetGameStateFromType(game);
                GameStates[game.Id].LoadFromCompressedSave();
                GameStates[game.Id].Running = true;
                GameStates[game.Id].OnGameStateUpdate += LobbyHub_OnGameStateUpdate;
                GameStates[game.Id].OnGameEnd += LobbyHub_OnGameEnd;
            }
            if (!UserGames.ContainsKey(GetUserId()))
            {
                UserGames[GetUserId()] = new List<string>();
            }

            if (!UserGames[GetUserId()].Contains(game.Id)) 
                UserGames[GetUserId()].Add(game.Id);

        }

        private static void LobbyHub_OnGameEnd(GameState sender, OnGameEndArgs args)
        {
            var clientProxy = GlobalHost.ConnectionManager.GetHubContext<LobbyHub>();
            var relatedClients = UserGames.Where(x => x.Value.Contains(sender.Game.Id)).Select(x => UserMatches.First(c => c.Value == x.Key).Key).ToList();
            clientProxy.Clients.Clients(relatedClients).Update(new
            {
                GameEnd = new
                {
                    args.Reason,
                    sender.Game.Id,
                }
            });
        }

        private static void LobbyHub_OnGameStateUpdate(GameState sender, OnGameStateUpdateArgs args)
        {
            var clientProxy = GlobalHost.ConnectionManager.GetHubContext<LobbyHub>();
            var relatedClients = UserGames.Where(x => x.Value.Contains(sender.Game.Id)).Select(x => UserMatches.First(c => c.Value == x.Key).Key).ToList();
            clientProxy.Clients.Clients(relatedClients).Update(args.State);
        }

        public void Login(string token)
        {
            if(!IsNotLoggedIn()) return;

            AuthenticationTicket ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(token);
            if (ticket?.Properties?.ExpiresUtc != null)
            {
                ClaimsIdentity identity = ticket.Identity;
                var userId = identity.GetUserId();
                UserMatches[Context.ConnectionId] = userId;
            }
        }
        public override Task OnConnected()
        {
            UserMatches[Context.ConnectionId] = null;
            Clients.Caller.Update("Connected!");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserMatches.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
        private bool IsNotLoggedIn()
        {
            return UserMatches[Context.ConnectionId] == null;
        }
    }

    public interface ILobbyHub
    {
        void ServerMessage(dynamic message);
        void Update(dynamic state);
    }
}