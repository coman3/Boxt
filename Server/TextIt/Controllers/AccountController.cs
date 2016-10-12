using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using TextIt.Helpers;
using TextIt.Models;
using TextIt.Providers;
using TextIt.Results;

namespace TextIt.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        [HttpGet]
        [AllowAnonymous]
        [Route("Check")]
        public HttpResponseMessage CheckToken()
        {
            return User.Identity.IsAuthenticated
                ? Request.CreateResponse(HttpStatusCode.Accepted, "Token Vaild!")
                : Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Token Invaild!");
        }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public async Task<UserInfoViewModel> GetUserInfo()
        {
            ExternalLoginData externalLogin = await ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity, UserManager);

            if (externalLogin == null) return null;
            return new UserInfoViewModel
            {
                CoverPicture = externalLogin.CoverPicture,
                Email = externalLogin.Email,
                Gender = externalLogin.Gender.ToString(),
                Name = externalLogin.Name,
                ProfilePicture = externalLogin.ProfilePicture
            };
        }
        [Route("Friends")]
        public async Task<List<ApplicationUser>> GetUserFriends()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var facebookFriends = await _dbContext.GetUserFriendsAsync(user);
            return facebookFriends;
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RegisterExternalToken
        [OverrideAuthentication]
        [AllowAnonymous]
        [Route("RegisterExternalToken")]
        public async Task<IHttpActionResult> RegisterExternalToken(RegisterExternalTokenBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //validate token
            ExternalLoginData externalLogin = await ExternalLoginData.FromToken(model.Provider, model.Token);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != model.Provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return InternalServerError();
            }
            //if we reached this point then token is valid
            ApplicationUser user = await UserManager.FindByEmailAsync(externalLogin.Email);

            bool hasRegistered = user != null;
            IdentityResult result;

            if (!hasRegistered)
            {
                user = new ApplicationUser
                {
                    UserName = externalLogin.ProviderKey,
                    Email = externalLogin.Email,
                    ProfilePicture = externalLogin.ProfilePicture,
                    CoverPicture = externalLogin.CoverPicture,
                    Gender = externalLogin.Gender,
                    Verified = externalLogin.Verified,
                    Name = externalLogin.Name,
                    FacebookId = externalLogin.ProviderKey
                };
                result = await UserManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }

            //authenticate
            var identity = await UserManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            IEnumerable<Claim> claims = externalLogin.GetClaims();
            identity.AddClaims(claims);
            Authentication.SignIn(identity);

            ClaimsIdentity oAuthIdentity = new ClaimsIdentity(Startup.OAuthOptions.AuthenticationType);

            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, new AuthenticationProperties());

            DateTime currentUtc = DateTime.UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(Startup.OAuthOptions.AccessTokenExpireTimeSpan);

            string accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);



            // Create the response building a JSON object that mimics exactly the one issued by the default /Token endpoint
            JObject token = new JObject(
                new JProperty("userName", user.UserName),
                new JProperty("userId", user.Id),
                new JProperty("access_token", accessToken),
                new JProperty("token_type", "bearer"),
                new JProperty("expires_in", Startup.OAuthOptions.AccessTokenExpireTimeSpan.TotalSeconds.ToString()),
                new JProperty("issued", currentUtc.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'")),
                new JProperty("expires",
                    currentUtc.Add(Startup.OAuthOptions.AccessTokenExpireTimeSpan)
                        .ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"))
            );

            return Ok(token);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }

            public string Name { get; set; }
            public Gender Gender { get; set; }
            public bool Verified { get; set; }
            public string CoverPicture { get; set; }
            public string ProfilePicture { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static async Task<ExternalLoginData> FromIdentity(ClaimsIdentity identity, UserManager<ApplicationUser> manager)
            {
                Claim providerKeyClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(providerKeyClaim?.Issuer) || string.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                var user = await manager.FindByIdAsync(identity.GetUserId());
                if (user == null)
                    return null;
                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = user.UserName,
                    CoverPicture = user.CoverPicture,
                    ProfilePicture = user.ProfilePicture,
                    Email = user.Email,
                    Gender = user.Gender,
                    Name = user.Name,
                    Verified = user.Verified
                };
            }

            private static string GetUserProfileEndPoint(string provider, string accessToken)
            {
                switch (provider)
                {
                    case "Facebook":
                        return $"https://graph.facebook.com/me?fields=email,name,age_range,gender,verified,cover&access_token={accessToken}";
                }
                return null;
            }
            private static string GetUserProfilePictureEndPoint(string provider, string accessToken)
            {
                switch (provider)
                {
                    case "Facebook":
                        return $"https://graph.facebook.com/me/picture?redirect=false&type=large&access_token={accessToken}";
                }
                return null;
            }
            private static string GetAppProfileEndPoint(string provider, string accessToken)
            {
                switch (provider)
                {
                    case "Facebook":
                        return $"https://graph.facebook.com/app?fields=id&access_token={accessToken}";
                }
                return null;
            }

            private static ExternalLoginData FromJObject(dynamic iObj)
            {
                if (iObj["id"] == null) return null;
                if (iObj["email"] == null) return null;
                if (iObj["name"] == null) return null;
                var loginData = new ExternalLoginData
                {
                    Email = iObj["email"],
                    Name = iObj["name"],
                    ProviderKey = iObj["id"],
                    Verified = iObj["verified"]
                };
                if (iObj["cover"] != null)
                {
                    loginData.CoverPicture = iObj["cover"]["source"] ?? "";
                }
                loginData.Gender = iObj["gender"];
                return loginData;
            }

            public static async Task<ExternalLoginData> FromToken(string provider, string accessToken)
            {
                ExternalLoginData loginData = null;
                using (var client = new HttpClient())
                {
                    var verifyTokenEndPoint = GetUserProfileEndPoint(provider, accessToken);
                    var verifyTokenPicturendPoint = GetUserProfilePictureEndPoint(provider, accessToken);
                    var verifyAppEndpoint = GetAppProfileEndPoint(provider, accessToken);

                    if (verifyTokenEndPoint == null || verifyAppEndpoint == null)
                        return null;

                    Uri uri = new Uri(verifyTokenEndPoint);
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        ClaimsIdentity identity = null;

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            dynamic iObj = (JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                            loginData = FromJObject(iObj);
                            if (loginData == null)
                                return null;
                            using (var profilePictureResponse = await client.GetAsync(verifyTokenPicturendPoint))
                            {
                                if (profilePictureResponse.IsSuccessStatusCode)
                                {
                                    var appContent = await profilePictureResponse.Content.ReadAsStringAsync();
                                    dynamic profilePictureContent = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(appContent);
                                    loginData.ProfilePicture = profilePictureContent["data"]["url"] ?? "";
                                }
                            }

                            identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

                            if (provider == "Facebook")
                            {
                                uri = new Uri(verifyAppEndpoint);
                                using (var appResonse = await client.GetAsync(uri))
                                {
                                    var appContent = await appResonse.Content.ReadAsStringAsync();
                                    dynamic appObj = (JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(appContent);

                                    if (appObj["id"] != Startup.FacebookAuthOptions.AppId)
                                    {
                                        return null;
                                    }
                                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, iObj["id"].ToString(),
                                        ClaimValueTypes.String, "Facebook", "Facebook"));
                                }

                            }
                        }
                        Claim providerKeyClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                        if (string.IsNullOrEmpty(providerKeyClaim?.Issuer) ||
                            string.IsNullOrEmpty(providerKeyClaim.Value))
                        {
                            return null;
                        }

                        if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                        {
                            return null;
                        }
                        loginData.LoginProvider = provider;
                        loginData.UserName = loginData.Email;
                        return loginData;
                    }
                }
            }
        }


       #endregion
    }
}
