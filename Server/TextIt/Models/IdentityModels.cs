using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TextIt.Controllers;
using TextIt.Games;
using TextIt.Hubs;

namespace TextIt.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    /// <summary>
    /// Application User For The Web Server
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Facebook Id to be used with Graph API requests
        /// </summary>
        public string FacebookId { get; set; }
        /// <summary>
        /// Facebook Access Token to be used with Graph API requests
        /// </summary>
        public string FacebookAccessToken { get; set; }
        /// <summary>
        /// The Full Name Of The User
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The Gender Of the User (Facebook Collected)
        /// </summary>
        public Gender Gender { get; set; }
        /// <summary>
        /// If the user is verfiyed on facebook or not
        /// </summary>
        public bool Verified { get; set; }
        /// <summary>
        /// The Users facebook cover picture (null if none)
        /// </summary>
        public string CoverPicture { get; set; }
        /// <summary>
        /// The Users Facebook profile picture
        /// </summary>
        public string ProfilePicture { get; set; }

        /// <summary>
        /// The Games that this user is accociated to
        /// </summary>
        [JsonIgnore]
        public virtual List<Game> Games { get; set; }

        /// <summary>
        /// Generate ClaimsIdentity For User
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    /// <summary>
    /// The Facebook Gender Model
    /// </summary>
    public enum Gender
    {
#pragma warning disable 1591
        Male,
        Female,
        Other
#pragma warning restore 1591
    }

    /// <summary>
    /// Main Database Context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// All Games In The Database
        /// </summary>
        public DbSet<Game> Games { get; set; }
        /// <summary>
        /// All GameInvites In the Database
        /// </summary>
        public DbSet<GameInvite> GameInvites { get; set; }
        /// <summary>
        /// All Game Applications in the Data base
        /// </summary>
        public DbSet<GameApplication> GameApplications { get; set; }
        /// <summary>
        /// Construct an <see cref="ApplicationDbContext"/>
        /// </summary>
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        /// <summary>
        /// Construct an <see cref="ApplicationDbContext"/>
        /// </summary>
        /// <returns></returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        /// <summary>
        /// Create Linkings within EF Code First
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasMany(x => x.Players)
                .WithMany(x => x.Games)
                .Map(map =>
                {
                    map.MapLeftKey("GameRefId");
                    map.MapRightKey("UserRefId");
                    map.ToTable("UserGame");
                });
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.Games)
                .WithMany(x => x.Players)
                .Map(map =>
                {
                    map.MapRightKey("GameRefId");
                    map.MapLeftKey("UserRefId");
                    map.ToTable("UserGame");
                });
            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<TextIt.Models.GameApplicationFlow> GameApplicationFlows { get; set; }
    }

    /// <summary>
    /// A Game Application (Ie. GameType)
    /// 
    /// A Game Application contains infomation about a game.
    /// </summary>
    public class GameApplication
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GameApplication()
        {
            Categories = new List<GameCategory>();
        }
        /// <summary>
        /// The Game Application ID.
        /// Used for any request to the <see cref="GameController"/>
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The Games Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The Min amount of players for this game
        /// </summary>
        public int MinPlayers { get; set; }
        /// <summary>
        /// The Max amount of players for this game
        /// </summary>
        public int MaxPlayers { get; set; }
        /// <summary>
        /// The Type.AssemblyQualifiedName of the GameState that relates to this game.
        /// </summary>
        public string GameStateType { get; set; }

        /// <summary>
        /// The order in which to sort this game item apart from all others
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The <see cref="GameCategory"/>'s for this game
        /// </summary>
        public List<GameCategory> Categories { get; set; }

        /// <summary>
        /// The Database Backing for <see cref="Categories"/>.
        /// </summary>
        [JsonIgnore]
        public string CategoriesList
        {
            get { return string.Join(",", Categories); }
            set { Categories = value.Split(',').Select(x=> Enum.Parse(typeof(GameCategory), x)).Cast<GameCategory>().ToList(); }
        }
        /// <summary>
        /// A Category identifying what a <see cref="GameApplication"/> is. Used mainly for filtering  purposes.
        /// </summary>
        public enum GameCategory
        {
#pragma warning disable 1591
            Featured, 
            Card,
            Clasic,
            TwoPlayer,
            ManyPlayer,
            Board,
            Word,
            Drawing,
            Computer,
#pragma warning restore 1591
        }
        /// <summary>
        /// The <see cref="GameApplicationFlow"/> associated to this <see cref="GameApplication"/>
        /// </summary>
        public GameApplicationFlow Flow { get; set; }

    }

    /// <summary>
    /// A Flow for the <see cref="GameApplication"/>.
    /// </summary>
    public class GameApplicationFlow
    {
        /// <summary>
        /// Game Flow id
        /// </summary>
        [Key]
        public string Id { get; set; }
        
        /// <summary>
        /// The Flow Row Span
        /// </summary>
        public int RowSpan { get; set; }
        /// <summary>
        /// The Flow Col Span
        /// </summary>
        public int ColSpan { get; set; }
        /// <summary>
        /// The Css Style for the Flow Object
        /// </summary>
        public Dictionary<string, string> Style { get; set; }
        /// <summary>
        /// The Database backing for <see cref="Style"/>
        /// </summary>
        [JsonIgnore]
        public string StyleString
        {
            get { return JsonConvert.SerializeObject(Style); }
            set { Style = JsonConvert.DeserializeObject<Dictionary<string, string>>(value); }
        }
        /// <summary>
        /// The path of the Icon on the client
        /// </summary>
        public string IconPath { get; set; }

    }


    /// <summary>
    /// A Game that is created for a <see cref="ApplicationUser"/>, can have many players, depending on the <see cref="GameApplication"/> associated. 
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The Game Id (<see cref="Guid"/>)
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// The <see cref="GameApplication"/> for this game
        /// </summary>
        public GameApplication GameApplication { get; set; }
        /// <summary>
        /// The Date this game was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// The <see cref="ApplicationUser"/> that created this game
        /// </summary>
        public ApplicationUser Owner { get; set; }
        /// <summary>
        /// The <see cref="List{ApplicationUser}"/> that are playing this game.
        /// Will contain the <see cref="Owner"/> as the first player.
        /// </summary>
        public virtual List<ApplicationUser> Players { get; set; }

        /// <summary>
        /// The Compressed Game State Database Backing Model
        /// </summary>
        public string GameState { get; set; }

        /// <summary>
        /// Get the GameState Associated to this Game.
        /// </summary>
        /// <param name="load">
        /// Wether or not it should load the <see cref="GameState"/> or if it should just construct it.
        /// If the game is already loaded this parameter is ignored.
        /// </param>
        /// <returns>
        /// If the game state is currently running in the <see cref="LobbyHub"/> it will collect it from there,
        /// otherwise it will collect it from the database.
        /// </returns>

        public GameState GetGameState(bool load = true)
        {
            if (LobbyHub.GameStates.ContainsKey(Id))
            {
                return LobbyHub.GameStates[Id];
            }
            if(GameApplication == null)
                throw new InvalidOperationException("Game Application can not be null!");
            var gameStateType = Type.GetType(GameApplication.GameStateType);
            if(gameStateType == null || !gameStateType.IsSubclassOf(typeof(GameState)))
                throw new TypeLoadException("Can not find game state type!");
            var instance = (GameState) Activator.CreateInstance(gameStateType, this);

            if(load)
                instance.LoadFromCompressedSave(GameState);
            return instance;
        }

    }
    /// <summary>
    /// A Game invite between two <see cref="ApplicationUser"/>'s
    /// 
    /// </summary>
    public class GameInvite
    {
        /// <summary>
        /// The Invite ID (<see cref="Guid"/>)
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// The Created <see cref="Game"/> associated to this Invite
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// The <see cref="ApplicationUser"/> who sent the invite.
        /// 
        /// Is also the <see cref="Game"/>.Owner
        /// </summary>
        public ApplicationUser Inviter { get; set; }

        /// <summary>
        /// The <see cref="ApplicationUser"/> who recived the invite.
        /// </summary>
        public ApplicationUser Invitee { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> this invite was created
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> this invite will expire
        /// </summary>
        public DateTime Expiry { get; set; }
    }
}