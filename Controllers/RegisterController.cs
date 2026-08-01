using BBEcom.Common.Auth;
using BBEcom.Models.ViewModels;
using Models.DAO;
using Models.EF;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class RegisterController : Controller
    {
        [HttpGet]
        public ActionResult Index(string returnUrl = null)
        {
            if (CurrentUser.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            RegisterViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View("Index", model);

            var dao = new AccountDAO();

            if (dao.GetAccountByUsername(model.UserName) != null)
                ModelState.AddModelError(
                    "UserName", "Tên đăng nhập đã tồn tại.");

            if (dao.GetAccountByEmail(model.Email) != null)
                ModelState.AddModelError(
                    "Email", "Email đã được sử dụng.");

            if (!ModelState.IsValid) return View("Index", model);

            dao.InsertUser(new Account
            {
                UserName = model.UserName.Trim(),
                Name = model.Name.Trim(),
                Email = model.Email.Trim(),
                Password = PasswordService.Hash(model.Password),
                Status = true,
                IsAdmin = false
            });

            TempData["LoginSuccess"] =
                "Đăng ký thành công. Vui lòng đăng nhập.";

            return RedirectToAction(
                "Index", "Login", new { returnUrl = returnUrl });
        }
    }
}
