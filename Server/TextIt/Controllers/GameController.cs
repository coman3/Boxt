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
        public async Task<HttpResponseMessage> GetGameState([FromUri] string gameId)
        {
            if (gameId == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Game id can not be null!");
            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");
            if (game.Players.All(x => x.Id != User.Identity.GetUserId()))
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You a player of this game!");

            return Request.CreateResponse(HttpStatusCode.OK, GetGameState(game));
        }

        [HttpGet]
        [Route("Join")]
        public async Task<HttpResponseMessage> JoinGame([FromUri] string gameId)
        {
            if (gameId == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Game id can not be null!");

            var userId = User.Identity.GetUserId();
            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);

            if (game?.Owner == null )
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");

            if (game.Owner.Id == userId)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Can not join game that you own! (Already Joined)");

            if (game.Players.Any(x => x.Id == userId))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Can not join game that you have already joined!");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            game.Players.Add(user);

            var gameState = GetGameState(game);
            gameState.AddPlayer(user);
            if (!gameState.Running)
            {
                game.GameState = gameState.SaveCompressed();
            }
            await _dbContext.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, "Joined Game!");
        }

        [HttpGet]
        [Route("Invite")]
        public async Task<HttpResponseMessage> InviteToGame([FromUri] string gameId, [FromUri] string inviteUserId)
        {
            if (gameId == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Game id can not be null!");
            if (inviteUserId == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invite user can not be null!");

            var currentUserId = User.Identity.GetUserId();
            var inviteUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == inviteUserId);

            if (inviteUser == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invite user not found!");

            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);

            if (game?.Owner == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");
            if (game.Owner.Id != currentUserId)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You do not own this game!");

            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);



            return Request.CreateResponse(HttpStatusCode.OK, new GameInvite());
        }

        [HttpGet]
        [Route("List")]
        public async Task<HttpResponseMessage> ListGames([FromUri] GameType? gameType = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
            if (user.Games == null)
                return Request.CreateResponse(HttpStatusCode.NoContent, "No Games Found");

            var games = gameType == null ? user.Games.ToList() : user.Games.Where(x => x.GameType == gameType.Value).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, games);


        }

        [HttpGet]
        [Route("ListInProgress")]
        public async Task<HttpResponseMessage> ListInProgressGames([FromUri] GameType? gameType = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
            if (user.Games == null)
                return Request.CreateResponse(HttpStatusCode.NoContent, "No Games Found");
            var games = gameType == null
                ? user.Games.Where(x => LobbyHub.GameStates.ContainsKey(x.Id)).ToList()
                : user.Games.Where(x => x.GameType == gameType.Value && LobbyHub.GameStates.ContainsKey(x.Id)).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, games);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<HttpResponseMessage> CreateGame([FromUri] GameType gameType)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
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
            return Request.CreateResponse(HttpStatusCode.Created, game);
        }

        //Privates
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
    }
}
