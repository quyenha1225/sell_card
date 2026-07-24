using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.EF;

namespace BBEcom.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            using (var db = new KeyDbContext())
            {
                ViewBag.ProductCount = db.Products.Count();
                ViewBag.CategoryCount = db.Categories.Count();
                ViewBag.UserCount = db.Accounts.Count();
                ViewBag.CodeCount = db.Giftcards.Count();
                ViewBag.RecentProducts = db.Products.OrderByDescending(x => x.CreatedDate).Take(6).ToList();
            }
            return View();
        }
    }
}
