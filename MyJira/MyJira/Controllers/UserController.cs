using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MyJira.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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

        public ActionResult Edit(string id)
        {
            var user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();
            user.AllTeams = GetAllTeams();

            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newUser, HttpPostedFileBase file)
        {
            var user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            user.AllTeams = GetAllTeams();

            try
            {
                var context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.UserName = newUser.UserName;
                    user.Email = newUser.Email;
                    user.PhoneNumber = newUser.PhoneNumber;
                    user.TeamId = newUser.TeamId;

                    string photoPath = null;
                    if (file != null)
                    {
                        if (file.ContentLength <= 0)
                        {
                            throw new Exception("Error while uploading");
                        }

                        photoPath = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Photos"), photoPath);
                        file.SaveAs(path);
                    }
                    user.PhotoPath = photoPath;

                    var roles = from role in db.Roles select role;
                    foreach (var role in roles)
                    {
                        userManager.RemoveFromRole(id, role.Name);
                    }

                    var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    userManager.AddToRole(id, selectedRole.Name);
                    db.SaveChanges();

                    TempData["message"] = "Dev has been edited successfully";
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = userManager.Users.FirstOrDefault(u => u.Id == id);

            userManager.Delete(user);
            db.SaveChanges();

            TempData["message"] = "Dev has been removed succesfully";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                if (role.Name.Equals("Organizer"))
                {
                    continue;
                }
                selectList.Add(new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                });
            }
            return selectList;
        }

        public IEnumerable<SelectListItem> GetAllTeams()
        {
            var selectList = new List<SelectListItem> {new SelectListItem {Value = null, Text = null}};

            var teams = from team in db.Teams select team;
            foreach (var team in teams)
            {
                selectList.Add(new SelectListItem
                {
                    Value = team.Id.ToString(),
                    Text = team.Name.ToString()
                });
            }

            return selectList;
        }
    }
}