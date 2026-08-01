using BBEcom.Common.Auth;
using System.Web.Mvc;
using System.Web.Routing;

namespace BBEcom.Controllers
{
    public class MemberBaseController : Controller
    {
        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            var allowAnonymous =
                filterContext.ActionDescriptor.IsDefined(
                    typeof(AllowAnonymousAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor
                    .IsDefined(typeof(AllowAnonymousAttribute), true);

            if (!allowAnonymous && !CurrentUser.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response
                        .SuppressFormsAuthenticationRedirect = true;

                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            status = false,
                            requiresLogin = true,
                            loginUrl = Url.Action(
                                "Index",
                                "Login",
                                new
                                {
                                    returnUrl =
                                        filterContext.HttpContext
                                            .Request.RawUrl
                                }
                            ),
                            message =
                                "Vui lòng đăng nhập để tiếp tục."
                        },
                        JsonRequestBehavior =
                            JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new
                        {
                            controller = "Login",
                            action = "Index",
                            returnUrl =
                                filterContext.HttpContext.Request.RawUrl
                        }));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
