using BBEcom.Areas.Admin.Models;
using BBEcom.Common.Auth;
using Models.DAO;
using System;
using System.Web.Mvc;

namespace BBEcom.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index(string returnUrl = null)
        {
            if (CurrentUser.IsAuthenticated && CurrentUser.IsAdmin)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel { RememberMe = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            LoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("Index", model);
            }

            var account = new AccountDAO().ValidateCredentials(
                model.UserName, model.Password);

            if (account == null || !account.Status || !account.IsAdmin)
            {
                ModelState.AddModelError(
                    "", "Tài khoản quản trị không hợp lệ.");
                ViewBag.ReturnUrl = returnUrl;
                return View("Index", model);
            }

            var token = JwtTokenService.CreateAccessToken(
                account.Id, account.UserName, account.Name,
                account.Email, true, model.RememberMe);

            AuthCookieService.WriteAccessCookie(
                Response, token, model.RememberMe,
                Request.IsSecureConnection);

            if (!String.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            AuthCookieService.ClearAccessCookie(
                Response, Request.IsSecureConnection);
            return RedirectToAction("Index");
        }
    }
}
