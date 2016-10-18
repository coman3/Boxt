using System;
using System.Collections.Generic;
#pragma warning disable 1591
namespace TextIt.Models
{
    // Models returned by AccountController actions.

    public class ExternalLoginViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string State { get; set; }
    }

    public class ManageInfoViewModel
    {
        public string LocalLoginProvider { get; set; }

        public string Email { get; set; }

        public IEnumerable<UserLoginInfoViewModel> Logins { get; set; }

    }

    public class UserInfoViewModel
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string ProfilePicture { get; set; }
        public string CoverPicture { get; set; }
        public string Gender { get; set; }
    }

    public class UserLoginInfoViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}
