using BBEcom.Common;
using Models.DAO;
using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class RegisterController : Controller
    {
        // GET: Register
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
        public ActionResult Register(Account model, string returnUrl = null)
        {
            model.Status = true;
            model.IsAdmin = false;
            model.Password = Encryptor.MD5Hash(model.Password);
            if (ModelState.IsValid)
            {
                var dao = new AccountDAO();
                var checkExitsUser = dao.GetAccountByUsername(model.UserName);
                if (checkExitsUser == null)
                {
                    var res = dao.InsertUser(model);
                    if (res > 0)
                    {
                        TempData["LoginSuccess"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập để tiếp tục mua hàng.";
                        return RedirectToAction("Index", "Login", new { returnUrl = returnUrl });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể đăng ký tài khoản");
                    }
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập này đã tồn tại!");
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View("Index", model);
        }
    }
}
