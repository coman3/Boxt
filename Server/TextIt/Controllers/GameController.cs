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
using TextIt.Helpers;
using TextIt.Hubs;
using TextIt.Models;

namespace TextIt.Controllers
{
    /// <summary>
    /// A Controler to access infomation about the games state, Create Games and List Users Games.
    /// </summary>
    [System.Web.Http.Authorize]
    [RoutePrefix("api/Game")]
    public class GameController : ApiController
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        /// <summary>
        /// Get a <see cref="GameState"/> from the specified <see cref="Game"/> 
        /// </summary>
        /// <param name="gameId">The <see cref="Game"/>.Id</param>
        /// <returns>
        /// A <see cref="GameState"/> accoicated to the <see cref="Game"/> (the type is specified in the <see cref="GameState"/>)
        /// </returns>
        [HttpGet]
        [Route("State")]
        public async Task<HttpResponseMessage> GetGameState([FromUri] string gameId)
        {
            if (gameId == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Game id can not be null!");
            var game = await _dbContext.Games
                .Include(b => b.Owner)
                .Include(b => b.Players)
                .Include(b => b.GameApplication)
                .FirstOrDefaultAsync(x => x.Id == gameId);
            if (game == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");
            if (game.Players.All(x => x.Id != User.Identity.GetUserId()))
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You a player of this game!");

            return Request.CreateResponse(HttpStatusCode.OK, game.GetGameState());
        }

        /// <summary>
        /// Get All <see cref="GameApplication"/>s in the Database at the current moment in time.
        /// </summary>
        /// <param name="gameCategoryFilter">A <see cref="GameApplication.GameCategory"/> that you wish to only return</param>
        /// <returns>A <see cref="List{GameApplication}"/></returns>
        [HttpGet]
        [Route("ListType")]
        public async Task<List<GameApplication>> CollectGames(
            [FromUri] GameApplication.GameCategory? gameCategoryFilter = null)
        {
            if (gameCategoryFilter == null)
                return await _dbContext.GameApplications
                    .OrderBy(x => x.Order)
                    .Include(b => b.Flow)
                    .ToListAsync();
            return
                await
                    _dbContext.GameApplications.Where(x => x.Categories.Contains(gameCategoryFilter.Value))
                        .OrderBy(x => x.Order)
                        .Include(b => b.Flow)
                        .ToListAsync();
        }

        /// <summary>
        /// List the current <see cref="ApplicationUser"/>s Games.
        /// </summary>
        /// <param name="gameCategoryFilter">A <see cref="GameApplication.GameCategory"/> that you wish to only return</param>
        /// <returns>
        /// A <see cref="List{GameApplication}"/>
        /// </returns>
        [HttpGet]
        [Route("List")]
        public async Task<HttpResponseMessage> ListGames(
            [FromUri] GameApplication.GameCategory? gameCategoryFilter = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users
                .Include(x => x.Games.Select(c => c.GameApplication))
                .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
            if (user.Games == null)
                return Request.CreateResponse(HttpStatusCode.NoContent, "No Games Found");

            var games = gameCategoryFilter == null
                ? user.Games.ToList()
                : user.Games.Where(x => x.GameApplication.Categories.Contains(gameCategoryFilter.Value)).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, games);


        }

        /// <summary>
        /// Gets the current <see cref="Game"/>s that are running on the <see cref="LobbyHub"/> accociated to the current <see cref="ApplicationUser"/>
        /// </summary>
        /// <param name="gameCategoryFilter">A <see cref="GameApplication.GameCategory"/> that you wish to only return</param>
        /// <returns>A <see cref="List{Game}"/></returns>
        [HttpGet]
        [Route("ListInProgress")]
        public async Task<HttpResponseMessage> ListInProgressGames([FromUri] GameApplication.GameCategory? gameCategoryFilter = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
            if (user.Games == null)
                return Request.CreateResponse(HttpStatusCode.NoContent, "No Games Found");
            var games = gameCategoryFilter == null
                ? user.Games.Where(x => LobbyHub.GameStates.ContainsKey(x.Id)).ToList()
                : user.Games.Where(x => x.GameApplication.Categories.Contains(gameCategoryFilter.Value) && LobbyHub.GameStates.ContainsKey(x.Id)).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, games);
        }

        /// <summary>
        /// Create a game for the current <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="gameApplicationId">The <see cref="GameApplication"/> that you wish to create</param>
        /// <returns>
        /// An Instance of <see cref="Game"/>.
        /// </returns>
        [HttpGet]
        [Route("Create")]
        public async Task<HttpResponseMessage> CreateGame([FromUri] int gameApplicationId)
        {
            var userId = User.Identity.GetUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");
            var gameApplication = await _dbContext.GameApplications.FirstOrDefaultAsync(x => x.Id == gameApplicationId);
            if (gameApplication == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game Application not found!");

            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                GameApplication = gameApplication,
                CreatedDate = DateTime.Now,
                Owner = user,
                Players = new List<ApplicationUser>
                {
                    user,
                },
            };
            var gameState = game.GetGameState(false);
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

    }
}
