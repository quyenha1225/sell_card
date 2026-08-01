using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace BBEcom.Common.Auth
{
    public class GoogleUserInfo
    {
        public string Subject { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Name { get; set; }
    }

    public class GoogleOAuthService
    {
        public string BuildAuthorizationUrl(
            string redirectUri, string state)
        {
            var clientId =
                ConfigurationManager.AppSettings["GoogleClientId"];

            if (String.IsNullOrWhiteSpace(clientId)
                || clientId.StartsWith("CHANGE_"))
                throw new InvalidOperationException(
                    "GoogleClientId chưa được cấu hình.");

            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + HttpUtility.UrlEncode(clientId)
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri)
                + "&response_type=code"
                + "&scope=" + HttpUtility.UrlEncode("openid email profile")
                + "&state=" + HttpUtility.UrlEncode(state)
                + "&access_type=online&prompt=select_account";
        }

        public async Task<GoogleUserInfo> ExchangeAsync(
            string code, string redirectUri)
        {
            var clientId =
                ConfigurationManager.AppSettings["GoogleClientId"];
            var clientSecret =
                ConfigurationManager.AppSettings["GoogleClientSecret"];

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            { "code", code },
                            { "client_id", clientId },
                            { "client_secret", clientSecret },
                            { "redirect_uri", redirectUri },
                            { "grant_type", "authorization_code" }
                        }));

                var tokenJson = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(
                        "Google từ chối mã đăng nhập.");

                var token = JObject.Parse(tokenJson);
                var accessToken =
                    Convert.ToString(token["access_token"]);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var userJson = await client.GetStringAsync(
                    "https://openidconnect.googleapis.com/v1/userinfo");
                var user = JObject.Parse(userJson);

                return new GoogleUserInfo
                {
                    Subject = Convert.ToString(user["sub"]),
                    Email = Convert.ToString(user["email"]),
                    EmailVerified =
                        Convert.ToBoolean(user["email_verified"] ?? false),
                    Name = Convert.ToString(user["name"])
                };
            }
        }
    }
}
