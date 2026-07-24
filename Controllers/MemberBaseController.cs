using BBEcom.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

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

            var sess = (UserLogin)Session[CommonConstants.USER_SESSION];
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
