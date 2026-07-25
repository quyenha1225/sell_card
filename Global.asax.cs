using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BBEcom.Common;
using Models.DAO;

namespace BBEcom
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (Context.Session == null || Context.Session[CommonConstants.USER_SESSION] != null ||
                Context.User == null || Context.User.Identity == null || !Context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var account = new AccountDAO().GetAccountByUsername(Context.User.Identity.Name);
            if (account == null || !account.Status) return;
            Context.Session[CommonConstants.USER_SESSION] = new UserLogin
            {
                UserId = account.Id,
                UserName = account.UserName,
                Name = account.Name,
                IsAdmin = account.IsAdmin
            };
        }
    }
}
