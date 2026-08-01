using System;
using System.Web;

namespace BBEcom.Common.Auth
{
    public static class AuthCookieService
    {
        public static void WriteAccessCookie(
            HttpResponseBase response, string token,
            bool rememberMe, bool secure)
        {
            var cookie = new HttpCookie(
                JwtTokenService.AccessCookieName, token)
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };

            if (rememberMe)
                cookie.Expires = DateTime.UtcNow.AddDays(30);

            response.Cookies.Add(cookie);
        }

        public static void WriteResetCookie(
            HttpResponseBase response, string token, bool secure)
        {
            response.Cookies.Add(new HttpCookie(
                JwtTokenService.ResetCookieName, token)
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(10)
            });
        }

        public static void ClearAccessCookie(
            HttpResponseBase response, bool secure)
        {
            Clear(response, JwtTokenService.AccessCookieName, secure);
            Clear(response, ".ASPXAUTH", secure);
            Clear(response, ".GiftToiAuth", secure);
        }

        public static void ClearResetCookie(
            HttpResponseBase response, bool secure)
        {
            Clear(response, JwtTokenService.ResetCookieName, secure);
        }

        private static void Clear(
            HttpResponseBase response, string name, bool secure)
        {
            response.Cookies.Add(new HttpCookie(name, "")
            {
                HttpOnly = true,
                Secure = secure,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddYears(-1)
            });
        }
    }
}
