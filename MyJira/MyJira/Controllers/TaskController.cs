using Microsoft.AspNet.Identity;
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
            Task task = db.Tasks.Find(id);
            return View(task);
        }

        public ActionResult New()
        {
            Task task = new Task();
            task.ReporterId = User.Identity.GetUserId();
            return View(task);
        }

        [HttpPost]
        public ActionResult New(Task task)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    task.FinishTime = DateTime.Now; // altfel seteaza by default la o data care iese din range-ul SQL
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "Task has been created succesfully!";
                    return RedirectToAction("Index");
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
            Task task = db.Tasks.Find(id);
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
                    Task task = db.Tasks.Find(id);
                    if (TryUpdateModel(task))
                    {
                        task.Title = newTask.Title;
                        task.Description = newTask.Description;
                        task.Status = newTask.Status;
                        task.AssigneeId = newTask.AssigneeId;
                        if (task.Status == TaskStatus.Done)
                        {
                            task.FinishTime = DateTime.Now;
                        }
                        db.SaveChanges();
                        TempData["message"] = "Task has been modified successfully";
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(newTask);
                }
            }
            catch (Exception e)
            {
                return View(newTask);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Find(id);
            db.Tasks.Remove(task);
            db.SaveChanges();
            TempData["message"] = "Task has been deleted successfully";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDevs()
        {
            var selectList = new List<SelectListItem>();
            var devs = from user in db.Users
                       orderby user.UserName
                       select user;

            foreach (var dev in devs)
            {
                selectList.Add(new SelectListItem
                {
                    Value = dev.Id.ToString(),
                    Text = dev.UserName.ToString()
                });
            }
            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStatuses()
        {
            var selectList = new List<SelectListItem>();
            foreach (TaskStatus taskStatus in (TaskStatus[])Enum.GetValues(typeof(TaskStatus)))
            {
                selectList.Add(new SelectListItem
                {
                    Value = taskStatus.ToString(),
                    Text = taskStatus.ToString()
                });
            }
            return selectList;
        }
    }
}