using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MyJira.Models;

namespace MyJira.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Project
        public ActionResult Index()
        {
            var projects = db.Projects.Include("User");
            ViewBag.Projects = projects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View();
        }

        public ActionResult Show(int id)
        {
            var project = db.Projects.Find(id);

            return View(project);
        }

        public ActionResult New()
        {
            var project = new Project
            {
                UserId = User.Identity.GetUserId()
            };
            return View(project);
        }

        [HttpPost]
        public ActionResult New(Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Projects.Add(project);
                    db.SaveChanges();
                    TempData["message"] = "The project has been added!";
                    return RedirectToAction("Index");
                }

                return View(project);
            }
            catch (Exception)
            {
                return View(project);
            }
        }
        public ActionResult Edit(int id)
        {
            var project = db.Projects.Find(id);

            return View(project);
        }

        [HttpPut]
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