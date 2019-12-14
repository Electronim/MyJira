using MyJira.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJira.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: User
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UserList = users;
            return View();
        }

    }
}