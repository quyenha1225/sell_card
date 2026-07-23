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
    public class ProfileController : MemberBaseController
    {
        // GET: Profile
        public ActionResult Index()
        {
            var member = (UserLogin)Session[BBEcom.Common.CommonConstants.USER_SESSION];
            if (member == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var data = new AccountDAO().GetAccountById(member.UserId);
                return View(data);
            }
        }

        public ActionResult ProfileMenu()
        {
            return PartialView();
        }

        public ActionResult OrderHistory()
        {
            var user = (UserLogin)Session[BBEcom.Common.CommonConstants.USER_SESSION];
            if (user != null)
            {
                var dataOrder = new OrderDAO().GetListOrderByCustomerID(user.UserId);
                return View(dataOrder);
            }
            return View();
        }
    }
}