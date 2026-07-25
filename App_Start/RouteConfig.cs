using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BBEcom
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
              name: "AboutUs",
              url: "gioi-thieu-ve-chung-toi",
              defaults: new { controller = "Page", action = "GioiThieu", id = UrlParameter.Optional },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
              name: "NewsDetail",
              url: "tin-tuc/{alias}-{id}",
              defaults: new { controller = "News", action = "View", id = UrlParameter.Optional },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
              name: "NewsIndex",
              url: "tin-tuc",
              defaults: new { controller = "News", action = "Index" },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
              name: "Login",
              url: "dang-nhap",
              defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
              name: "Register",
              url: "dang-ky",
              defaults: new { controller = "Register", action = "Index", id = UrlParameter.Optional },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
               name: "Profile",
               url: "trang-ca-nhan",
               defaults: new { controller = "Profile", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
              name: "OrderHistory",
              url: "lich-su-mua-hang",
              defaults: new { controller = "Profile", action = "OrderHistory", id = UrlParameter.Optional },
              namespaces: new[] { "BBEcom.Controllers" }
          );

            routes.MapRoute(
               name: "SePayIpn",
               url: "thanh-toan/sepay/ipn",
               defaults: new { controller = "Cart", action = "SePayIpn" },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "SePaySuccess",
               url: "thanh-toan/sepay/thanh-cong",
               defaults: new { controller = "Cart", action = "SePaySuccess" },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "SePayError",
               url: "thanh-toan/sepay/loi",
               defaults: new { controller = "Cart", action = "SePayError" },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "SePayCancel",
               url: "thanh-toan/sepay/huy",
               defaults: new { controller = "Cart", action = "SePayCancel" },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "BuyNow",
               url: "mua-ngay",
               defaults: new { controller = "Cart", action = "BuyNow" },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "Payment",
               url: "thanh-toan",
               defaults: new { controller = "Cart", action = "Payment", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "CartAddItem",
               url: "gio-hang/them-{productID}-{quantity}",
               defaults: new { controller = "Cart", action = "AddItem", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "Cart",
               url: "gio-hang",
               defaults: new { controller = "Cart", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "ProductDetail",
               url: "san-pham/{alias}-{id}",
               defaults: new { controller = "Product", action = "Detail", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
                name: "ProductCategory",
                url: "danh-muc/{alias}-{id}",
                defaults: new { controller = "Product", action = "Category", id = UrlParameter.Optional },
                namespaces: new[] { "BBEcom.Controllers" }
            );

            routes.MapRoute(
               name: "404",
               url: "page-404",
               defaults: new { controller = "Error", action = "PageNotFound", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );

            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "BBEcom.Controllers" }
           );
        }
    }
}
