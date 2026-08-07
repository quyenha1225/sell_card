using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace BBEcom.Common.Auth
{
    public class EmailService
    {
        public void SendPasswordResetOtp(
            string destination,
            string displayName,
            string otp)
        {
            var host = RequiredSetting(
                "SmtpHost",
                "SMTP host chưa được cấu hình."
            );

            var user = RequiredSetting(
                "SmtpUser",
                "SMTP user chưa được cấu hình."
            );

            var password = RequiredSetting(
                "SmtpPassword",
                "SMTP App Password chưa được cấu hình."
            );

            var from = RequiredSetting(
                "SmtpFrom",
                "SMTP from chưa được cấu hình."
            );

            if (String.IsNullOrWhiteSpace(destination))
            {
                throw new InvalidOperationException(
                    "Email nhận OTP không hợp lệ."
                );
            }

            int port;

            if (!Int32.TryParse(
                ConfigurationManager.AppSettings["SmtpPort"],
                out port))
            {
                port = 587;
            }

            bool enableSsl;

            if (!Boolean.TryParse(
                ConfigurationManager.AppSettings["SmtpEnableSsl"],
                out enableSsl))
            {
                enableSsl = true;
            }

            using (var message = new MailMessage())
            {
                message.From =
                    new MailAddress(from, "GiftToi");

                message.To.Add(destination);

                message.Subject =
                    "Mã OTP đặt lại mật khẩu GiftToi";

                message.IsBodyHtml = true;

                message.Body =
                    "<div style='font-family:Arial,sans-serif;max-width:560px;margin:auto'>"
                    + "<h2>Đặt lại mật khẩu GiftToi</h2>"
                    + "<p>Xin chào "
                    + WebUtility.HtmlEncode(
                        String.IsNullOrWhiteSpace(displayName)
                            ? "bạn"
                            : displayName
                    )
                    + ",</p>"
                    + "<p>Mã OTP của bạn là:</p>"
                    + "<div style='font-size:32px;font-weight:700;letter-spacing:8px;padding:18px;background:#eef9ff;text-align:center;border-radius:12px'>"
                    + WebUtility.HtmlEncode(otp)
                    + "</div>"
                    + "<p>Mã có hiệu lực trong 10 phút.</p>"
                    + "</div>";

                using (var client =
                    new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Timeout = 15000;
                    client.Credentials =
                        new NetworkCredential(
                            user,
                            password
                        );

                    client.Send(message);
                }
            }
        }

        private static string RequiredSetting(
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
