using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace BBEcom.Common.Auth
{
    public class EmailService
    {
        public void SendPasswordResetOtp(
            string destination, string displayName, string otp)
        {
            string host = ConfigurationManager.AppSettings["SmtpHost"];
            string user = ConfigurationManager.AppSettings["SmtpUser"];
            string password = ConfigurationManager.AppSettings["SmtpPassword"];
            string from = ConfigurationManager.AppSettings["SmtpFrom"];

            int port;
            if (!Int32.TryParse(
                ConfigurationManager.AppSettings["SmtpPort"], out port))
                port = 587;

            bool enableSsl;
            if (!Boolean.TryParse(
                ConfigurationManager.AppSettings["SmtpEnableSsl"],
                out enableSsl))
                enableSsl = true;

            if (String.IsNullOrWhiteSpace(host)
                || String.IsNullOrWhiteSpace(from)
                || host.StartsWith("CHANGE_"))
                throw new InvalidOperationException(
                    "SMTP chưa được cấu hình trong Web.config.");

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(from, "GiftToi");
                message.To.Add(destination);
                message.Subject = "Mã OTP đặt lại mật khẩu GiftToi";
                message.IsBodyHtml = true;
                message.Body =
                    "<div style='font-family:Arial,sans-serif;max-width:560px;margin:auto'>"
                    + "<h2>Đặt lại mật khẩu GiftToi</h2>"
                    + "<p>Xin chào "
                    + WebUtility.HtmlEncode(
                        String.IsNullOrWhiteSpace(displayName)
                            ? "bạn" : displayName)
                    + ",</p><p>Mã OTP của bạn là:</p>"
                    + "<div style='font-size:32px;font-weight:700;letter-spacing:8px;padding:18px;background:#eef9ff;text-align:center;border-radius:12px'>"
                    + WebUtility.HtmlEncode(otp)
                    + "</div><p>Mã có hiệu lực trong 10 phút.</p></div>";

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    if (!String.IsNullOrWhiteSpace(user))
                        client.Credentials =
                            new NetworkCredential(user, password);

                    client.Send(message);
                }
            }
        }
    }
}
