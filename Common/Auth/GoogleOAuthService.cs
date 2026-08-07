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
            string redirectUri,
            string state)
        {
            var clientId = GetRequiredSetting(
                "GoogleClientId",
                "GoogleClientId chưa được cấu hình."
            );

            if (String.IsNullOrWhiteSpace(redirectUri))
            {
                throw new InvalidOperationException(
                    "Google redirect URI không hợp lệ."
                );
            }

            if (String.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException(
                    "Google OAuth state không hợp lệ."
                );
            }

            return "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + HttpUtility.UrlEncode(clientId)
                + "&redirect_uri=" + HttpUtility.UrlEncode(redirectUri)
                + "&response_type=code"
                + "&scope=" + HttpUtility.UrlEncode("openid email profile")
                + "&state=" + HttpUtility.UrlEncode(state)
                + "&access_type=online"
                + "&prompt=select_account";
        }

        public async Task<GoogleUserInfo> ExchangeAsync(
            string code,
            string redirectUri)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException(
                    "Google không trả authorization code."
                );
            }

            var clientId = GetRequiredSetting(
                "GoogleClientId",
                "GoogleClientId chưa được cấu hình."
            );

            var clientSecret = GetRequiredSetting(
                "GoogleClientSecret",
                "GoogleClientSecret chưa được cấu hình."
            );

            using (var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            })
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
                        }
                    )
                );

                var tokenJson =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        "Google từ chối mã đăng nhập."
                    );
                }

                var token = JObject.Parse(tokenJson);
                var accessToken =
                    Convert.ToString(token["access_token"]);

                if (String.IsNullOrWhiteSpace(accessToken))
                {
                    throw new InvalidOperationException(
                        "Google không trả access token."
                    );
                }

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        accessToken
                    );

                var userResponse = await client.GetAsync(
                    "https://openidconnect.googleapis.com/v1/userinfo"
                );

                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        "Không lấy được thông tin tài khoản Google."
                    );
                }

                var userJson =
                    await userResponse.Content.ReadAsStringAsync();

                var user = JObject.Parse(userJson);

                return new GoogleUserInfo
                {
                    Subject = Convert.ToString(user["sub"]),
                    Email = Convert.ToString(user["email"]),
                    EmailVerified =
                        Convert.ToBoolean(
                            user["email_verified"] ?? false
                        ),
                    Name = Convert.ToString(user["name"])
                };
            }
        }

        private static string GetRequiredSetting(
            string key,
            string errorMessage)
        {
            var value =
                ConfigurationManager.AppSettings[key];

            if (String.IsNullOrWhiteSpace(value)
                || value.StartsWith(
                    "CHANGE_",
                    StringComparison.OrdinalIgnoreCase)
                || value.StartsWith(
                    "YOUR_",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    errorMessage
                );
            }

            return value.Trim();
        }
    }
}
