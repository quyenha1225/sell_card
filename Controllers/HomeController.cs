using Models.DAO;
using System;
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

        [ChildActionOnly]
        [OutputCache(Duration = 600, VaryByParam = "none")]
        public PartialViewResult Categories()
        {
            var model = new CategoryDAO().GetCategoriesName(null);
            return PartialView(model);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 300, VaryByParam = "none")]
        public PartialViewResult NewsPost()
        {
            var model = new NewsDAO().GetNews();
            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult ListProduct(int page = 1)
        {
            var model = new ProductDAO()
                .GetStorefrontProducts(Math.Max(1, page), 8);

            return PartialView(model);
        }
    }
}
