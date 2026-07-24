using Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index(int page = 1)
        {
            ViewBag.ProductPage = Math.Max(1, page);
            return View();
        }

        public PartialViewResult Categories()
        {
            var model = new CategoryDAO().GetCategoriesName(null);
            return PartialView(model);
        }

        public PartialViewResult NewsPost()
        {
            var model = new NewsDAO().GetNews();
            return PartialView(model);
        }

        public PartialViewResult ListProduct(int page = 1)
        {
            var list = new ProductDAO().GetStorefrontProducts(Math.Max(1, page), 8);
            return PartialView(list);
        }
    }
}
