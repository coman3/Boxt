using System;
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
#pragma warning disable 1591

namespace TextIt.Hubs
{
    public class LobbyHub : Hub<ILobbyHub>
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();
        public static Dictionary<string, string> UserMatches = new Dictionary<string, string>();
        public static Dictionary<string, GameState> GameStates = new Dictionary<string, GameState>();
        public static Dictionary<string, List<string>> UserGames = new Dictionary<string, List<string>>();

        public static void NotifyUsers(UserNotify data, params string[] userId)
        {
            var clientProxy = GlobalHost.ConnectionManager.GetHubContext<LobbyHub>();
            var relatedClients = UserMatches.Where(x => userId.Contains(x.Value)).Select(x=> x.Key).ToList();
            if (relatedClients.Count <= 0)
                return;

            clientProxy.Clients.Clients(relatedClients).Update(data);
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

        public void LeaveGame(string gameId)
        {
            if (IsNotLoggedIn())
                return;
            var game = _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .Include(b => b.GameApplication)
                .FirstOrDefault(x => x.Id == gameId);
            if (game == null || game.Players.All(x => x.Id != UserMatches[Context.ConnectionId]))
                return;
            if (GameStates.ContainsKey(game.Id) && UserGames.ContainsKey(GetUserId()) && UserGames[GetUserId()].Contains(game.Id))
            {
                UserGames[GetUserId()].Remove(gameId);
            }
        }

        public void JoinGame(string gameId)
        {
            if (IsNotLoggedIn())
                return;
            var game = _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .Include(b => b.GameApplication)
                .FirstOrDefault(x => x.Id == gameId);
            if (game == null || game.Players.All(x => x.Id != UserMatches[Context.ConnectionId]))
                return;
            if (!GameStates.ContainsKey(game.Id))
            {
                GameStates[game.Id] = game.GetGameState();
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
                    Data = args,
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
            try
            {
                AuthenticationTicket ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(token);
                if (ticket?.Properties?.ExpiresUtc != null && ticket.Identity.IsAuthenticated)
                {
                    ClaimsIdentity identity = ticket.Identity;
                    var userId = identity.GetUserId();
                    foreach (var userMatch in UserMatches)
                    {
                        if (userMatch.Value != userId) continue;

                        UserMatches[userMatch.Key] = "*LOG_OUT:OTHER_USER";
                        break;
                    }
                    UserMatches[Context.ConnectionId] = userId;
                    Clients.Caller.Update(new
                    {
                        Event = new
                        {
                            Connected = true,
                        }
                    });
                    return;
                }
                Clients.Caller.Update(new
                {
                    Event = new
                    {
                        Connected = false,
                        Error = "Login Failed. Please check the access token.",
                    }
                });
            }
            catch (Exception ex)
            {
                Clients.Caller.Update(new
                {
                    Event = new
                    {
                        Connected = false,
                        Error = ex.Message,
                    }
                });
            }
        }

        public override Task OnConnected()
        {
            UserMatches[Context.ConnectionId] = null;
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserMatches.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }
        private bool IsNotLoggedIn()
        {
            if (UserMatches.ContainsKey(Context.ConnectionId) && UserMatches[Context.ConnectionId] != null && !UserMatches[Context.ConnectionId].StartsWith("*"))
                return false;

            Clients.Caller.Update(new
            {
                Error = new
                {
                    Connected = false,
                    Error = "Login Check Failed. Please Login."
                }
            });
            return true;
        }
    }

    public class UserNotify
    {
        public UserNotifyToast Toast { get; set; }
        public UserNotifyEvent Event { get; set; }
    }

    public class UserNotifyEvent
    {
        public dynamic Event { get; set; }
    }

    public class UserNotifyToast
    {
        public UserNotifyToast(string message)
        {
            Message = message;
        }

        public string Message { get; set; }

    }

    public interface ILobbyHub
    {
        void ServerMessage(dynamic message);
        void Update(dynamic state);
        void Notify(dynamic state); //TODO;
    }
}