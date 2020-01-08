using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using MyJira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJira.Controllers
{
    public class TaskController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Task
        public ActionResult Index()
        {
            var tasks = from task in db.Tasks
                        orderby task.Title
                        select task;
            ViewBag.tasks = tasks;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
        }

        public ActionResult Show(int id)
        {
            var task = db.Tasks.Find(id);
            ViewBag.Comments = db.Comments.Where(m => m.TaskId == id).ToList();
            ViewBag.CurrentUser = User.Identity.GetUserId();
            return View(task);
        }

        // TODO: has to be possible only if the organizer is trying to add tasks in the curr project
        public ActionResult New()
        {
            var task = new Task
            {
                ReporterId = User.Identity.GetUserId()
            };
            return View(task);
        }

        [HttpPost]
        public ActionResult New(Task task)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var teamId =
                        (from user in db.Users
                        where user.Id == task.ReporterId
                        select user.TeamId).ToArray()[0];

                // task.FinishTime = DateTime.Now; // altfel seteaza by default la o data care iese din range-ul SQL
                db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "Task has been created successfully!";
                    return RedirectToAction("Show", "Team", new { id = teamId });
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

        public ActionResult Edit(int id)
        {
            var task = db.Tasks.Find(id);
            ViewBag.allDevs = GetAllDevs();
            ViewBag.allStatuses = GetAllStatuses();
            return View(task);
        }

        [HttpPut]
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

                    var teamId =
                        (from user in db.Users
                            where user.Id == task.ReporterId
                            select user.TeamId).ToArray()[0];
                    return RedirectToAction("Show", "Team", new { id = teamId });
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
        public ActionResult Delete(int id)
        {
            var task = db.Tasks.Find(id);
            db.Tasks.Remove(task);
            db.SaveChanges();
            TempData["message"] = "Task has been deleted successfully";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDevs()
        {
            var devs = from user in db.Users
                       orderby user.UserName
                       select user;

            return devs.Select(dev => new SelectListItem {Value = dev.Id.ToString(), Text = dev.UserName.ToString()}).ToList();
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

        public ActionResult EditComment(int id)
        {
            var comment = db.Comments.Find(id);
            return View(comment);
        }

        [HttpPut]
        [ValidateInput(false)]
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
        public ActionResult DeleteComment(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
            db.SaveChanges();
            TempData["message"] = "Comment has been deleted successfully!";

            return RedirectToAction("Show/" + comment.TaskId);
        }

    }
}