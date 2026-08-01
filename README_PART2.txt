GIFTTOI PART 2 — JWT, OTP, GOOGLE OAUTH2, ADMIN
================================================

ÁP DỤNG SAU PART 1.

NỘI DUNG
--------
- JWT HMAC-SHA256 trong HttpOnly Cookie.
- Không dùng USER_SESSION cho trạng thái đăng nhập.
- RememberMe: cookie 30 ngày; không tích: cookie phiên, token 8 giờ.
- Xóa cookie Forms Authentication cũ.
- Mật khẩu PBKDF2; tài khoản MD5 cũ tự nâng cấp sau lần login đúng.
- Quên mật khẩu OTP email: 6 số, 10 phút, tối đa 5 lần sai.
- Google OAuth2 Authorization Code Flow + state chống CSRF.
- Register truyền thống giữ nguyên.
- Admin Product/User: ViewModel validation, POST + AntiForgeryToken, phân trang.
- Checkout Part 1 lấy tài khoản từ JWT claims.

CÁCH CÀI
--------
1. Sao lưu hoặc commit project.
2. Giải nén ZIP vào thư mục có BBEcom.csproj.
3. Chọn Replace files in destination.
4. PowerShell tại thư mục project:

   Set-ExecutionPolicy -Scope Process Bypass
   .\apply_part2.ps1

5. Chạy SQL:
   Data\PART2_AUTH_MIGRATION.sql

6. Sửa Web.config:
   - JwtSecret: chuỗi ngẫu nhiên >= 48 ký tự.
   - OtpHashSecret: chuỗi khác >= 32 ký tự.
   - SMTP Gmail/App Password.
   - GoogleClientId và GoogleClientSecret.

7. Reload Project -> Clean Solution -> Rebuild Solution.

GMAIL OTP
---------
- Bật xác minh hai bước.
- Tạo Google App Password.
- SmtpHost: smtp.gmail.com
- SmtpPort: 587
- SmtpEnableSsl: true
- SmtpPassword phải là App Password, không phải mật khẩu Gmail chính.

GOOGLE OAUTH2
-------------
Google Cloud Console > OAuth Client ID > Web application.

Redirect local:
https://localhost:44315/Login/GoogleCallback

Redirect host:
https://TEN-MIEN-CUA-BAN/Login/GoogleCallback

COOKIE
------
DevTools > Application > Cookies phải thấy:
GiftToi.AccessToken

Cookie phải có HttpOnly. Khi host thật phải dùng HTTPS và domain cố định.
localhost, domain host và trycloudflare URL khác nhau không dùng chung cookie.

BẢO MẬT
-------
SePay Secret Key từng nằm công khai trong Web.config trên GitHub.
Phải rotate/revoke key cũ trong trang SePay. Chỉ xóa khỏi file không đủ
vì key cũ vẫn có trong lịch sử Git.

Không commit:
- JwtSecret
- OtpHashSecret
- SmtpPassword
- GoogleClientSecret
- SePaySecretKey

KIỂM TRA
--------
1. Chạy SQL migration.
2. Đăng ký tài khoản.
3. Login và reload vẫn giữ.
4. Đóng/mở trình duyệt khi RememberMe được tích.
5. Quên mật khẩu nhận OTP.
6. Google login.
7. User thường không vào được /Admin.
8. Product Admin chặn PriceSale >= Price.
9. Delete Product/User gửi POST + AntiForgeryToken.
10. Phân trang giữ search và pageSize.
