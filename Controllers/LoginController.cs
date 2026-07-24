using BBEcom.Areas.Admin.Models;
using BBEcom.Common;
using Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index(string returnUrl = null)
        {
            var sess = (UserLogin)Session[CommonConstants.USER_SESSION];
            if (sess != null) //had login
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {

                var dao = new AccountDAO();
                var res = dao.Login(model.UserName, Encryptor.MD5Hash(model.Password));
                if (res == 1)
                {
                    var user = dao.GetAccountByUsername(model.UserName);
                    var userSession = new UserLogin();
                    userSession.UserName = user.UserName;
                    userSession.UserId = user.Id;
                    userSession.Name = user.Name;
                    userSession.IsAdmin = user.IsAdmin;
                    Session.Add(CommonConstants.USER_SESSION, userSession);
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (res == -1)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị khoá");

                }
                else
                {
                    ModelState.AddModelError("", "Thông tin đăng nhập không hợp lệ");

                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View("Index", model);
        }

        public ActionResult Logout()
        {
            Session.Remove(CommonConstants.USER_SESSION);
            return RedirectToAction("Index", "Login");
        }
    }
}
