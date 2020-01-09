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

            var tasksPerTeam = db.Tasks.Where(m => m.TeamId == id).ToList();

            ViewBag.TasksPerTeam = tasksPerTeam;
            ViewBag.DevsWithoutTeam = GetAllDevsWihoutTeams();
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

        [HttpPut]
        public ActionResult AddDev(int teamId)
        {
            string devId = HttpContext.Request.Params.Get("newDev");
            if (devId != null)
            {
                try
                {
                    var selectedDev = db.Users.Find(devId);
                    if (ModelState.IsValid)
                    {
                        if (TryUpdateModel(selectedDev))
                        {
                            selectedDev.TeamId = teamId;
                            db.SaveChanges();
                            TempData["message"] = "Dev has been added to the team";
                            return RedirectToAction("Show/" + teamId);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Show/" + teamId);
                    }
                }
                catch (Exception e)
                {
                    return RedirectToAction("Show/" + teamId);
                }
            }
            return RedirectToAction("Show/" + teamId);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDevsWihoutTeams()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var devs = db.Users.Where(m => m.TeamId == null).OrderBy(m => m.UserName).ToList();
            var devsWithoutTeam = db.Users.Where(m => m.TeamId == null).OrderBy(m => m.UserName).ToList();

            foreach (var dev in devs)
            {
                if (userManager.IsInRole(dev.Id, "Administrator") || userManager.IsInRole(dev.Id, "Organizer"))
                {
                    devsWithoutTeam.Remove(dev);
                }
            }
            var finalDevs =  devsWithoutTeam.Select(dev => new SelectListItem { Value = dev.Id.ToString(), Text = dev.UserName.ToString() }).ToList();
            finalDevs.Insert(0, null);
            return finalDevs;
        }
    }
}