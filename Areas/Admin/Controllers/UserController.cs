using BBEcom.Areas.Admin.Models;
using BBEcom.Common.Auth;
using Models.DAO;
using Models.EF;
using System;
using System.Web.Mvc;

namespace BBEcom.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        public ActionResult Index(
            string txtSearch, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(5, Math.Min(100, pageSize));

            ViewBag.Search = txtSearch;
            ViewBag.PageSize = pageSize;

            return View(new AccountDAO().GetListAccounts(
                txtSearch, page, pageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new UserAdminViewModel { Status = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserAdminViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(
                    "Password", "Vui lòng nhập mật khẩu.");

            var dao = new AccountDAO();

            if (dao.GetAccountByUsername(model.UserName) != null)
                ModelState.AddModelError(
                    "UserName", "Tên đăng nhập đã tồn tại.");

            if (dao.GetAccountByEmail(model.Email) != null)
                ModelState.AddModelError(
                    "Email", "Email đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var id = dao.InsertUser(new Account
            {
                UserName = model.UserName.Trim(),
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                Password = PasswordService.Hash(model.Password),
                IsAdmin = model.IsAdmin,
                Status = model.Status
            });

            SetAlert(
                id > 0 ? "Thêm người dùng thành công."
                    : "Không thể thêm người dùng.",
                id > 0 ? "success" : "error");

            return id > 0
                ? RedirectToAction("Index")
                : (ActionResult)View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var account = new AccountDAO().GetAccountById(id);
            if (account == null) return HttpNotFound();

            return View(new UserAdminViewModel
            {
                Id = account.Id,
                UserName = account.UserName,
                Name = account.Name,
                Email = account.Email,
                IsAdmin = account.IsAdmin,
                Status = account.Status
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserAdminViewModel model)
        {
            var dao = new AccountDAO();
            var existing = dao.GetAccountById(model.Id);
            if (existing == null) return HttpNotFound();

            var sameUser = dao.GetAccountByUsername(model.UserName);
            if (sameUser != null && sameUser.Id != model.Id)
                ModelState.AddModelError(
                    "UserName", "Tên đăng nhập đã tồn tại.");

            var sameEmail = dao.GetAccountByEmail(model.Email);
            if (sameEmail != null && sameEmail.Id != model.Id)
                ModelState.AddModelError(
                    "Email", "Email đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var result = dao.UpdateUser(new Account
            {
                Id = model.Id,
                UserName = model.UserName.Trim(),
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                IsAdmin = model.IsAdmin,
                Status = model.Status,
                Password = String.IsNullOrWhiteSpace(model.Password)
                    ? null : PasswordService.Hash(model.Password)
            });

            SetAlert(
                result ? "Cập nhật người dùng thành công."
                    : "Không thể cập nhật người dùng.",
                result ? "success" : "error");

            return result
                ? RedirectToAction("Index")
                : (ActionResult)View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (id == CurrentUser.UserId)
            {
                SetAlert(
                    "Không thể xóa tài khoản đang đăng nhập.", "error");
                return RedirectToAction("Index");
            }

            var result = new AccountDAO().DeleteUser(id);

            SetAlert(
                result ? "Xóa người dùng thành công."
                    : "Không thể xóa người dùng.",
                result ? "warning" : "error");

            return RedirectToAction("Index");
        }
    }
}
