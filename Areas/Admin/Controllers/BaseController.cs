using BBEcom.Common.Auth;
using System.Web.Mvc;
using System.Web.Routing;

namespace BBEcom.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Login",
                        action = "Index",
                        area = "Admin",
                        returnUrl =
                            filterContext.HttpContext.Request.RawUrl
                    }));
            }
            else if (!CurrentUser.IsAdmin)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index",
                        area = ""
                    }));
            }

            base.OnActionExecuting(filterContext);
        }

        protected void SetAlert(string message, string type)
        {
            TempData["AlertMsg"] = message;
            TempData["AlertType"] =
                type == "success" ? "alert-success"
                : type == "warning" ? "alert-warning"
                : "alert-danger";
        }
    }
}
