/* OPTIONAL - CHỈ DÙNG LOCAL/DEMO.
   admin / Admin@123
   user  / User@123
   Chỉ reset password + mở khóa 2 account demo, không xóa dữ liệu khác. */
USE [BBEcom];
GO
UPDATE dbo.Account
SET Password = '0e7517141fb53f21ee439b355b5a1d0a', Status = 1
WHERE UserName = 'admin';
UPDATE dbo.Account
SET Password = '448ddd517d3abb70045aea6929f02367', Status = 1
WHERE UserName = 'user';
GO
SELECT Id, UserName, IsAdmin, Status, LEN(Password) AS PasswordHashLength
FROM dbo.Account WHERE UserName IN ('admin','user');
GO
