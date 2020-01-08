using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using MyJira.Models;
using Owin;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;

[assembly: OwinStartupAttribute(typeof(MyJira.Startup))]
namespace MyJira
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // InitialInsert();
            createAdminUserAndApplicationRoles();
        }

        private void createAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            context.Configuration.LazyLoadingEnabled = true;

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!roleManager.RoleExists("Administrator"))
            {
                var role = new IdentityRole {Name = "Administrator"};
                roleManager.Create(role);

                var user = new ApplicationUser {UserName = "admin@admin.com", Email = "admin@admin.com"};
                var adminCreated = userManager.Create(user, "Administrator1!");
                if (adminCreated.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Administrator");
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

        private void InitialInsert()
        {
            var db = ApplicationDbContext.Create();

            var project = new Project
            {
                Name = "Default",
                Description = "default project"
            };

            if (db.Projects.Find(1) == null)
            {
                db.Projects.Add(project);
            }

            var team = new Team
            {
                Name = "Bench",
                ProjectId = 1
            };

            if (db.Teams.Find(1) == null)
            {
                db.Teams.Add(team);
            }
            db.SaveChanges();
        }
    }
}
