/*
    GiftToi - HARDENING PATCH
    Chạy 1 lần trên database BBEcom trước khi chạy code đã vá.
    Không DROP TABLE, không xóa đơn hàng/tài khoản/sản phẩm.
*/

USE [BBEcom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================
   1. OTP TABLE - idempotent
   ============================================================ */
IF OBJECT_ID(N'dbo.PasswordResetOtp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetOtp
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AccountId INT NOT NULL,
        CodeHash NVARCHAR(128) NOT NULL,
        ExpiresAtUtc DATETIME2 NOT NULL,
        FailedAttempts INT NOT NULL
            CONSTRAINT DF_PasswordResetOtp_FailedAttempts DEFAULT (0),
        UsedAtUtc DATETIME2 NULL,
        CreatedAtUtc DATETIME2 NOT NULL
            CONSTRAINT DF_PasswordResetOtp_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_PasswordResetOtp_Account
            FOREIGN KEY (AccountId)
            REFERENCES dbo.Account(Id)
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PasswordResetOtp_Account_Active'
      AND object_id = OBJECT_ID(N'dbo.PasswordResetOtp')
)
BEGIN
    CREATE INDEX IX_PasswordResetOtp_Account_Active
        ON dbo.PasswordResetOtp(AccountId, UsedAtUtc, ExpiresAtUtc);
END;
GO

/* ============================================================
   2. PRODUCT COST PRICE
   CostPrice = giá nhập / giá vốn
   Price     = giá bán niêm yết
   PriceSale = giá khuyến mãi
   ============================================================ */
IF COL_LENGTH('dbo.Product', 'CostPrice') IS NULL
BEGIN
    ALTER TABLE dbo.Product
    ADD CostPrice INT NOT NULL
        CONSTRAINT DF_Product_CostPrice DEFAULT (0);
END;
GO

/* Backfill dữ liệu cũ để code EF không gặp CostPrice = 0.
   Demo: giá vốn = 80% giá thực tế đang bán. */
UPDATE dbo.Product
SET CostPrice =
    CASE
        WHEN PriceSale IS NOT NULL
             AND PriceSale > 0
             AND PriceSale < Price
            THEN
                CASE
                    WHEN CAST(PriceSale * 0.80 AS INT) < 1
                        THEN 1
                    ELSE CAST(PriceSale * 0.80 AS INT)
                END
        WHEN Price > 0
            THEN
                CASE
                    WHEN CAST(Price * 0.80 AS INT) < 1
                        THEN 1
                    ELSE CAST(Price * 0.80 AS INT)
                END
        ELSE 1
    END
WHERE CostPrice <= 0
   OR CostPrice >= Price;
GO

/* Promotion cũ sai logic thì bỏ promotion, giữ nguyên giá niêm yết. */
UPDATE dbo.Product
SET PriceSale = NULL
WHERE PriceSale IS NOT NULL
  AND
  (
      PriceSale <= 0
      OR PriceSale >= Price
      OR PriceSale <= CostPrice
  );
GO

/* Constraint đảm bảo không tạo sản phẩm bán lỗ do nhập sai giá. */
IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_CostPrice_Positive'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
BEGIN
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_CostPrice_Positive;
END;
GO

ALTER TABLE dbo.Product WITH CHECK
ADD CONSTRAINT CK_Product_CostPrice_Positive
CHECK (CostPrice > 0);
GO

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_Price_Above_Cost'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
BEGIN
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_Price_Above_Cost;
END;
GO

ALTER TABLE dbo.Product WITH CHECK
ADD CONSTRAINT CK_Product_Price_Above_Cost
CHECK (Price > CostPrice);
GO

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_PriceSale_Range'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
BEGIN
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_PriceSale_Range;
END;
GO

ALTER TABLE dbo.Product WITH CHECK
ADD CONSTRAINT CK_Product_PriceSale_Range
CHECK
(
    PriceSale IS NULL
    OR
    (
        PriceSale > CostPrice
        AND PriceSale < Price
    )
);
GO

/* ============================================================
   3. INDEXES CHO LUỒNG MUA HÀNG / TỒN KHO
   ============================================================ */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Giftcard_ProductId'
      AND object_id = OBJECT_ID(N'dbo.Giftcard')
)
BEGIN
    CREATE INDEX IX_Giftcard_ProductId
        ON dbo.Giftcard(ProductId, Id);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Order_OrderCode'
      AND object_id = OBJECT_ID(N'dbo.[Order]')
)
BEGIN
    CREATE INDEX IX_Order_OrderCode
        ON dbo.[Order](OrderCode);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Order_Customer_CreatedDate'
      AND object_id = OBJECT_ID(N'dbo.[Order]')
)
BEGIN
    CREATE INDEX IX_Order_Customer_CreatedDate
        ON dbo.[Order](CustomerID, CreatedDate DESC);
END;
GO

/* ============================================================
   4. CHECK KẾT QUẢ
   ============================================================ */
SELECT
    Id,
    Name,
    CostPrice AS GiaNhap,
    Price AS GiaBanNiemYet,
    PriceSale AS GiaKhuyenMai,
    CASE
        WHEN PriceSale IS NOT NULL AND PriceSale > 0
            THEN PriceSale
        ELSE Price
    END AS GiaBanThucTe,
    CASE
        WHEN PriceSale IS NOT NULL AND PriceSale > 0
            THEN PriceSale - CostPrice
        ELSE Price - CostPrice
    END AS LoiNhuanDuKien
FROM dbo.Product
ORDER BY Id;
GO

PRINT N'GiftToi hardening database patch completed.';
GO
