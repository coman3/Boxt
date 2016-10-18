using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using TextIt.Helpers;
using TextIt.Hubs;
using TextIt.Models;

namespace TextIt.Controllers
{
    /// <summary>
    /// Invite Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Invite")]
    public class InviteController : ApiController
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();
        /// <summary>
        /// Invite a <see cref="ApplicationUser"/> to a <see cref="Game"/>
        /// </summary>
        /// <param name="gameId">The Id of the <see cref="Game"/></param>
        /// <param name="inviteUserId">The Id or the <see cref="ApplicationUser"/></param>
        /// <returns></returns>
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
                .Include(b => b.GameApplication)
                .Include(b => b.Players)
                .FirstOrDefaultAsync(x => x.Id == gameId);

            if (game?.Owner == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");
            if (game.Owner.Id != currentUserId)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You do not own this game!");
            if (game.Players.Any(x => x.Id == inviteUserId))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Can not invite user to game when they are already a player!");

            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);

            var invite = new GameInvite
            {
                CreateDate = DateTime.Now,
                Expiry = DateTime.Now.AddDays(14),
                Game = game,
                Id = Guid.NewGuid().ToString(),
                Invitee = inviteUser,
                Inviter = currentUser
            };
            _dbContext.GameInvites.Add(invite);
            await _dbContext.SaveChangesAsync();

            LobbyHub.NotifyUsers(new UserNotify
            {
                Event = new UserNotifyEvent
                {
                    Event = new
                    {
                        CreateInvite = invite
                    }
                }
            }, currentUserId, inviteUserId);

            return Request.CreateResponse(HttpStatusCode.OK, invite);
        }

        /// <summary>
        /// List Invites received from other users
        /// </summary>
        /// <returns>
        /// A <see cref="List{GameInvite}"/>
        /// </returns>

        [HttpGet]
        [Route("List")]
        public async Task<HttpResponseMessage> ListInvites()
        {
            var currentUserId = User.Identity.GetUserId();

            if (currentUserId == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found!");

            var invites =
                await
                    _dbContext.GameInvites.Include(x => x.Game)
                        .Include(x => x.Inviter)
                        .Include(x => x.Game.GameApplication)
                        .Where(x => x.Invitee.Id == currentUserId)
                        .ToListAsync();

            return invites == null
                ? Request.CreateResponse(HttpStatusCode.NoContent, "Invites not found!")
                : Request.CreateResponse(HttpStatusCode.OK, invites);
        }

        /// <summary>
        /// Accept an <see cref="GameInvite"/>
        /// </summary>
        /// <param name="inviteId">The Id of the <see cref="GameInvite"/></param>
        /// <returns>
        /// A <see cref="GameInvite"/> if accepted, else HTTP Error
        /// </returns>
        [HttpGet]
        [Route("Accept")]
        public async Task<HttpResponseMessage> AcceptInvite([FromUri] string inviteId)
        {
            var invite = await _dbContext.GameInvites
                .Include(x => x.Inviter)
                .Include(x => x.Invitee)
                .Include(x => x.Game)
                .Include(x => x.Game.GameApplication)
                .Include(x => x.Game.Players)
                .Include(x => x.Game.Owner)
                .FirstOrDefaultAsync(x => x.Id == inviteId);
            if (invite == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invite not found!");


            var userId = User.Identity.GetUserId();

            if (invite.Invitee.Id != userId)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "You do not have access to this invite.");

            var game = invite.Game;

            if (game?.Owner == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found!");

            if (game.Owner.Id == userId)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Can not join game that you own! (Already Joined)");

            if (game.Players.Any(x => x.Id == userId))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Can not join game that you have already joined!");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            game.Players.Add(user);

            var gameState = game.GetGameState();
            gameState.AddPlayer(user);
            if (!gameState.Running)
            {
                game.GameState = gameState.SaveCompressed();
            }

            LobbyHub.NotifyUsers(new UserNotify
            {
                Event = new UserNotifyEvent
                {
                    Event = new
                    {
                        AcceptInvite = invite
                    }
                }
            }, invite.Invitee.Id, invite.Inviter.Id);

            _dbContext.GameInvites.Remove(invite);
            await _dbContext.SaveChangesAsync();

            return Request.CreateResponse(HttpStatusCode.OK, game);
        }
    }
}
