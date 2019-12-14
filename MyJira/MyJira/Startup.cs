using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using MyJira.Models;
using Owin;
using System;
using System.Data.Entity.Validation;

[assembly: OwinStartupAttribute(typeof(MyJira.Startup))]
namespace MyJira
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            initialInsert();
            createAdminUserAndApplicationRoles();
        }

        private void createAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            context.Configuration.LazyLoadingEnabled = true;

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Administrator"))
            {
                var role = new IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);

                var user = new ApplicationUser();
                user.UserName = "admin@admin.com";
                user.Email = "admin@admin.com";
                user.TeamId = 1;
                var adminCreated = UserManager.Create(user, "Administrator1!");
                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Administrator");
                }
                context.SaveChanges();
            }

            if (!roleManager.RoleExists("Organizer"))
            {
                var role = new IdentityRole();
                role.Name = "Organizer";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("Dev"))
            {
                var role = new IdentityRole();
                role.Name = "Dev";
                roleManager.Create(role);
            }
        }

        private void initialInsert()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            Team team = new Team();
            team.TeamName = "Bench";
            if (db.Teams.Find(1) == null)
            {
                db.Teams.Add(team);
            }
            db.SaveChanges();
        }
    }
}
