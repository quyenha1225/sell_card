using BBEcom.Common.Auth;
using System;
using System.Web;
using System.Web.Mvc;

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
                var request = filterContext.HttpContext.Request;
                var returnUrl = ResolveReturnUrl(request);

                var loginUrl = Url.Action(
                    "Index",
                    "Login",
                    new { returnUrl = returnUrl }
                );

                if (request.IsAjaxRequest())
                {
                    var response = filterContext.HttpContext.Response;

                    response.StatusCode = 401;
                    response.TrySkipIisCustomErrors = true;
                    response.SuppressFormsAuthenticationRedirect = true;

                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            status = false,
                            authRequired = true,
                            loginUrl = loginUrl,
                            message = "Phiên đăng nhập đã hết hạn hoặc bạn chưa đăng nhập."
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };

                    return;
                }

                filterContext.Result = new RedirectResult(loginUrl);
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private string ResolveReturnUrl(HttpRequestBase request)
        {
            string returnUrl = null;

            if (request != null
                && String.Equals(
                    request.HttpMethod,
                    "GET",
                    StringComparison.OrdinalIgnoreCase))
            {
                returnUrl = request.RawUrl;
            }
            else if (request != null && request.UrlReferrer != null)
            {
                // POST chỉ quay lại trang nguồn GET.
                // Không bao giờ redirect browser bằng GET vào /Cart/AddItem.
                returnUrl = request.UrlReferrer.PathAndQuery;
            }

            if (String.IsNullOrWhiteSpace(returnUrl)
                || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }

            return returnUrl;
        }
    }
}
