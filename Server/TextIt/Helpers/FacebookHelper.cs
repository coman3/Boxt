using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextIt.Models;

namespace TextIt.Helpers
{
    public static class FacebookHelper
    {
        private const string FacebookFriendsApiEndpoint = "https://graph.facebook.com/{0}/friends";
        public static async Task<List<ApplicationUser>> GetUserFriendsAsync(this ApplicationDbContext context, ApplicationUser user)
        {
            if (context == null || user == null)
                throw new ArgumentException("ApplicationDbContext or ApplicationUser can not be null!");
            using (var client = new HttpClient())
            {
                using (var request = await client.GetAsync(string.Format(FacebookFriendsApiEndpoint, user.FacebookId)))
                {
                    var json = (JObject) JsonConvert.DeserializeObject(await request.Content.ReadAsStringAsync());

                }
            }


            return null;
        }
    }
}