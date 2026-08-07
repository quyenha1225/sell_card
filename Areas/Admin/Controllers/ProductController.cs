using BBEcom.Areas.Admin.Models;
using BBEcom.Common;
using Models.DAO;
using Models.EF;
using System;
using System.Web.Mvc;

namespace BBEcom.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        public ActionResult Index(
            string txtSearch, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(5, Math.Min(100, pageSize));

            ViewBag.Search = txtSearch;
            ViewBag.PageSize = pageSize;

            return View(new ProductDAO().GetProducts(
                txtSearch, page, pageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            SetViewBag();
            return View(new ProductAdminViewModel { Status = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductAdminViewModel model)
        {
            NormalizeModel(model);
            SetViewBag(model.CategoryId);
            if (!ModelState.IsValid) return View(model);

            var entity = new Product
            {
                Name = model.Name,
                Alias = CommonHelper.ChuyenCoDauThanhKhongDau(model.Name),
                CategoryId = model.CategoryId,
                Images = model.Images,
                CostPrice = model.CostPrice,
                Price = model.Price,
                PriceSale = model.PriceSale.GetValueOrDefault() > 0
                    ? model.PriceSale : null,
                Detail = model.Detail,
                Status = model.Status,
                CreatedDate = DateTime.Now
            };

            var id = new ProductDAO().AddProduct(entity);

            if (id <= 0)
            {
                ModelState.AddModelError("", "Không thể thêm sản phẩm.");
                return View(model);
            }

            SetAlert("Thêm sản phẩm thành công.", "success");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = new ProductDAO().GetProductById(id);
            if (product == null) return HttpNotFound();

            SetViewBag(product.CategoryId);

            return View(new ProductAdminViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Images = product.Images,
                CostPrice = product.CostPrice,
                Price = product.Price,
                PriceSale = product.PriceSale,
                Detail = product.Detail,
                Status = product.Status.GetValueOrDefault()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductAdminViewModel model)
        {
            NormalizeModel(model);
            SetViewBag(model.CategoryId);
            if (!ModelState.IsValid) return View(model);

            var entity = new Product
            {
                Id = model.Id,
                Name = model.Name,
                Alias = CommonHelper.ChuyenCoDauThanhKhongDau(model.Name),
                CategoryId = model.CategoryId,
                Images = model.Images,
                CostPrice = model.CostPrice,
                Price = model.Price,
                PriceSale = model.PriceSale.GetValueOrDefault() > 0
                    ? model.PriceSale : null,
                Detail = model.Detail,
                Status = model.Status
            };

            if (!new ProductDAO().UpdateProduct(entity))
            {
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm.");
                return View(model);
            }

            SetAlert("Cập nhật sản phẩm thành công.", "success");
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var result = new ProductDAO().DeleteProduct(id);

            SetAlert(
                result
                    ? "Xóa sản phẩm thành công."
                    : "Không thể xóa sản phẩm đang có dữ liệu liên quan.",
                result ? "warning" : "error");

            return RedirectToAction("Index");
        }

        private void NormalizeModel(ProductAdminViewModel model)
        {
            if (model == null)
            {
                return;
            }

            model.Name = (model.Name ?? String.Empty).Trim();
            model.Images = (model.Images ?? String.Empty).Trim();

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    "Name",
                    "Vui lòng nhập tên sản phẩm."
                );
            }

            if (String.IsNullOrWhiteSpace(model.Images))
            {
                ModelState.AddModelError(
                    "Images",
                    "Vui lòng nhập đường dẫn ảnh."
                );
            }
        }

        private void SetViewBag(int? selectedId = null)
        {
            ViewBag.productCategoryID = new SelectList(
                new CategoryDAO().GetCategoriesName(null),
                "Id", "Name", selectedId);
        }
    }
}
