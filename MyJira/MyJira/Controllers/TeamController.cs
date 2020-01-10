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
    [Authorize]
    public class TeamController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Team
        [Authorize(Roles = "Dev,Organizer,Administrator")]
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

        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Show(int id)
        {
            var userId = User.Identity.GetUserId();
            var team = db.Teams.Find(id);
            var tasksPerTeam =
                from task in db.Tasks
                where task.TeamId == id
                select task;

            ViewBag.ProjectName = team.Project.Name;
            ViewBag.TasksPerTeam = tasksPerTeam;
            ViewBag.DevsWithoutTeam = GetAllDevsWihoutTeams();
            ViewBag.showButtons = false;

            if (User.IsInRole("Organizer") || User.IsInRole("Administrator"))
            {
                ViewBag.showButtons = true;
            }

            ViewBag.userIsAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = User.Identity.GetUserId();
            ViewBag.TeamIdUser = db.Users.Find(userId).TeamId;

            ViewBag.Todo = db.Tasks.Where(m => m.TeamId == id && m.Status == TaskStatus.Todo).ToList();
            ViewBag.InProgress = db.Tasks.Where(m => m.TeamId == id && m.Status == TaskStatus.InProgress).ToList();
            ViewBag.Done = db.Tasks.Where(m => m.TeamId == id && m.Status == TaskStatus.Done).ToList();

            return View(team);
        }

        [Authorize(Roles = "Organizer,Administrator")]
        public ActionResult New(int id)
        {
            var team = new Team
            {
                ProjectId = id
            };

            return View(team);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer,Administrator")]
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

        [Authorize(Roles = "Organizer,Administrator")]
        public ActionResult Edit(int id)
        {
            var team = db.Teams.Find(id);
            return View(team);
        }

        [HttpPut]
        [Authorize(Roles = "Organizer,Administrator")]
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

                    return RedirectToAction("Show", "Team", new { id = team.Id });
                }
                
                return View(requestTeam);
            }
            catch (Exception)
            {
                return View(requestTeam);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organizer,Administrator")]
        public ActionResult Delete(int id)
        {
            var team = db.Teams.Find(id);
            var users = db.Users.Where(m => m.TeamId == team.Id).ToList();
            foreach (var user in users)
            {
                user.TeamId = null;
            }
            db.Teams.Remove(team);
            db.SaveChanges();
            TempData["message"] = "Team has been deleted successfully!";
            return RedirectToAction("Show", "Project", new {id = team.ProjectId});
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