using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyJira.Models;

namespace MyJira.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Project
        public ActionResult Index()
        {
            var projects =  from project in db.Projects
                            orderby project.Name
                            select project;

            ViewBag.Projects = projects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View();
        }

        public ActionResult New()
        {
            var project = new Project();
            
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