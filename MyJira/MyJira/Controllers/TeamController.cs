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
        // GET: Category
        public ActionResult Index()
        {
            var teams = from team in db.Teams
                             orderby team.TeamName
                             select team;
            ViewBag.Teams = teams;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View();

        }

        //public ActionResult Show(int id)
        //{
        //    Category category = db.Categories.Find(id);
        //    return View(category);
        //}

        public ActionResult New()
        {
            Team team = new Team();
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
                    TempData["message"] = "Team has been added!";
                    return RedirectToAction("Index");
                }
                else
                    return View(team);
            }
            catch (Exception e)
            {
                return View(team);
            }
        }

        //public ActionResult Edit(int id)
        //{
        //    Category category = db.Categories.Find(id);
        //    return View(category);
        //}

        //[HttpPut]
        //public ActionResult Edit(int id, Category requestCategory)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            Category category = db.Categories.Find(id);
        //            if (TryUpdateModel(category))
        //            {
        //                category.CategoryName = requestCategory.CategoryName;
        //                db.SaveChanges();
        //                TempData["message"] = "Categoria a fost editata!";
        //            }
        //            return RedirectToAction("Index");
        //        }
        //        else
        //            return View(requestCategory);
        //    }
        //    catch (Exception e)
        //    {
        //        return View(requestCategory);
        //    }
        //}

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Team team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
            TempData["message"] = "Team has been deleted!";
            return RedirectToAction("Index");
        }
    }
}