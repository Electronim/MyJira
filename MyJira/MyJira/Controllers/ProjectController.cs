using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using MyJira.Models;

namespace MyJira.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Project
        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Index()
        {
            var projects = db.Projects;
            ViewBag.Projects = projects;


            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View();
        }

        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Show(int id)
        {
            var project = db.Projects.Find(id);

            return View(project);
        }

        [Authorize(Roles="Dev,Administrator")]
        public ActionResult New()
        {
            if (User.IsInRole("Dev"))
            {
                var userId = User.Identity.GetUserId();
                var teamId =
                    (from user in db.Users
                    where user.Id == userId
                    select user.TeamId).ToArray().FirstOrDefault();

                if (teamId != null)
                {
                    TempData["message"] = "You cannot create a project (you are assigned to a team)";
                    return RedirectToAction("Index");
                }
            }

            var project = new Project
            {
                LeaderId = User.Identity.GetUserId()
            };
            return View(project);
        }

        [HttpPost]
        [Authorize(Roles = "Dev,Administrator")]
        public ActionResult New(Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Projects.Add(project);
                    db.SaveChanges();
                    TempData["message"] = "The project has been added!";

                    if (User.IsInRole("Dev"))
                    {
                        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                        var roles = (from role in db.Roles select role).ToArray();
                        var userId = User.Identity.GetUserId();

                        foreach (var role in roles)
                        {
                            userManager.RemoveFromRole(userId, role.Name);
                        }

                        userManager.AddToRole(userId, "Organizer");
                        db.SaveChanges();

                        var authenticationManager = HttpContext.GetOwinContext().Authentication;
                        authenticationManager.SignOut("ApplicationCookie");
                    }

                    return RedirectToAction("Index");
                }

                return View(project);
            }
            catch (Exception)
            {
                return View(project);
            }
        }

        [Authorize(Roles = "Organizer")]
        public ActionResult Edit(int id)
        {
            var project = db.Projects.Find(id);

            return View(project);
        }

        [HttpPut]
        [Authorize(Roles = "Organizer")]
        public ActionResult Edit(int id, Project requestProject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var project = db.Projects.Find(id);

                    if (TryUpdateModel(project))
                    {
                        var oldName = project.Name;

                        project.Name = requestProject.Name;
                        project.Description = requestProject.Description;

                        db.SaveChanges();
                        TempData["message"] = "The project " + oldName + " has been modified!";
                    }

                    return RedirectToAction("Index");
                }

                return View(requestProject);
            }
            catch (Exception)
            {
                return View(requestProject);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organizer,Administrator")]
        public ActionResult Delete(int id)
        {
            var project = db.Projects.Find(id);

            if (project != null)
            {
                db.Projects.Remove(project);
                db.SaveChanges();
                TempData["message"] = "Project " + project.Name + " was deleted!";
            }

            return RedirectToAction("Index");
        }
    }
}