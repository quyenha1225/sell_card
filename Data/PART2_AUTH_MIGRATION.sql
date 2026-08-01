IF OBJECT_ID(N'dbo.PasswordResetOtp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetOtp
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AccountId INT NOT NULL,
        CodeHash NVARCHAR(128) NOT NULL,
        ExpiresAtUtc DATETIME2 NOT NULL,
        FailedAttempts INT NOT NULL DEFAULT 0,
        UsedAtUtc DATETIME2 NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PasswordResetOtp_Account
            FOREIGN KEY (AccountId) REFERENCES dbo.Account(Id)
            ON DELETE CASCADE
    );

    CREATE INDEX IX_PasswordResetOtp_Account_Active
        ON dbo.PasswordResetOtp(AccountId, UsedAtUtc, ExpiresAtUtc);
END;
GO

SELECT Email, COUNT(*) AS DuplicateCount
FROM dbo.Account
WHERE Email IS NOT NULL AND LTRIM(RTRIM(Email)) <> ''
GROUP BY Email
HAVING COUNT(*) > 1;
GO
