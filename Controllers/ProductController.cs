using Models.DAO;
using System;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return RedirectToAction("Search");
        }

        // Trang danh mục sản phẩm.
        public ActionResult Category(
            int id,
            string sort = "sale",
            int page = 1,
            int pageSize = 8)
        {
            var categoryDao = new CategoryDAO();
            var category = categoryDao.GetCategoryById(id);

            if (category == null || category.Status != true)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryProductCount = categoryDao.GetCountProductCategory(id);
            ViewBag.Sort = NormalizeSort(sort);
            ViewBag.Page = Math.Max(1, page);
            ViewBag.PageSize = NormalizePageSize(pageSize, 8);

            return View(category);
        }

        // Trang chi tiết sản phẩm.
        public ActionResult Detail(int id)
        {
            var product = new ProductDAO().GetProductById(id);

            if (product == null || product.Status != true)
            {
                return HttpNotFound();
            }

            ViewBag.Stock = new GiftcardDAO().GetStock(id);
            return View(product);
        }

        // Partial danh sách sản phẩm của một danh mục.
        [ChildActionOnly]
        public ActionResult Products(
            int id,
            string sort = "sale",
            int page = 1,
            int pageSize = 8)
        {
            page = Math.Max(1, page);
            pageSize = NormalizePageSize(pageSize, 8);
            sort = NormalizeSort(sort);

            var category = new CategoryDAO().GetCategoryById(id);
            if (category == null || category.Status != true)
            {
                return HttpNotFound();
            }

            var products = new ProductDAO().GetListProductByCategory(
                id,
                sort,
                page,
                pageSize);

            ViewBag.CategoryId = id;
            ViewBag.CategoryAlias = category.Alias;
            ViewBag.Sort = sort;
            ViewBag.PageSize = pageSize;

            return PartialView("~/Views/Shared/Products.cshtml", products);
        }

        // Tìm kiếm, sắp xếp và phân trang sản phẩm ngoài storefront.
        [HttpGet]
        public ActionResult Search(
            string keyword,
            string sort = "sale",
            int page = 1,
            int pageSize = 12)
        {
            keyword = (keyword ?? string.Empty).Trim();
            page = Math.Max(1, page);
            pageSize = NormalizePageSize(pageSize, 12);
            sort = NormalizeSort(sort);

            var products = new ProductDAO().SearchStorefrontProducts(
                keyword,
                sort,
                page,
                pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.Sort = sort;
            ViewBag.PageSize = pageSize;

            return View(products);
        }

        private static int NormalizePageSize(int pageSize, int defaultValue)
        {
            switch (pageSize)
            {
                case 8:
                case 12:
                case 16:
                case 24:
                    return pageSize;
                default:
                    return defaultValue;
            }
        }

        private static string NormalizeSort(string sort)
        {
            switch ((sort ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "latest":
                case "price_asc":
                case "price_desc":
                case "name_asc":
                case "name_desc":
                case "sale":
                    return sort.Trim().ToLowerInvariant();
                default:
                    return "sale";
            }
        }
    }
}
