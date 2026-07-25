using BBEcom.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Models.DAO;

namespace BBEcom.Controllers
{
    public class MemberBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var allowAnonymous = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true);

            if (allowAnonymous)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var sess = Session[CommonConstants.USER_SESSION] as UserLogin;
            if (sess == null && Request.IsAuthenticated && User != null && User.Identity != null)
            {
                var account = new AccountDAO().GetAccountByUsername(User.Identity.Name);
                if (account != null && account.Status)
                {
                    sess = new UserLogin
                    {
                        UserId = account.Id,
                        UserName = account.UserName,
                        Name = account.Name,
                        IsAdmin = account.IsAdmin
                    };
                    Session[CommonConstants.USER_SESSION] = sess;
                }
            }
            if (sess == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Index",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
