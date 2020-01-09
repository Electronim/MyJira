using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using MyJira.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJira.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Task
        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Index()
        {
            var tasks = from task in db.Tasks
                        orderby task.Title
                        select task;

            var projects = from project in db.Projects
                select project;
            
            var projectNames = new Dictionary<int, string>();
            foreach (var project in projects)
            {
                projectNames.Add(project.Id, project.Name);
            }

            ViewBag.tasks = tasks;
            ViewBag.projectNames = projectNames;
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
            var task = db.Tasks.Find(id);
            ViewBag.Comments = db.Comments.Where(m => m.TaskId == id).ToList();

            var project = db.Projects.Find(task.ProjectId);
            ViewBag.Project = project;

            ViewBag.showButtons = false;

            if (User.IsInRole("Organizer") || User.IsInRole("Administrator"))
            {
                ViewBag.showButtons = true;
            }

            ViewBag.userIsAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = userId;
            ViewBag.TeamIdUser = db.Users.Find(userId).TeamId;
            return View(task);
        }

        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult New(int id)  // id is actually the team id from where the task is created
        {
            var team = db.Teams.Find(id);
            var currentUserId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);
            if ((User.IsInRole("Organizer") && team.Project.LeaderId == User.Identity.GetUserId()) || (User.IsInRole("Dev") && (currentUser.TeamId == team.Id)) || User.IsInRole("Administrator"))
            {
                var task = new Task
                {
                    ReporterId = User.Identity.GetUserId(),
                    TeamId = team.Id,
                    ProjectId = team.ProjectId
                };
                return View(task);
            }
            else
            {
                TempData["message"] = "You do not have permissions!";
                return RedirectToAction("Show", "Team", new { id = team.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult New(Task task)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "Task has been created successfully!";

                    return RedirectToAction("Show", "Team", new { id = task.TeamId });
                }
                else
                {
                    return View(task);
                }
            }
            catch (Exception e)
            {
                return View(task);
            }
        }

        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Edit(int id)
        {
            var task = db.Tasks.Find(id);
            var project = db.Projects.Find(task.ProjectId);
            ViewBag.Project = project;
            ViewBag.allDevs = GetAllDevs();
            ViewBag.allStatuses = GetAllStatuses();
            return View(task);
        }

        [HttpPut]
        [Authorize(Roles = "Dev,Organizer,Administrator")]
        public ActionResult Edit(int id, Task newTask)
        {
            ViewBag.allDevs = GetAllDevs();
            ViewBag.allStatuses = GetAllStatuses();
            try
            {
                if (ModelState.IsValid)
                {
                    var task = db.Tasks.Find(id);

                    if (TryUpdateModel(task))
                    {
                        task.Title = newTask.Title;
                        task.Description = newTask.Description;
                        task.Status = newTask.Status;
                        task.ReporterId = newTask.ReporterId;
                        task.AssigneeId = newTask.AssigneeId;
                        if (task.Status == TaskStatus.Done)
                        {
                            task.FinishTime = DateTime.Now;
                        }
                        db.SaveChanges();
                        TempData["message"] = "Task has been modified successfully";
                    }

                    return RedirectToAction("Show", "Task", new { id = task.Id });
                }
                else
                {
                    return View(newTask);
                }
            }
            catch (Exception)
            {
                return View(newTask);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Dev, Organizer,Administrator")]
        public ActionResult Delete(int id)
        {
            var task = db.Tasks.Find(id);
            var teamId = task.TeamId;
            db.Tasks.Remove(task);
            db.SaveChanges();
            TempData["message"] = "Task has been deleted successfully";
            return RedirectToAction("Show", "Team", new {id = teamId});
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDevs()
        {
            var devs = from user in db.Users
                       orderby user.UserName
                       select user;

            return devs.Select(dev => new SelectListItem { Value = dev.Id.ToString(), Text = dev.UserName.ToString() }).ToList();
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStatuses()
        {
            var selectList = new List<SelectListItem>();
            foreach (var taskStatus in (TaskStatus[])Enum.GetValues(typeof(TaskStatus)))
            {
                selectList.Add(new SelectListItem
                {
                    Value = taskStatus.ToString(),
                    Text = taskStatus.ToString()
                });
            }
            return selectList;
        }

        [Authorize(Roles = "Organizer,Dev,Administrator")]
        public ActionResult AddComment(int id)
        {
            var comment = new Comment
            {
                TaskId = id,
                UserId = User.Identity.GetUserId()
            };

            return View(comment);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize(Roles = "Organizer,Dev,Administrator")]
        public ActionResult AddComment(Comment comment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    comment.Content = Sanitizer.GetSafeHtmlFragment(comment.Content);
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    TempData["message"] = "Comment has been added succesfully!";
                    return RedirectToAction("Show/" + comment.TaskId);
                }
                else
                {
                    return View(comment);
                }
            }
            catch (Exception e)
            {
                return View(comment);
            }
        }

        [Authorize(Roles = "Organizer,Dev,Administrator")]
        public ActionResult EditComment(int id)
        {
            var comment = db.Comments.Find(id);
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(comment);
            }
            else
            {
                TempData["message"] = "You have not permissions !";
                return RedirectToAction("Show/" + comment.TaskId);
            }
        }

        [HttpPut]
        [ValidateInput(false)]
        [Authorize(Roles = "Organizer,Dev,Administrator")]
        public ActionResult EditComment(int id, Comment newComment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var comment = db.Comments.Find(id);
                    if (TryUpdateModel(comment))
                    {
                        newComment.Content = Sanitizer.GetSafeHtmlFragment(newComment.Content);

                        comment.Content = newComment.Content;
                        comment.Date = newComment.Date;

                        db.SaveChanges();
                        TempData["message"] = "Comment has been modified successfully";
                    }
                    return RedirectToAction("Show/" + newComment.TaskId);
                }
                else
                {
                    return View(newComment);
                }
            }
            catch (Exception)
            {
                return View(newComment);
            }
        }


        [HttpDelete]
        [Authorize(Roles = "Organizer,Dev,Administrator")]
        public ActionResult DeleteComment(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                TempData["message"] = "Comment has been deleted successfully!";
            }
            else
            {
                TempData["message"] = "You have not permissions !";
            }
            return RedirectToAction("Show/" + comment.TaskId);
        }

    }
}