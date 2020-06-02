using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UPPrayerService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace UPPrayerService
{
    public class DataContext : IdentityDbContext<Models.User>
    {
        private const string DEFAULT_USERNAME = "admin";
        private const string DEFAULT_PASSWORD = "ThePowerOfPrayer1!";

        public DbSet<Endorsement> Endorsements { get; set; }
        public DbSet<Confirmation> Confirmations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }

        public DataContext(DbContextOptions<DataContext> dataOptions) : base(dataOptions)
        {
            
        }

        public async void Initialize(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure that all roles exist in the DB
            foreach (string role in User.ALL_ROLES)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // If there are no users, create the default user.
            if (await this.Users.CountAsync() == 0)
            {
                Console.WriteLine("[INFO]: No users in database. Creating default user. (See DataContext.cs)");
                User newUser = new User(DEFAULT_USERNAME);
                IdentityResult createResult = await userManager.CreateAsync(newUser, DEFAULT_PASSWORD);
                if (createResult.Succeeded)
                {
                    //await this.SaveChangesAsync();
                    await userManager.AddToRoleAsync(newUser, User.ROLE_SPECTATOR);
                    await userManager.AddToRoleAsync(newUser, User.ROLE_ADMIN);
                    await this.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Couldn't create default user: " + createResult.ToString());
                }
            }

            // if the other o endorsement is not in the database then create
            if (!this.Endorsements.Any((Endorsement e) => e.ID == new Guid().ToString())) 
            {
                Endorsement otherEndorsement = new Endorsement() { Name = "Other", ID = new Guid().ToString(), DonateURL = "", HomepageURL = "", Summary = "" };
                Endorsements.Add(otherEndorsement);
                await this.SaveChangesAsync();
            }
        }
    }
}
