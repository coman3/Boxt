using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace TextIt.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public bool Verified { get; set; }
        public string CoverPicture { get; set; }
        public string ProfilePicture { get; set; }
        [JsonIgnore]
        public virtual List<Game> Games { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Game> Games { get; set; }
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

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
    }

    public class Game
    {
        [Key]
        public string Id { get; set; }
        public GameType GameType { get; set; }
        public DateTime CreatedDate { get; set; }

        //[JsonIgnore]
        public ApplicationUser Owner { get; set; }
        //[JsonIgnore]
        /// <summary>
        /// List Of User Id's
        /// </summary>
        public virtual List<ApplicationUser> Players { get; set; }

        [MaxLength(int.MaxValue)]
        public string GameState { get; set; }
        
    }

    public enum GameType
    {
        TicTacToe
    }
}