using BBEcom.Common.Auth;
using System;
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
                var request = filterContext.HttpContext.Request;

                // AJAX không nên bị 302 sang trang Login rồi trả HTML về cho $.ajax.
                // Trả 401 để JavaScript chủ động chuyển sang trang đăng nhập.
                if (request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(401);
                    return;
                }

                // GET có thể quay lại đúng URL hiện tại.
                // POST (AddItem/BuyNow/...) không được dùng RawUrl làm returnUrl,
                // vì sau OAuth trình duyệt sẽ GET vào action chỉ nhận POST và gây 404.
                string returnUrl = null;

                if (String.Equals(
                    request.HttpMethod,
                    "GET",
                    StringComparison.OrdinalIgnoreCase))
                {
                    returnUrl = request.RawUrl;
                }
                else if (request.UrlReferrer != null)
                {
                    returnUrl = request.UrlReferrer.PathAndQuery;
                }

                if (String.IsNullOrWhiteSpace(returnUrl)
                    || !Url.IsLocalUrl(returnUrl))
                {
                    returnUrl = Url.Action("Index", "Home");
                }

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Login",
                        action = "Index",
                        returnUrl = returnUrl
                    }));

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
