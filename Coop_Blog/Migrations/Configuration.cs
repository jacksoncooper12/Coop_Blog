namespace Coop_Blog.Migrations
{
    using Coop_Blog.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Coop_Blog.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Coop_Blog.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));
            if (!context.Roles.Any (r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Moderator"))
            {
                roleManager.Create(new IdentityRole { Name = "Moderator" });
            }
            //code that creates a user
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));
            //look for presence of a user with a specified Email
            //IF it is NOT found, I will create a user with that email
            if(!context.Users.Any(u => u.Email == "jacksoncooper12@gmail.com"))
            {
                userManager.Create(new ApplicationUser()
                {
                    Email = "jacksoncooper12@gmail.com",
                    UserName = "jacksoncooper12@gmail.com",
                    FirstName = "Jackson",
                    LastName = "Cooper",
                    DisplayName = "Coop",                    
                    
                }, "Coopdawg12!");
                //Step1 : Grab the Id that was just created by adding the above user
                var userId = userManager.FindByEmail("jacksoncooper12@gmail.com").Id;
                //Assing the created user with a secific role
                userManager.AddToRole(userId, "Admin");
                //Homework Write the code that creates the user to occupy the Moderator Role
                //assign that user the Moderator role
            }
            if (!context.Users.Any(u => u.Email == "AndrewRussell@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser()
                {
                    Email = "AndrewRussell@coderfoundry.com",
                    UserName = "AndrewRussell@coderfoundry.com",
                    FirstName = "Andrew",
                    LastName = "Russell",
                    DisplayName = "The Stache",

                }, "The$tache15"); //password
                //Step1 : Grab the Id that was just created by adding the above user
                var userId = userManager.FindByEmail("AndrewRussell@coderfoundry.com").Id;
                //Assing the created user with a secific role
                userManager.AddToRole(userId, "Moderator");
            }

        }
    }
}
