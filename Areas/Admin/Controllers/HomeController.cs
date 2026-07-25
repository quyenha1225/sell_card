using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.EF;
using System.Data.Entity;

namespace BBEcom.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            using (var db = new KeyDbContext())
            {
                var counts = db.Database.SqlQuery<DashboardCounts>(@"
                    SELECT
                        (SELECT COUNT(*) FROM Product) AS ProductCount,
                        (SELECT COUNT(*) FROM Category) AS CategoryCount,
                        (SELECT COUNT(*) FROM Account) AS UserCount,
                        (SELECT COUNT(*) FROM Giftcard) AS CodeCount").First();
                ViewBag.ProductCount = counts.ProductCount;
                ViewBag.CategoryCount = counts.CategoryCount;
                ViewBag.UserCount = counts.UserCount;
                ViewBag.CodeCount = counts.CodeCount;
                ViewBag.RecentProducts = db.Products.AsNoTracking().OrderByDescending(x => x.CreatedDate).Take(6).ToList();
            }
            return View();
        }

        private class DashboardCounts
        {
            public int ProductCount { get; set; }
            public int CategoryCount { get; set; }
            public int UserCount { get; set; }
            public int CodeCount { get; set; }
        }
    }
}
