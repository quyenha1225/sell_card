using BBEcom.Areas.Admin.Models;
using BBEcom.Common.Auth;
using BBEcom.Models.ViewModels;
using Models.DAO;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index(string returnUrl = null)
        {
            var safeReturnUrl = NormalizeReturnUrl(returnUrl);

            // Nếu JWT hiện tại đã hợp lệ thì không hiển thị lại form đăng nhập.
            // Điều này tránh việc form Anti-Forgery cũ của người dùng anonymous
            // bị submit sau khi Google OAuth đã đổi sang ClaimsPrincipal mới.
            if (CurrentUser.IsAuthenticated)
            {
                if (!String.IsNullOrWhiteSpace(safeReturnUrl))
                    return Redirect(safeReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = safeReturnUrl;
            return View(new LoginModel { RememberMe = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            LoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View("Index", model);
            }

            var account = new AccountDAO().ValidateCredentials(
                model.UserName, model.Password);

            if (account == null)
            {
                ModelState.AddModelError(
                    "", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                ViewBag.ReturnUrl = returnUrl;
                return View("Index", model);
            }

            if (!account.Status)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa.");
                ViewBag.ReturnUrl = returnUrl;
                return View("Index", model);
            }

            WriteLogin(account, model.RememberMe);

            // Token chống CSRF được tạo trước khi đăng nhập thuộc anonymous user.
            // Sau khi principal đổi sang JWT user phải bỏ token cũ để trang kế tiếp
            // tự tạo cặp Anti-Forgery mới cho đúng AccountId.
            ClearAntiForgeryCookies();

            var safeReturnUrl = NormalizeReturnUrl(returnUrl);
            if (!String.IsNullOrWhiteSpace(safeReturnUrl))
                return Redirect(safeReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = new AccountDAO().GetAccountByEmail(model.Email);

            if (account != null && account.Status)
            {
                var otp = GenerateOtp();
                var hash = HashOtp(account.Id, otp);

                new PasswordResetOtpDAO().Create(account.Id, hash);

                try
                {
                    new EmailService().SendPasswordResetOtp(
                        account.Email, account.Name, otp);
                }
                catch
                {
                    ModelState.AddModelError(
                        "", "Không thể gửi email. Kiểm tra cấu hình SMTP.");
                    return View(model);
                }
            }

            TempData["OtpSent"] =
                "Nếu email tồn tại, mã OTP đã được gửi.";

            return RedirectToAction(
                "VerifyOtp", new { email = model.Email });
        }

        [HttpGet]
        public ActionResult VerifyOtp(string email)
        {
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var account = new AccountDAO().GetAccountByEmail(model.Email);

            if (account == null)
            {
                ModelState.AddModelError(
                    "", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            var dao = new PasswordResetOtpDAO();
            var active = dao.GetLatestActive(account.Id);

            if (active == null
                || !String.Equals(
                    active.CodeHash,
                    HashOtp(account.Id, model.Otp),
                    StringComparison.Ordinal))
            {
                if (active != null) dao.IncreaseFailure(active.Id);

                ModelState.AddModelError(
                    "", "Mã OTP không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            dao.MarkUsed(active.Id);

            var token = JwtTokenService.CreateResetToken(
                account.Id, account.Email);

            AuthCookieService.WriteResetCookie(
                Response, token, IsSecureRequest());

            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            int accountId;
            string email;

            if (!TryReadResetToken(out accountId, out email))
            {
                TempData["LoginError"] =
                    "Phiên đặt lại mật khẩu đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(
            ResetPasswordViewModel model)
        {
            int accountId;
            string email;

            if (!TryReadResetToken(out accountId, out email))
            {
                TempData["LoginError"] =
                    "Phiên đặt lại mật khẩu đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            if (!ModelState.IsValid) return View(model);

            var result = new AccountDAO().UpdatePassword(
                accountId, PasswordService.Hash(model.Password));

            if (!result)
            {
                ModelState.AddModelError(
                    "", "Không thể cập nhật mật khẩu.");
                return View(model);
            }

            AuthCookieService.ClearResetCookie(
                Response, IsSecureRequest());

            TempData["PasswordResetSuccess"] =
                "Đặt lại mật khẩu thành công.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Google(string returnUrl = null)
        {
            returnUrl = NormalizeReturnUrl(returnUrl);
            var state = Convert.ToBase64String(RandomBytes(32));

            Response.Cookies.Add(new HttpCookie(
                "GiftToi.GoogleState", state)
            {
                HttpOnly = true,
                Secure = IsSecureRequest(),
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(10)
            });

            if (!String.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                Response.Cookies.Add(new HttpCookie(
                    "GiftToi.ReturnUrl", returnUrl)
                {
                    HttpOnly = true,
                    Secure = IsSecureRequest(),
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddMinutes(10)
                });
            }

            var redirectUri = BuildGoogleRedirectUri();

            return Redirect(
                new GoogleOAuthService().BuildAuthorizationUrl(
                    redirectUri, state));
        }

        [HttpGet]
        public async Task<ActionResult> GoogleCallback(
            string code, string state, string error)
        {
            if (!String.IsNullOrWhiteSpace(error))
            {
                TempData["LoginError"] = "Đăng nhập Google đã bị hủy.";
                return RedirectToAction("Index");
            }

            var stateCookie = Request.Cookies["GiftToi.GoogleState"];
            var expectedState = stateCookie == null
                ? null : stateCookie.Value;

            if (String.IsNullOrWhiteSpace(state)
                || !String.Equals(
                    state, expectedState, StringComparison.Ordinal))
            {
                TempData["LoginError"] =
                    "Trạng thái OAuth2 không hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                var redirectUri = BuildGoogleRedirectUri();

                var googleUser = await new GoogleOAuthService()
                    .ExchangeAsync(code, redirectUri);

                if (googleUser == null
                    || !googleUser.EmailVerified
                    || String.IsNullOrWhiteSpace(googleUser.Email))
                    throw new InvalidOperationException();

                var account = new AccountDAO()
                    .GetOrCreateGoogleAccount(
                        googleUser.Email, googleUser.Name);

                if (!account.Status)
                {
                    TempData["LoginError"] = "Tài khoản đã bị khóa.";
                    return RedirectToAction("Index");
                }

                WriteLogin(account, true);

                var returnCookie = Request.Cookies["GiftToi.ReturnUrl"];
                var returnUrl = returnCookie == null
                    ? null : returnCookie.Value;

                var safeReturnUrl = NormalizeReturnUrl(returnUrl);

                // Xóa token CSRF của phiên anonymous cũ và cookie OAuth tạm.
                ClearAntiForgeryCookies();
                ClearTemporaryOAuthCookies();

                if (!String.IsNullOrWhiteSpace(safeReturnUrl))
                    return Redirect(safeReturnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                TempData["LoginError"] =
                    "Không thể đăng nhập Google. Kiểm tra cấu hình OAuth2.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            AuthCookieService.ClearAccessCookie(
                Response, IsSecureRequest());

            ClearAntiForgeryCookies();
            ClearTemporaryOAuthCookies();

            return RedirectToAction("Index");
        }

        private bool IsSecureRequest()
        {
            var forwardedProto =
                Request.Headers["X-Forwarded-Proto"];

            if (!String.IsNullOrWhiteSpace(forwardedProto))
            {
                var value = forwardedProto
                    .Split(',')[0]
                    .Trim();

                if (String.Equals(
                    value,
                    "https",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (String.Equals(
                    value,
                    "http",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return Request.IsSecureConnection;
        }

        private string BuildGoogleRedirectUri()
        {
            var forwardedHost =
                Request.Headers["X-Forwarded-Host"];

            var host =
                !String.IsNullOrWhiteSpace(forwardedHost)
                    ? forwardedHost
                        .Split(',')[0]
                        .Trim()
                    : Request.Url.Authority;

            var configured =
                ConfigurationManager.AppSettings[
                    "GoogleRedirectUri"
                ];

            // Localhost có thể dùng URI cố định trong Web.config.
            // Khi đi qua Cloudflare Tunnel, ưu tiên host hiện tại
            // để tránh callback quay về localhost hoặc tunnel cũ.
            var isLocalHost =
                host.StartsWith(
                    "localhost",
                    StringComparison.OrdinalIgnoreCase)
                || host.StartsWith(
                    "127.0.0.1",
                    StringComparison.OrdinalIgnoreCase);

            Uri configuredUri;

            if (isLocalHost
                && !String.IsNullOrWhiteSpace(configured)
                && Uri.TryCreate(
                    configured,
                    UriKind.Absolute,
                    out configuredUri))
            {
                return configuredUri.ToString();
            }

            var scheme =
                IsSecureRequest()
                    ? "https"
                    : "http";

            if (host.EndsWith(
                ".trycloudflare.com",
                StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            return scheme
                + "://"
                + host
                + Url.Action(
                    "GoogleCallback",
                    "Login"
                );
        }

        private string NormalizeReturnUrl(string returnUrl)
        {
            if (String.IsNullOrWhiteSpace(returnUrl)
                || !Url.IsLocalUrl(returnUrl))
            {
                return null;
            }

            var path = returnUrl;
            var queryIndex = path.IndexOf('?');
            if (queryIndex >= 0)
                path = path.Substring(0, queryIndex);

            var hashIndex = path.IndexOf('#');
            if (hashIndex >= 0)
                path = path.Substring(0, hashIndex);

            path = path.TrimEnd('/').ToLowerInvariant();

            // Không redirect browser bằng GET vào các action chỉ nhận POST.
            // Đây chính là nguyên nhân của lỗi 404 /Cart/AddItem sau Google OAuth.
            if (path == "/cart/additem"
                || path == "/cart/buynow"
                || path == "/mua-ngay"
                || path.StartsWith("/gio-hang/them-")
                || path == "/login/login")
            {
                return null;
            }

            return returnUrl;
        }

        private void ClearAntiForgeryCookies()
        {
            var keys = Request.Cookies.AllKeys;
            if (keys == null) return;

            foreach (var key in keys)
            {
                if (String.IsNullOrWhiteSpace(key)
                    || !key.StartsWith(
                        "__RequestVerificationToken",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                ExpireCookie(key);
            }
        }

        private void ClearTemporaryOAuthCookies()
        {
            ExpireCookie("GiftToi.GoogleState");
            ExpireCookie("GiftToi.ReturnUrl");
        }

        private void ExpireCookie(string name)
        {
            Response.Cookies.Add(new HttpCookie(name, "")
            {
                HttpOnly = true,
                Secure = IsSecureRequest(),
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTime.UtcNow.AddYears(-1)
            });
        }

        private void WriteLogin(
    global::Models.EF.Account account,
    bool rememberMe)
        {
            var token = JwtTokenService.CreateAccessToken(
                account.Id,
                account.UserName,
                account.Name,
                account.Email,
                account.IsAdmin,
                rememberMe
            );

            AuthCookieService.WriteAccessCookie(
                Response,
                token,
                rememberMe,
                IsSecureRequest()
            );
        }
        private bool TryReadResetToken(
    out int accountId,
    out string email)
        {
            accountId = 0;
            email = null;

            var cookie = Request.Cookies[
                JwtTokenService.ResetCookieName
            ];

            if (cookie == null
                || String.IsNullOrWhiteSpace(cookie.Value))
            {
                return false;
            }

            return JwtTokenService.TryValidateResetToken(
                cookie.Value,
                out accountId,
                out email
            );
        }
        private static string GenerateOtp()
        {
            var bytes = RandomBytes(4);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (value % 1000000).ToString("D6");
        }

        private static string HashOtp(int accountId, string otp)
        {
            var secret =
                ConfigurationManager.AppSettings["OtpHashSecret"];

            if (String.IsNullOrWhiteSpace(secret)
                || secret.Length < 24
                || secret.StartsWith("CHANGE_"))
                throw new InvalidOperationException(
                    "OtpHashSecret chưa được cấu hình.");

            using (var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(secret)))
            {
                return Convert.ToBase64String(
                    hmac.ComputeHash(
                        Encoding.UTF8.GetBytes(accountId + ":" + otp)));
            }
        }

        private static byte[] RandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(bytes);
            return bytes;
        }
    }
}
