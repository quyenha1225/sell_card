using System;
using System.Security.Claims;
using System.Web;

namespace BBEcom.Common.Auth
{
    public static class CurrentUser
    {
        public static bool IsAuthenticated
        {
            get
            {
                return HttpContext.Current != null
                    && HttpContext.Current.User != null
                    && HttpContext.Current.User.Identity != null
                    && HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        public static int UserId
        {
            get
            {
                int id;
                return Int32.TryParse(
                    GetClaim(ClaimTypes.NameIdentifier), out id) ? id : 0;
            }
        }

        public static string UserName
        {
            get { return GetClaim(ClaimTypes.Name) ?? ""; }
        }

        public static string DisplayName
        {
            get
            {
                var value = GetClaim("display_name");
                return String.IsNullOrWhiteSpace(value) ? UserName : value;
            }
        }

        public static string Email
        {
            get { return GetClaim(ClaimTypes.Email) ?? ""; }
        }

        public static bool IsAdmin
        {
            get
            {
                return HttpContext.Current != null
                    && HttpContext.Current.User != null
                    && HttpContext.Current.User.IsInRole("Admin");
            }
        }

        public static UserLogin Get()
        {
            if (!IsAuthenticated) return null;

            return new UserLogin
            {
                UserId = UserId,
                UserName = UserName,
                Name = DisplayName,
                IsAdmin = IsAdmin
            };
        }

        private static string GetClaim(string type)
        {
            var principal = HttpContext.Current == null
                ? null
                : HttpContext.Current.User as ClaimsPrincipal;
            var claim = principal == null ? null : principal.FindFirst(type);
            return claim == null ? null : claim.Value;
        }
    }
}
