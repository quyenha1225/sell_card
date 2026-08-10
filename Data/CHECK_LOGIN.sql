USE [BBEcom];
GO
SELECT Id, UserName, Email, IsAdmin, Status,
       LEN(Password) AS PasswordHashLength,
       CASE WHEN Password IS NULL THEN N'NULL PASSWORD'
            WHEN LEN(Password) = 32 THEN N'Legacy MD5 - project hỗ trợ'
            ELSE N'Modern hash - Crypto.VerifyHashedPassword' END AS PasswordFormat
FROM dbo.Account
ORDER BY Id;
GO
