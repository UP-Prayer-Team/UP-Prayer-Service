using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UPPrayerService.Models
{
    public class User : Microsoft.AspNetCore.Identity.IdentityUser
    {
        public const string ROLE_ADMIN = "admin";
        public const string ROLE_SPECTATOR = "spectator";
        public static readonly string[] ALL_ROLES = { ROLE_ADMIN, ROLE_SPECTATOR };

        public string DisplayName { get; set; } = "";

        public User()
        {

        }

        public User(string username)
        {
            this.UserName = username;
        }
    }
}
