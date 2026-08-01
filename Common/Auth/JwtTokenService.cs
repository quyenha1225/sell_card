using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BBEcom.Common.Auth
{
    public static class JwtTokenService
    {
        public const string AccessCookieName = "GiftToi.AccessToken";
        public const string ResetCookieName = "GiftToi.ResetToken";
        private const string Issuer = "GiftToi";
        private const string Audience = "GiftToi.Web";

        public static string CreateAccessToken(
            int userId, string userName, string displayName,
            string email, bool isAdmin, bool rememberMe)
        {
            var expires = rememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddHours(8);

            return CreateToken(new Dictionary<string, object>
            {
                { "sub", userId.ToString() },
                { "name", userName ?? "" },
                { "display_name", displayName ?? "" },
                { "email", email ?? "" },
                { "role", isAdmin ? "Admin" : "Customer" },
                { "purpose", "access" },
                { "iss", Issuer },
                { "aud", Audience },
                { "iat", ToUnix(DateTime.UtcNow) },
                { "exp", ToUnix(expires) }
            });
        }

        public static string CreateResetToken(int userId, string email)
        {
            return CreateToken(new Dictionary<string, object>
            {
                { "sub", userId.ToString() },
                { "email", email ?? "" },
                { "purpose", "password_reset" },
                { "iss", Issuer },
                { "aud", Audience },
                { "iat", ToUnix(DateTime.UtcNow) },
                { "exp", ToUnix(DateTime.UtcNow.AddMinutes(10)) }
            });
        }

        public static ClaimsPrincipal ValidateAccessToken(string token)
        {
            JObject payload;
            if (!TryValidate(token, "access", out payload)) return null;

            int userId;
            if (!Int32.TryParse(Convert.ToString(payload["sub"]), out userId))
                return null;

            var claims = new List<Claim>
{
    // ID duy nhất của tài khoản
    new Claim(
        ClaimTypes.NameIdentifier,
        userId.ToString()
    ),

    // MVC AntiForgery với Claims Authentication đôi khi yêu cầu claim này
    new Claim(
        "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
        "GiftToi"
    ),

    new Claim(
        ClaimTypes.Name,
        Convert.ToString(payload["name"]) ?? ""
    ),

    new Claim(
        "display_name",
        Convert.ToString(payload["display_name"]) ?? ""
    ),

    new Claim(
        ClaimTypes.Email,
        Convert.ToString(payload["email"]) ?? ""
    ),

    new Claim(
        ClaimTypes.Role,
        Convert.ToString(payload["role"]) ?? "Customer"
    )
};

            var identity = new ClaimsIdentity(
                claims,
                "GiftToiJwt",
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            return new ClaimsPrincipal(identity);
        }

        public static bool TryValidateResetToken(
            string token, out int userId, out string email)
        {
            userId = 0;
            email = null;
            JObject payload;

            if (!TryValidate(token, "password_reset", out payload))
                return false;

            if (!Int32.TryParse(Convert.ToString(payload["sub"]), out userId))
                return false;

            email = Convert.ToString(payload["email"]);
            return userId > 0;
        }

        private static string CreateToken(IDictionary<string, object> payload)
        {
            var header = new Dictionary<string, object>
            {
                { "alg", "HS256" },
                { "typ", "JWT" }
            };

            var headerPart = Encode(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(header)));
            var payloadPart = Encode(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(payload)));
            var unsigned = headerPart + "." + payloadPart;

            return unsigned + "." + Encode(Sign(
                Encoding.UTF8.GetBytes(unsigned)));
        }

        private static bool TryValidate(
            string token, string purpose, out JObject payload)
        {
            payload = null;
            if (String.IsNullOrWhiteSpace(token)) return false;

            var parts = token.Split('.');
            if (parts.Length != 3) return false;

            byte[] provided;
            try { provided = Decode(parts[2]); }
            catch { return false; }

            var expected = Sign(Encoding.UTF8.GetBytes(parts[0] + "." + parts[1]));
            if (!FixedTimeEquals(expected, provided)) return false;

            try
            {
                payload = JObject.Parse(
                    Encoding.UTF8.GetString(Decode(parts[1])));
            }
            catch { return false; }

            if (Convert.ToString(payload["iss"]) != Issuer
                || Convert.ToString(payload["aud"]) != Audience
                || Convert.ToString(payload["purpose"]) != purpose)
                return false;

            long expiry;
            if (!Int64.TryParse(Convert.ToString(payload["exp"]), out expiry))
                return false;

            return expiry > ToUnix(DateTime.UtcNow);
        }

        private static byte[] Sign(byte[] data)
        {
            using (var hmac = new HMACSHA256(GetSecret()))
                return hmac.ComputeHash(data);
        }

        private static byte[] GetSecret()
        {
            var secret = ConfigurationManager.AppSettings["JwtSecret"];

            if (String.IsNullOrWhiteSpace(secret)
                || secret.Length < 32
                || secret.StartsWith("CHANGE_"))
                throw new InvalidOperationException(
                    "JwtSecret phải có ít nhất 32 ký tự.");

            return Encoding.UTF8.GetBytes(secret);
        }

        private static long ToUnix(DateTime utc)
        {
            return Convert.ToInt64(
                (utc.ToUniversalTime()
                - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds);
        }

        private static string Encode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Decode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            if (padded.Length % 4 == 2) padded += "==";
            else if (padded.Length % 4 == 3) padded += "=";
            return Convert.FromBase64String(padded);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;

            int difference = 0;
            for (int i = 0; i < left.Length; i++)
                difference |= left[i] ^ right[i];

            return difference == 0;
        }
    }
}
