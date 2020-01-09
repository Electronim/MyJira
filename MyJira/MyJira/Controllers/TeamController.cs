using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MyJira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJira.Controllers
{
    public class TeamController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Team
        public ActionResult Index()
        {
            var teams = from team in db.Teams.Include("Project")
                             orderby team.Name
                             select team;
            ViewBag.Teams = teams;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View();
        }

        public ActionResult Show(int id)
        {
            var team = db.Teams.Find(id);

            var tasksPerTeam =
                from t in db.Teams
                join user in db.Users on t.Id equals user.TeamId
                join task in db.Tasks on user.Id equals task.ReporterId
                where t.Id == id
                select task;

            ViewBag.TasksPerTeam = tasksPerTeam;
            return View(team);
        }

        public ActionResult New(int id)
        {
            var team = new Team
            {
                ProjectId = id
            };

            return View(team);
        }

        [HttpPost]
        public ActionResult New(Team team)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Teams.Add(team);
                    db.SaveChanges();
                    TempData["message"] = "Team has been added successfully!";
                    return RedirectToAction("Show", "Project", new { id = team.ProjectId });
                }
                
                return View(team);
            }
            catch (Exception)
            {
                return View(team);
            }
        }

        public ActionResult Edit(int id)
        {
            var team = db.Teams.Find(id);
            return View(team);
        }

        [HttpPut]
        public ActionResult Edit(int id, Team requestTeam)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var team = db.Teams.Find(id);
                    if (TryUpdateModel(team))
                    {
                        team.Name = requestTeam.Name;
                        db.SaveChanges();
                        TempData["message"] = "Team has been modified successfully!";
                    }

                    return RedirectToAction("Show", "Project", new { id = team.ProjectId });
                }
                else
                    return View(requestTeam);
            }
            catch (Exception)
            {
                return View(requestTeam);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
            TempData["message"] = "Team has been deleted successfully!";
            return RedirectToAction("Index");
        }

        //[NonAction]
        //public IEnumerable<SelectListItem> GetAllDevsWihout()
        //{
        //    var context = new ApplicationDbContext();
        //    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        //    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

        //    var devs = from user in db.Users
        //               orderby user.UserName
        //               select user;



        //    return devs.Select(dev => new SelectListItem { Value = dev.Id.ToString(), Text = dev.UserName.ToString() }).ToList();
        //}
    }
}