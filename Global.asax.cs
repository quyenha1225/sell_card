using BBEcom.Common.Auth;
using System;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BBEcom
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Database đã tồn tại và được cập nhật bằng SQL.
            // Không cho Entity Framework kiểm tra hoặc tự tạo lại database.
            System.Data.Entity.Database.SetInitializer<
                global::Models.EF.KeyDbContext
            >(
                new System.Data.Entity.NullDatabaseInitializer<
                    global::Models.EF.KeyDbContext
                >()
            );

            // Dùng AccountId trong JWT làm định danh duy nhất
            // khi ASP.NET MVC tạo Anti-Forgery Token.
            AntiForgeryConfig.UniqueClaimTypeIdentifier =
                ClaimTypes.NameIdentifier;

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(
                GlobalFilters.Filters
            );

            RouteConfig.RegisterRoutes(
                RouteTable.Routes
            );

            BundleConfig.RegisterBundles(
                BundleTable.Bundles
            );
        }

        protected void Application_AuthenticateRequest(
            object sender,
            EventArgs e)
        {
            HttpCookie cookie = Request.Cookies[
                JwtTokenService.AccessCookieName
            ];

            if (cookie == null ||
                String.IsNullOrWhiteSpace(cookie.Value))
            {
                return;
            }

            ClaimsPrincipal principal =
                JwtTokenService.ValidateAccessToken(
                    cookie.Value
                );

            if (principal == null)
            {
                return;
            }

            Context.User = principal;
            Thread.CurrentPrincipal = principal;
        }
    }
}