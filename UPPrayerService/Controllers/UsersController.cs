using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UPPrayerService.Models;

namespace UPPrayerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class UsersController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private UserManager<User> UserManager { get; }
        private RoleManager<IdentityRole> RoleManager { get; }
        private SignInManager<User> SignInManager { get; }

        public UsersController(IAuthenticationService auth, IConfiguration configuration, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {

            this.Configuration = configuration;
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.SignInManager = signInManager;
        }

        // POST: api/users/authenticate
        #region Authenticate
        public class AuthenticateRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest request)
        {
            const string InvalidUsernameOrPassword = "Incorrect username or password.";
            User user = await UserManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return this.MakeFailure(InvalidUsernameOrPassword, StatusCodes.Status400BadRequest);
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await SignInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (result.Succeeded)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim("ss", user.SecurityStamp),
                        new Claim("id", user.Id)
                    };
                    foreach (string role in await UserManager.GetRolesAsync(user))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]));
                    SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    JwtSecurityToken token = new JwtSecurityToken(Configuration["Tokens:Issuer"],
                      Configuration["Tokens:Issuer"],
                      claims,
                      expires: DateTime.Now.AddHours(24),
                      signingCredentials: creds);
                    return this.MakeSuccess(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                }
                else
                {
                    if (result.IsLockedOut)
                    {
                        return this.MakeFailure("User is locked out.", StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        return this.MakeFailure(InvalidUsernameOrPassword, StatusCodes.Status400BadRequest);
                    }
                }

            }
        }
        #endregion

        // GET: api/users/list
        #region List
        public class UserInfo
        {
            public string ID { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public IEnumerable<string> Roles { get; set; }

            public UserInfo()
            {

            }

            public UserInfo(User user, UserManager<User> manager, bool includePrivate)
            {
                this.ID = includePrivate ? user.Id : null;
                this.Username = user.UserName;
                this.DisplayName = user.DisplayName;
                this.Email = includePrivate ? user.Email : null;
                this.Roles = new List<string>(manager.GetRolesAsync(user).Result);
            }
        }

        [Authorize(Roles = Models.User.ROLE_SPECTATOR)]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            List<UserInfo> users = new List<UserInfo>();
            bool includePrivate = HttpContext.User.IsInRole(Models.User.ROLE_ADMIN);

            foreach (User user in UserManager.Users)
            {
                users.Add(new UserInfo(user, UserManager, includePrivate));
            }

            return this.MakeSuccess(users);
        }
        #endregion

        // GET: api/users/user/<id>
        #region User
        [Authorize(Roles = Models.User.ROLE_SPECTATOR)]
        [HttpGet("user/{id}")]
        public new async Task<IActionResult> User(string id)
        {
            // Include private data if the user is an admin or is accessing themself.
            bool includePrivate = HttpContext.User.IsInRole(Models.User.ROLE_ADMIN) || id == HttpContext.User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return this.MakeFailure("Could not find any user with the given ID.", StatusCodes.Status404NotFound);
            }
            else
            {
                UserInfo info = new UserInfo(user, UserManager, includePrivate);
                return this.MakeSuccess(info);
            }
        }
        #endregion

        // POST: api/users/create
        #region Create
        public class CreateRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public IEnumerable<string> Roles { get; set; }
        }

        [Authorize(Roles = Models.User.ROLE_ADMIN)]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateRequest request)
        {
            User newUser = new User(request.Username)
            {
                DisplayName = request.DisplayName,
                Email = request.Email,
            };
            IdentityResult createResult = await UserManager.CreateAsync(newUser, request.Password);
            if (createResult.Succeeded)
            {
                foreach (string role in request.Roles)
                {
                    await UserManager.AddToRoleAsync(newUser, role);
                }
                // TODO: Save database
                return this.MakeSuccess();
            }
            else
            {
                return this.MakeFailure("Could not create user (" + createResult.Errors.Count() + " errors): " + String.Join(", ", createResult.Errors.Select(error => error.Description)), StatusCodes.Status400BadRequest);
            }
        }
        #endregion

        // POST: api/users/update
        #region Update
        [Authorize(Roles = Models.User.ROLE_SPECTATOR)]
        [HttpPost("update")]
        public async Task<IActionResult> Update(UserInfo request)
        {
            bool isAdmin = HttpContext.User.IsInRole(Models.User.ROLE_ADMIN);
            bool isEditingSelf = request.ID == HttpContext.User.FindFirst("id").Value;

            // If the user is not an admin and is not updating themselves, disallow the update.
            if (!isAdmin && !isEditingSelf)
            {
                return this.MakeFailure("Only users with the admin role can update other users.", StatusCodes.Status401Unauthorized);
            }

            User targetUser = await UserManager.FindByIdAsync(request.ID);
            if (targetUser == null)
            {
                return this.MakeFailure("No such user exists.", StatusCodes.Status404NotFound);
            }
            else
            {
                // If the user is not an admin and they are trying to edit roles, that is not allowed.
                if (!isAdmin && !System.Linq.Enumerable.SequenceEqual(request.Roles, await UserManager.GetRolesAsync(targetUser)))
                {
                    return this.MakeFailure("Only users with the admin role can modify roles.", StatusCodes.Status401Unauthorized);
                }

                // If the user is an admin and are trying to remove their admin role, that is not allowed.
                if (isAdmin && await UserManager.IsInRoleAsync(targetUser, Models.User.ROLE_ADMIN) && !request.Roles.Contains(Models.User.ROLE_ADMIN))
                {
                    return this.MakeFailure("Users with the admin role may not remove their own admin role.", StatusCodes.Status401Unauthorized);
                }

                // If any of the requested roles don't exist, that's a 404
                foreach (string role in request.Roles)
                {
                    if (!await RoleManager.RoleExistsAsync(role))
                    {
                        return this.MakeFailure("Role '" + role + "' does not exist.", StatusCodes.Status404NotFound);
                    }
                }

                targetUser.DisplayName = request.DisplayName;
                await UserManager.SetEmailAsync(targetUser, request.Email);
                await UserManager.SetUserNameAsync(targetUser, request.Username);
                await UserManager.RemoveFromRolesAsync(targetUser, await UserManager.GetRolesAsync(targetUser));
                await UserManager.AddToRolesAsync(targetUser, request.Roles);

                return this.MakeSuccess();
            }
        }
        #endregion

        // POST: api/users/resetpassword
        #region Reset Password

        #endregion

        // POST: api/users/setpassword
        #region Set Password

        #endregion

        // POST: api/users/delete
        #region Delete
        public class DeleteRequest
        {
            public string ID { get; set; }
        }

        [Authorize(Roles = Models.User.ROLE_ADMIN)]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(DeleteRequest request)
        {
            // If the user is trying to delete themselves, that's an invalid request.
            if (request.ID == HttpContext.User.FindFirst("id").Value)
            {
                return this.MakeFailure("Users may not delete their own account.", StatusCodes.Status403Forbidden);
            }

            User user = await UserManager.FindByIdAsync(request.ID);
            if (user == null)
            {
                return this.MakeFailure("No such user exists.", StatusCodes.Status404NotFound);
            }
            else
            {
                await UserManager.DeleteAsync(user);
                return this.MakeSuccess();
            }
        }
        #endregion
    }
}