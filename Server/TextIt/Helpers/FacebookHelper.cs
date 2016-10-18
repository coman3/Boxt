using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextIt.Models;

namespace TextIt.Helpers
{
    /// <summary>
    /// Facebook Helper
    /// </summary>
    public static class FacebookHelper
    {
        private const string FacebookFriendsApiEndpoint = "https://graph.facebook.com/{0}/friends?access_token={1}";
        /// <summary>
        /// Get <see cref="ApplicationUser"/>s Friends
        /// </summary>
        /// <param name="context"><see cref="ApplicationDbContext"/></param>
        /// <param name="user"><see cref="ApplicationUser"/> that you wish to get friends from</param>
        /// <returns>
        /// A <see cref="List{ApplicationUser}"/> of friends
        /// </returns>
        public static async Task<List<ApplicationUser>> GetUserFriendsAsync(this ApplicationDbContext context, ApplicationUser user)
        {
            if (context == null || user == null)
                throw new ArgumentException("ApplicationDbContext or ApplicationUser can not be null!");
            var friendList = new List<ApplicationUser>();
            using (var client = new HttpClient())
            {
                using (
                    var request =
                        await
                            client.GetAsync(string.Format(FacebookFriendsApiEndpoint, user.FacebookId,
                                user.FacebookAccessToken)))
                {
                    var json = (dynamic) JsonConvert.DeserializeObject(await request.Content.ReadAsStringAsync());

                    if (json.data == null)
                        return null;

                    var friends = json.data;
                    foreach (var friend in friends)
                    {
                        if (friend.id == null)
                            continue;
                        var friendId = (string) friend.id;
                        var userFriend = context.Users.FirstOrDefault(x => x.FacebookId == friendId);
                        if(userFriend == null)
                            continue;
                        
                        friendList.Add(userFriend);
                    }
                    return friendList;
                }
            }
        }
    }
}