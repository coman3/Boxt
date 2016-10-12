using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using TextIt.Games;
using TextIt.Hubs;
using TextIt.Models;

namespace TextIt.Controllers
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/Game")]
    public class GameController : ApiController
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        [HttpGet]
        [Route("State")]
        public async Task<GameState> GetGameState([FromUri] string gameId)
        {
            if (gameId == null) return null;
            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null) return null;
            if (game.Players.All(x => x.Id != User.Identity.GetUserId())) return null;

            return GetGameState(game);
        }

        private static GameState GetGameState(Game game)
        {
            GameState gameState = null;
            if (LobbyHub.GameStates.ContainsKey(game.Id))
            {
                return LobbyHub.GameStates[game.Id];
            }

            switch (game.GameType)
            {
                case GameType.TicTacToe:
                    gameState = new TicTacToeGameState(game);
                    break;
            }
            if (gameState == null)
                return null;
            gameState.LoadFromCompressedSave(game.GameState);
            return gameState;
        }

        [HttpGet]
        [Route("Join")]
        public async Task<bool> JoinGame([FromUri] string gameId)
        {
            if (gameId == null) return false;

            var userId = User.Identity.GetUserId();
            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);

            if (game?.Owner == null || game.Owner.Id == userId) return false;
            if (game.Players.Any(x => x.Id == userId)) return false;

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            game.Players.Add(user);

            var gameState = GetGameState(game);
            gameState.AddPlayer(user);
            if (!gameState.Running)
            {
                game.GameState = gameState.SaveCompressed();
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        [HttpGet]
        [Route("List")]
        public async Task<List<Game>> ListGames([FromUri] GameType? gameType = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user?.Games == null) return null;
            if (gameType == null) return user.Games.ToList();
            return user.Games.Where(x => x.GameType == gameType.Value).ToList();
        }

        [HttpGet]
        [Route("Create")]
        public async Task<Game> CreateGame([FromUri] GameType gameType)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return null;
            
            
            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                GameType = gameType,
                CreatedDate = DateTime.Now,
                Owner = user,
                Players = new List<ApplicationUser>
                {
                    user,
                },
            };
            var gameState = GameState.GetGameStateFromType(game);
            gameState.SetupStart();
            game.GameState = gameState.SaveCompressed();

            if (user.Games == null)
            {
                user.Games = new List<Game>();
            }
            user.Games.Add(game);
            _dbContext.Games.Add(game);
            await _dbContext.SaveChangesAsync();
            return game;
        }
    }
}
