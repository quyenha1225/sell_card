IF DB_ID(N'BBEcom') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [BBEcom] COLLATE Vietnamese_CI_AS;');
END;
GO

USE [BBEcom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @RESET_DEMO_DATA BIT = 0;
DECLARE @SEED_DEMO_IF_EMPTY BIT = 1;
DECLARE @SYNC_CANONICAL_NEWS BIT = 1;

/* ==================================================================
   0. BOOTSTRAP SCHEMA - TẠO DB/BẢNG NẾU CHƯA CÓ
   ================================================================== */

IF OBJECT_ID(N'dbo.Account', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Account
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Account PRIMARY KEY,
        UserName VARCHAR(50) NOT NULL,
        Password VARCHAR(100) NOT NULL,
        Name NVARCHAR(100) NULL,
        Email NVARCHAR(100) NULL,
        IsAdmin BIT NOT NULL,
        Status BIT NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Category
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Category PRIMARY KEY,
        Name NVARCHAR(50) NULL,
        Image NVARCHAR(250) NULL,
        Alias NVARCHAR(50) NULL,
        CreatedDate DATETIME NULL,
        Status BIT NULL
    );
END;

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
        Name NVARCHAR(100) NULL,
        Alias NVARCHAR(100) NULL,
        CategoryId INT NULL,
        Images NVARCHAR(500) NULL,
        CreatedDate DATETIME NULL,
        PriceSale INT NULL,
        Price INT NOT NULL,
        CostPrice INT NOT NULL,
        Detail NVARCHAR(MAX) NULL,
        Status BIT NULL,

        CONSTRAINT FK_Product_Category
            FOREIGN KEY (CategoryId) REFERENCES dbo.Category(Id)
    );
END;

IF OBJECT_ID(N'dbo.[Order]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Order]
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Order PRIMARY KEY,
        OrderCode NVARCHAR(50) NOT NULL,
        CustomerID INT NOT NULL,
        TotalAmount INT NOT NULL,
        Status BIT NOT NULL,
        CreatedDate DATE NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.Giftcard', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Giftcard
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Giftcard PRIMARY KEY,
        ProductId INT NULL,
        Code NVARCHAR(50) NULL,

        CONSTRAINT FK_Giftcard_Product
            FOREIGN KEY (ProductId) REFERENCES dbo.Product(Id)
    );
END;

IF OBJECT_ID(N'dbo.OrderDetail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDetail
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDetail PRIMARY KEY,
        OrderID INT NOT NULL,
        ProductID INT NOT NULL,
        Price INT NOT NULL,
        Quantity INT NOT NULL,
        Code NVARCHAR(50) NOT NULL,

        CONSTRAINT FK_OrderDetail_Order
            FOREIGN KEY (OrderID) REFERENCES dbo.[Order](Id),

        CONSTRAINT FK_OrderDetail_Product
            FOREIGN KEY (ProductID) REFERENCES dbo.Product(Id)
    );
END;

IF OBJECT_ID(N'dbo.Faq', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Faq
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Faq PRIMARY KEY,
        Question NVARCHAR(250) NULL,
        Answer NVARCHAR(MAX) NULL
    );
END;

IF OBJECT_ID(N'dbo.News', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.News
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_News PRIMARY KEY,
        Title NVARCHAR(100) NULL,
        Description NVARCHAR(MAX) NULL,
        Alias NVARCHAR(100) NULL,
        Image NVARCHAR(100) NULL,
        CreateDate DATETIME NULL
    );
END;

IF OBJECT_ID(N'dbo.SystemConfig', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemConfig
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemConfig PRIMARY KEY,
        Name NVARCHAR(50) NULL,
        Value NVARCHAR(50) NULL
    );
END;

IF OBJECT_ID(N'dbo.sysdiagrams', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.sysdiagrams
    (
        diagram_id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_sysdiagrams PRIMARY KEY,
        name NVARCHAR(128) NOT NULL,
        principal_id INT NOT NULL,
        version INT NULL,
        definition VARBINARY(MAX) NULL
    );
END;

/* Index FK cơ bản theo InitialCreate EF6 */
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Product_CategoryId'
      AND object_id = OBJECT_ID(N'dbo.Product')
)
    CREATE INDEX IX_Product_CategoryId ON dbo.Product(CategoryId);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Giftcard_ProductId_Base'
      AND object_id = OBJECT_ID(N'dbo.Giftcard')
)
    CREATE INDEX IX_Giftcard_ProductId_Base ON dbo.Giftcard(ProductId);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_OrderDetail_OrderID'
      AND object_id = OBJECT_ID(N'dbo.OrderDetail')
)
    CREATE INDEX IX_OrderDetail_OrderID ON dbo.OrderDetail(OrderID);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_OrderDetail_ProductID'
      AND object_id = OBJECT_ID(N'dbo.OrderDetail')
)
    CREATE INDEX IX_OrderDetail_ProductID ON dbo.OrderDetail(ProductID);

/* ==================================================================
   0.1. UNICODE HARDENING - CHỐNG LỖI FONT / MẤT DẤU TIẾNG VIỆT
   ================================================================== */

/* Các cột hiển thị nội dung cho người dùng luôn là Unicode. */
IF COL_LENGTH('dbo.Account', 'Name') IS NOT NULL
    ALTER TABLE dbo.Account ALTER COLUMN Name NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Account', 'Email') IS NOT NULL
    ALTER TABLE dbo.Account ALTER COLUMN Email NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Category', 'Name') IS NOT NULL
    ALTER TABLE dbo.Category ALTER COLUMN Name NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.Category', 'Image') IS NOT NULL
    ALTER TABLE dbo.Category ALTER COLUMN Image NVARCHAR(250) NULL;

IF COL_LENGTH('dbo.Category', 'Alias') IS NOT NULL
    ALTER TABLE dbo.Category ALTER COLUMN Alias NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.Product', 'Name') IS NOT NULL
    ALTER TABLE dbo.Product ALTER COLUMN Name NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Product', 'Alias') IS NOT NULL
    ALTER TABLE dbo.Product ALTER COLUMN Alias NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.Product', 'Images') IS NOT NULL
    ALTER TABLE dbo.Product ALTER COLUMN Images NVARCHAR(500) NULL;

IF COL_LENGTH('dbo.Product', 'Detail') IS NOT NULL
    ALTER TABLE dbo.Product ALTER COLUMN Detail NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Faq', 'Question') IS NOT NULL
    ALTER TABLE dbo.Faq ALTER COLUMN Question NVARCHAR(250) NULL;

IF COL_LENGTH('dbo.Faq', 'Answer') IS NOT NULL
    ALTER TABLE dbo.Faq ALTER COLUMN Answer NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.News', 'Title') IS NOT NULL
    ALTER TABLE dbo.News ALTER COLUMN Title NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.News', 'Description') IS NOT NULL
    ALTER TABLE dbo.News ALTER COLUMN Description NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.News', 'Alias') IS NOT NULL
    ALTER TABLE dbo.News ALTER COLUMN Alias NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.News', 'Image') IS NOT NULL
    ALTER TABLE dbo.News ALTER COLUMN Image NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.SystemConfig', 'Name') IS NOT NULL
    ALTER TABLE dbo.SystemConfig ALTER COLUMN Name NVARCHAR(50) NULL;

IF COL_LENGTH('dbo.SystemConfig', 'Value') IS NOT NULL
    ALTER TABLE dbo.SystemConfig ALTER COLUMN Value NVARCHAR(50) NULL;

PRINT N'Unicode schema OK: các cột nội dung chính đã dùng NVARCHAR.';

/* ==================================================================
   1. AUTH / OTP - IDEMPOTENT
   ================================================================== */
IF OBJECT_ID(N'dbo.PasswordResetOtp', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetOtp
    (
        Id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PasswordResetOtp PRIMARY KEY,
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


/* ==================================================================
   2. PRODUCT COST PRICE + CHUẨN HÓA LOGIC GIÁ
   ================================================================== */
IF COL_LENGTH('dbo.Product', 'CostPrice') IS NULL
BEGIN
    /* Default 0 chỉ dùng tạm để migrate các dòng cũ. */
    ALTER TABLE dbo.Product
    ADD CostPrice INT NOT NULL
        CONSTRAINT DF_Product_CostPrice DEFAULT (0);
END;

/* Chuẩn hóa sale cũ rõ ràng sai trước khi tính giá vốn. */
UPDATE dbo.Product
SET PriceSale = NULL
WHERE PriceSale IS NOT NULL
  AND
  (
      PriceSale <= 0
      OR PriceSale >= Price
  );

/* Không tự bịa giá bán nếu dữ liệu lõi đã sai. */
IF EXISTS
(
    SELECT 1
    FROM dbo.Product
    WHERE Price IS NULL OR Price <= 1
)
BEGIN
    RAISERROR(N'Có Product.Price <= 1. Hãy sửa giá bán trước khi chạy hardening.', 16, 1);
    RETURN;
END;

/* Backfill giá vốn cho dữ liệu cũ:
   khoảng 80% giá thực tế đang bán. */
UPDATE dbo.Product
SET CostPrice =
    CASE
        WHEN PriceSale IS NOT NULL
             AND PriceSale > 0
             AND PriceSale < Price
            THEN
                CASE
                    WHEN CAST(PriceSale * 0.80 AS INT) < 1 THEN 1
                    ELSE CAST(PriceSale * 0.80 AS INT)
                END
        ELSE
            CASE
                WHEN CAST(Price * 0.80 AS INT) < 1 THEN 1
                ELSE CAST(Price * 0.80 AS INT)
            END
    END
WHERE CostPrice IS NULL
   OR CostPrice <= 0
   OR CostPrice >=
      CASE
          WHEN PriceSale IS NOT NULL
               AND PriceSale > 0
               AND PriceSale < Price
              THEN PriceSale
          ELSE Price
      END;

/* Nếu promotion sau backfill vẫn không tạo lợi nhuận thì bỏ promotion. */
UPDATE dbo.Product
SET PriceSale = NULL
WHERE PriceSale IS NOT NULL
  AND
  (
      PriceSale <= CostPrice
      OR PriceSale >= Price
  );

/* Chuẩn hóa nullability nếu DB cũ từng có CostPrice nullable. */
IF COL_LENGTH('dbo.Product', 'CostPrice') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Product ALTER COLUMN CostPrice INT NOT NULL;
END;

/* Bỏ default tạm của CostPrice để mọi insert mới buộc phải truyền giá vốn. */
DECLARE @CostPriceDefault SYSNAME;

SELECT @CostPriceDefault = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c
    ON c.default_object_id = dc.object_id
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Product')
  AND c.name = N'CostPrice';

IF @CostPriceDefault IS NOT NULL
BEGIN
    DECLARE @DropCostPriceDefaultSql NVARCHAR(1000);

    SET @DropCostPriceDefaultSql =
        N'ALTER TABLE dbo.Product DROP CONSTRAINT '
        + QUOTENAME(@CostPriceDefault)
        + N';';

    EXEC sys.sp_executesql @DropCostPriceDefaultSql;
END;

/* Recreate check constraints theo một định nghĩa duy nhất. */
IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_CostPrice_Positive'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_CostPrice_Positive;

ALTER TABLE dbo.Product WITH CHECK
ADD CONSTRAINT CK_Product_CostPrice_Positive
CHECK (CostPrice > 0);


IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_Price_Above_Cost'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_Price_Above_Cost;

ALTER TABLE dbo.Product WITH CHECK
ADD CONSTRAINT CK_Product_Price_Above_Cost
CHECK (Price > CostPrice);


IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_Product_PriceSale_Range'
      AND parent_object_id = OBJECT_ID(N'dbo.Product')
)
    ALTER TABLE dbo.Product
    DROP CONSTRAINT CK_Product_PriceSale_Range;

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


/* ==================================================================
   3. INDEX HARDENING
   ================================================================== */
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

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Product_CategoryId_Status'
      AND object_id = OBJECT_ID(N'dbo.Product')
)
BEGIN
    CREATE INDEX IX_Product_CategoryId_Status
        ON dbo.Product(CategoryId, Status);
END;

/* OrderCode nên duy nhất vì SePay/polling tìm đơn bằng mã đơn. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.[Order]
    WHERE OrderCode IS NOT NULL
      AND LTRIM(RTRIM(OrderCode)) <> ''
    GROUP BY OrderCode
    HAVING COUNT(*) > 1
)
AND NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Order_OrderCode'
      AND object_id = OBJECT_ID(N'dbo.[Order]')
)
BEGIN
    CREATE UNIQUE INDEX UX_Order_OrderCode
        ON dbo.[Order](OrderCode)
        WHERE OrderCode IS NOT NULL AND OrderCode <> N'';
END
ELSE IF EXISTS
(
    SELECT 1
    FROM dbo.[Order]
    WHERE OrderCode IS NOT NULL
      AND LTRIM(RTRIM(OrderCode)) <> ''
    GROUP BY OrderCode
    HAVING COUNT(*) > 1
)
BEGIN
    PRINT N'WARNING: OrderCode đang bị trùng; chưa tạo UX_Order_OrderCode.';
END;

/* GiftCode đã cấp phải không bị trùng nhau. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Giftcard
    WHERE Code IS NOT NULL
      AND LTRIM(RTRIM(Code)) <> ''
    GROUP BY Code
    HAVING COUNT(*) > 1
)
AND NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_Giftcard_Code'
      AND object_id = OBJECT_ID(N'dbo.Giftcard')
)
BEGIN
    CREATE UNIQUE INDEX UX_Giftcard_Code
        ON dbo.Giftcard(Code)
        WHERE Code IS NOT NULL AND Code <> N'';
END
ELSE IF EXISTS
(
    SELECT 1
    FROM dbo.Giftcard
    WHERE Code IS NOT NULL
      AND LTRIM(RTRIM(Code)) <> ''
    GROUP BY Code
    HAVING COUNT(*) > 1
)
BEGIN
    PRINT N'WARNING: Giftcard.Code đang bị trùng; chưa tạo UX_Giftcard_Code.';
END;


/* ==================================================================
   4. OPTIONAL RESET + SEED DEMO
   Mặc định @RESET_DEMO_DATA = 0 => KHÔNG xóa dữ liệu.
   ================================================================== */
IF @RESET_DEMO_DATA = 1
   OR
   (
       @SEED_DEMO_IF_EMPTY = 1
       AND NOT EXISTS (SELECT 1 FROM dbo.Account)
       AND NOT EXISTS (SELECT 1 FROM dbo.Category)
       AND NOT EXISTS (SELECT 1 FROM dbo.Product)
       AND NOT EXISTS (SELECT 1 FROM dbo.Giftcard)
       AND NOT EXISTS (SELECT 1 FROM dbo.[Order])
       AND NOT EXISTS (SELECT 1 FROM dbo.OrderDetail)
   )
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        IF OBJECT_ID(N'dbo.PasswordResetOtp', N'U') IS NOT NULL
            DELETE FROM dbo.PasswordResetOtp;

        DELETE FROM dbo.OrderDetail;
        DELETE FROM dbo.[Order];
        DELETE FROM dbo.Giftcard;
        DELETE FROM dbo.Product;
        DELETE FROM dbo.Category;
        DELETE FROM dbo.News;
        DELETE FROM dbo.Faq;
        DELETE FROM dbo.SystemConfig;
        DELETE FROM dbo.Account;

        DBCC CHECKIDENT ('dbo.OrderDetail', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.[Order]', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.Giftcard', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.Product', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.Category', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.News', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.Faq', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.SystemConfig', RESEED, 0) WITH NO_INFOMSGS;
        DBCC CHECKIDENT ('dbo.Account', RESEED, 0) WITH NO_INFOMSGS;

        /* 1. Tài khoản */
    SET IDENTITY_INSERT [dbo].[Account] ON;

    INSERT INTO [dbo].[Account]
        ([Id], [UserName], [Password], [Name], [Email], [IsAdmin], [Status])
    VALUES
        (1, 'admin', '0e7517141fb53f21ee439b355b5a1d0a',
         N'Quản trị viên', 'admin@bbecom.vn', 1, 1),

        (2, 'user', '448ddd517d3abb70045aea6929f02367',
         N'Khách hàng demo', 'user@bbecom.vn', 0, 1);

    SET IDENTITY_INSERT [dbo].[Account] OFF;

    /* 2. Danh mục */
    SET IDENTITY_INSERT [dbo].[Category] ON;

    INSERT INTO [dbo].[Category]
        ([Id], [Name], [Image], [Alias], [CreatedDate], [Status])
    VALUES
        (1, N'Steam', 
         '/Content/Client/img/categories/steam.jpg',
         'steam', DATEADD(DAY, -30, GETDATE()), 1),

        (2, N'PlayStation',
         '/Content/Client/img/categories/playstation.jpg',
         'playstation', DATEADD(DAY, -29, GETDATE()), 1),

        (3, N'Xbox',
         '/Content/Client/img/categories/xbox.jpg',
         'xbox', DATEADD(DAY, -28, GETDATE()), 1),

        (4, N'Google Play',
         '/Content/Client/img/categories/google-play.jpg',
         'google-play', DATEADD(DAY, -27, GETDATE()), 1),

        (5, N'Apple & iTunes',
         '/Content/Client/img/categories/apple-itunes.jpg',
         'apple-itunes', DATEADD(DAY, -26, GETDATE()), 1),

        (6, N'Giải trí số',
         '/Content/Client/img/categories/digital-entertainment.jpg',
         'giai-tri-so', DATEADD(DAY, -25, GETDATE()), 1);

    SET IDENTITY_INSERT [dbo].[Category] OFF;

        /* 3. Sản phẩm */
    SET IDENTITY_INSERT [dbo].[Product] ON;

    INSERT INTO [dbo].[Product]
        ([Id], [Name], [Alias], [CategoryId], [Images], [CreatedDate],
         [PriceSale], [Price], [CostPrice], [Detail], [Status])
    VALUES
        (1, N'Steam Wallet 100.000đ', 'steam-wallet-100k', 1,
         '/Content/Client/img/products/products1.jpg',
         DATEADD(HOUR, -1, GETDATE()), 95000, 100000, 76000,
         N'<h3>Steam Wallet 100.000đ</h3><p>Mã nạp dùng cho tài khoản Steam Việt Nam. Code được gửi tự động sau khi thanh toán thành công.</p><ul><li>Nhận mã ngay</li><li>Không giới hạn thời gian sử dụng</li><li>Hỗ trợ 24/7</li></ul>', 1),

        (2, N'Steam Wallet 200.000đ', 'steam-wallet-200k', 1,
         '/Content/Client/img/products/products2.jpg',
         DATEADD(HOUR, -2, GETDATE()), 189000, 200000, 151200,
         N'<h3>Steam Wallet 200.000đ</h3><p>Nạp tiền vào ví Steam nhanh chóng để mua game, DLC và vật phẩm.</p>', 1),

        (3, N'Steam Wallet 500.000đ', 'steam-wallet-500k', 1,
         '/Content/Client/img/products/products3.jpg',
         DATEADD(HOUR, -3, GETDATE()), 469000, 500000, 375200,
         N'<h3>Steam Wallet 500.000đ</h3><p>Phiên bản mệnh giá cao, phù hợp mua các game AAA và gói mở rộng.</p>', 1),

        (4, N'PlayStation Store 10 USD', 'playstation-store-10-usd', 2,
         '/Content/Client/img/products/products4.jpg',
         DATEADD(HOUR, -4, GETDATE()), 245000, 259000, 196000,
         N'<h3>PlayStation Store 10 USD</h3><p>Mã dành cho tài khoản PlayStation khu vực US. Kiểm tra đúng vùng tài khoản trước khi nạp.</p>', 1),

        (5, N'PlayStation Plus Essential 1 tháng', 'ps-plus-essential-1-thang', 2,
         '/Content/Client/img/products/products5.jpg',
         DATEADD(HOUR, -5, GETDATE()), 229000, 249000, 183200,
         N'<h3>PlayStation Plus Essential</h3><p>Chơi multiplayer trực tuyến và nhận game miễn phí hàng tháng.</p>', 1),

        (6, N'PlayStation Plus Extra 3 tháng', 'ps-plus-extra-3-thang', 2,
         '/Content/Client/img/products/products6.jpg',
         DATEADD(HOUR, -6, GETDATE()), 799000, 849000, 639200,
         N'<h3>PlayStation Plus Extra</h3><p>Truy cập thư viện hàng trăm trò chơi PlayStation trong 3 tháng.</p>', 1),

        (7, N'Xbox Gift Card 10 USD', 'xbox-gift-card-10-usd', 3,
         '/Content/Client/img/products/products7.jpg',
         DATEADD(HOUR, -7, GETDATE()), 239000, 255000, 191200,
         N'<h3>Xbox Gift Card 10 USD</h3><p>Mua game, nội dung bổ sung và ứng dụng trên Microsoft Store.</p>', 1),

        (8, N'Xbox Game Pass Ultimate 1 tháng', 'xbox-game-pass-ultimate-1-thang', 3,
         '/Content/Client/img/products/products8.jpg',
         DATEADD(HOUR, -8, GETDATE()), 289000, 319000, 231200,
         N'<h3>Xbox Game Pass Ultimate</h3><p>Bao gồm Game Pass, EA Play và multiplayer trực tuyến.</p>', 1),

        (9, N'Xbox Game Pass PC 3 tháng', 'xbox-game-pass-pc-3-thang', 3,
         '/Content/Client/img/products/products9.jpg',
         DATEADD(HOUR, -9, GETDATE()), 399000, 449000, 319200,
         N'<h3>Xbox Game Pass PC</h3><p>Khám phá thư viện game PC phong phú trong 3 tháng.</p>', 1),

        (10, N'Google Play 100.000đ', 'google-play-100k', 4,
         '/Content/Client/img/products/products10.jpg',
         DATEADD(HOUR, -10, GETDATE()), 97000, 100000, 77600,
         N'<h3>Google Play 100.000đ</h3><p>Dùng mua ứng dụng, game, phim và vật phẩm trên Google Play.</p>', 1),

        (11, N'Google Play 200.000đ', 'google-play-200k', 4,
         '/Content/Client/img/products/products11.jpg',
         DATEADD(HOUR, -11, GETDATE()), 193000, 200000, 154400,
         N'<h3>Google Play 200.000đ</h3><p>Mã nạp Google Play gửi tự động qua hệ thống.</p>', 1),

        (12, N'Apple Gift Card 10 USD', 'apple-gift-card-10-usd', 5,
         '/Content/Client/img/products/products12.jpg',
         DATEADD(HOUR, -12, GETDATE()), 249000, 269000, 199200,
         N'<h3>Apple Gift Card 10 USD</h3><p>Dùng cho App Store, Apple Music, iCloud và các dịch vụ Apple tại khu vực US.</p>', 1),

        (13, N'Apple Music 3 tháng', 'apple-music-3-thang', 5,
         '/Content/Client/img/products/products13.jpg',
         DATEADD(HOUR, -13, GETDATE()), 199000, 229000, 159200,
         N'<h3>Apple Music 3 tháng</h3><p>Nghe nhạc không quảng cáo với chất lượng cao và tải về ngoại tuyến.</p>', 1),

        (14, N'Spotify Premium 3 tháng', 'spotify-premium-3-thang', 6,
         '/Content/Client/img/products/products14.jpg',
         DATEADD(HOUR, -14, GETDATE()), 159000, 189000, 127200,
         N'<h3>Spotify Premium 3 tháng</h3><p>Nghe nhạc không quảng cáo, bỏ qua không giới hạn và nghe offline.</p>', 1),

        (15, N'YouTube Premium 3 tháng', 'youtube-premium-3-thang', 6,
         '/Content/Client/img/products/products15.jpg',
         DATEADD(HOUR, -15, GETDATE()), 179000, 209000, 143200,
         N'<h3>YouTube Premium 3 tháng</h3><p>Xem video không quảng cáo, phát nền và sử dụng YouTube Music Premium.</p>', 1),

        (16, N'Netflix Premium 1 tháng', 'netflix-premium-1-thang', 6,
         '/Content/Client/img/products/products16.jpg',
         DATEADD(HOUR, -16, GETDATE()), 219000, 249000, 175200,
         N'<h3>Netflix Premium 1 tháng</h3><p>Gói xem phim chất lượng cao. Thông tin sử dụng được gửi sau khi thanh toán.</p>', 1),

        (17, N'Discord Nitro 1 tháng', 'discord-nitro-1-thang', 6,
         '/Content/Client/img/products/products17.jpg',
         DATEADD(HOUR, -17, GETDATE()), 179000, 199000, 143200,
         N'<h3>Discord Nitro 1 tháng</h3><p>Nâng cấp trải nghiệm Discord với emoji tùy chỉnh, upload dung lượng lớn và phát video HD.</p>', 1);

    SET IDENTITY_INSERT [dbo].[Product] OFF;

        /* 4. Kho mã game: 8 code cho mỗi sản phẩm */
    DECLARE @ProductId INT = 1;
    DECLARE @CodeNo INT;

    WHILE @ProductId <= 17
    BEGIN
        SET @CodeNo = 1;

        WHILE @CodeNo <= 8
        BEGIN
            INSERT INTO [dbo].[Giftcard] ([ProductId], [Code])
            VALUES
            (
                @ProductId,
                CONCAT(
                    'BBE-',
                    RIGHT('00' + CAST(@ProductId AS VARCHAR(2)), 2),
                    '-',
                    RIGHT('000' + CAST(@CodeNo AS VARCHAR(3)), 3),
                    '-',
                    UPPER(LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 12))
                )
            );

            SET @CodeNo += 1;
        END;

        SET @ProductId += 1;
    END;

        /* 6. Câu hỏi thường gặp */
    SET IDENTITY_INSERT [dbo].[Faq] ON;

    INSERT INTO [dbo].[Faq] ([Id], [Question], [Answer])
    VALUES
        (1, N'Sau khi thanh toán bao lâu tôi nhận được code?',
         N'<p>Hệ thống gửi code tự động ngay khi đơn hàng được xác nhận thanh toán thành công.</p>'),

        (2, N'Tôi nhập code nhưng không sử dụng được thì sao?',
         N'<p>Hãy chụp màn hình lỗi và liên hệ hỗ trợ. Không công khai code trên mạng xã hội.</p>'),

        (3, N'Sản phẩm có giới hạn khu vực không?',
         N'<p>Một số sản phẩm chỉ dùng cho tài khoản US hoặc Việt Nam. Vui lòng đọc kỹ mô tả trước khi mua.</p>'),

        (4, N'GiftToi có hoàn tiền không?',
         N'<p>Code kỹ thuật số đã hiển thị hoặc gửi cho khách thường không thể hoàn lại, trừ khi code lỗi do hệ thống.</p>'),

        (5, N'Tôi có thể thanh toán bằng SePay không?',
         N'<p>Có. GiftToi sử dụng SePay để xác nhận giao dịch và cập nhật trạng thái đơn hàng.</p>'),

        (6, N'Làm sao kiểm tra lịch sử đơn hàng?',
         N'<p>Đăng nhập tài khoản, mở trang hồ sơ và chọn mục lịch sử mua hàng.</p>');

    SET IDENTITY_INSERT [dbo].[Faq] OFF;

    /* 7. Cấu hình hệ thống */
    SET IDENTITY_INSERT [dbo].[SystemConfig] ON;

    INSERT INTO [dbo].[SystemConfig] ([Id], [Name], [Value])
    VALUES
        (1, 'SiteName', 'GiftToi'),
        (2, 'SupportEmail', 'support@bbecom.vn'),
        (3, 'SupportPhone', '0901234567'),
        (4, 'Currency', 'VND'),
        (5, 'OrderPrefix', 'BBE');

    SET IDENTITY_INSERT [dbo].[SystemConfig] OFF;

    /* 8. Đơn hàng mẫu */
    SET IDENTITY_INSERT [dbo].[Order] ON;

    INSERT INTO [dbo].[Order]
        ([Id], [OrderCode], [CustomerID], [TotalAmount], [Status], [CreatedDate])
    VALUES
        (1, 'BBE-DEMO-001', 2, 351000, 1, CAST(DATEADD(DAY, -3, GETDATE()) AS DATE)),
        (2, 'BBE-DEMO-002', 2, 478000, 1, CAST(DATEADD(DAY, -2, GETDATE()) AS DATE)),
        (3, 'BBE-DEMO-003', 2, 179000, 0, CAST(DATEADD(DAY, -1, GETDATE()) AS DATE));

    SET IDENTITY_INSERT [dbo].[Order] OFF;

    SET IDENTITY_INSERT [dbo].[OrderDetail] ON;

    INSERT INTO [dbo].[OrderDetail]
        ([Id], [OrderID], [ProductID], [Price], [Quantity], [Code])
    VALUES
        (1, 1, 1, 95000, 1, 'DEMO-STEAM-100K-001'),
        (2, 1, 10, 97000, 1, 'DEMO-GPLAY-100K-001'),
        (3, 1, 14, 159000, 1, 'DEMO-SPOTIFY-001'),

        (4, 2, 2, 189000, 1, 'DEMO-STEAM-200K-001'),
        (5, 2, 8, 289000, 1, 'DEMO-GAMEPASS-001'),

        (6, 3, 15, 179000, 1, 'DEMO-YOUTUBE-001');

    SET IDENTITY_INSERT [dbo].[OrderDetail] OFF;

        COMMIT TRANSACTION;

        PRINT N'Đã reset và seed lại dữ liệu demo GiftToi.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @SeedErrorMessage NVARCHAR(4000);
        DECLARE @SeedErrorSeverity INT;
        DECLARE @SeedErrorState INT;

        SELECT
            @SeedErrorMessage = ERROR_MESSAGE(),
            @SeedErrorSeverity = ERROR_SEVERITY(),
            @SeedErrorState = ERROR_STATE();

        RAISERROR(
            @SeedErrorMessage,
            @SeedErrorSeverity,
            @SeedErrorState
        );

        RETURN;
    END CATCH;
END
ELSE
BEGIN
    PRINT N'Giữ nguyên dữ liệu hiện tại; không reset/seed demo vì DB không rỗng.';
END;



/* ==================================================================
   4.1. CANONICAL FAQ - REPAIR/UPSERT UNICODE
   ================================================================== */
DECLARE @FaqCanonical TABLE
(
    Id INT NOT NULL PRIMARY KEY,
    Question NVARCHAR(250) NOT NULL,
    Answer NVARCHAR(MAX) NOT NULL
);

INSERT INTO @FaqCanonical (Id, Question, Answer)
VALUES
(1, N'Sau khi thanh toán bao lâu tôi nhận được code?',
 N'<p>Hệ thống gửi code tự động ngay khi đơn hàng được xác nhận thanh toán thành công.</p>'),
(2, N'Tôi nhập code nhưng không sử dụng được thì sao?',
 N'<p>Hãy chụp màn hình lỗi và liên hệ hỗ trợ. Không công khai code trên mạng xã hội.</p>'),
(3, N'Sản phẩm có giới hạn khu vực không?',
 N'<p>Một số sản phẩm chỉ dùng cho tài khoản US hoặc Việt Nam. Vui lòng đọc kỹ mô tả trước khi mua.</p>'),
(4, N'GiftToi có hoàn tiền không?',
 N'<p>Code kỹ thuật số đã hiển thị hoặc gửi cho khách thường không thể hoàn lại, trừ khi code lỗi do hệ thống.</p>'),
(5, N'Tôi có thể thanh toán bằng SePay không?',
 N'<p>Có. GiftToi sử dụng SePay để xác nhận giao dịch và cập nhật trạng thái đơn hàng.</p>'),
(6, N'Làm sao kiểm tra lịch sử đơn hàng?',
 N'<p>Đăng nhập tài khoản, mở trang hồ sơ và chọn mục lịch sử mua hàng.</p>');

UPDATE f
SET
    f.Question = c.Question,
    f.Answer = c.Answer
FROM dbo.Faq AS f
INNER JOIN @FaqCanonical AS c ON c.Id = f.Id;

SET IDENTITY_INSERT dbo.Faq ON;

INSERT INTO dbo.Faq (Id, Question, Answer)
SELECT c.Id, c.Question, c.Answer
FROM @FaqCanonical AS c
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Faq AS f
    WHERE f.Id = c.Id
);

SET IDENTITY_INSERT dbo.Faq OFF;

PRINT N'FAQ Unicode OK: đã cập nhật/thêm 6 câu hỏi chuẩn.';


/* ==================================================================
   5. CANONICAL NEWS
   Chỉ còn một bộ tin chuẩn; không seed 6 tin cũ rồi patch lần hai.
   ================================================================== */
IF @SYNC_CANONICAL_NEWS = 1
BEGIN
    DECLARE @News TABLE
(
    Title       NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Alias       NVARCHAR(100) NOT NULL,
    Image       NVARCHAR(100) NOT NULL,
    CreateDate  DATETIME NULL
);

INSERT INTO @News (Title, Description, Alias, Image, CreateDate)
VALUES
(
    N'Hướng dẫn mua Gift Card và nhận code tự động',
    N'<p>Mua Gift Card tại GiftToi được thiết kế theo quy trình ngắn gọn, minh bạch và phù hợp với cả người mới sử dụng sản phẩm số.</p>
      <h2>1. Chọn đúng sản phẩm</h2>
      <p>Hãy kiểm tra tên nền tảng, mệnh giá, khu vực tài khoản và thời hạn của gói dịch vụ. Một số mã chỉ dùng cho tài khoản Việt Nam hoặc Hoa Kỳ, vì vậy khu vực phải trùng với tài khoản nhận mã.</p>
      <h2>2. Thêm vào giỏ và kiểm tra đơn</h2>
      <p>Chọn số lượng cần mua, mở giỏ hàng và kiểm tra lại giá bán, số lượng cũng như tổng thanh toán. Hệ thống chỉ cho đặt số lượng không vượt quá lượng GiftCode còn trong kho.</p>
      <h2>3. Thanh toán và nhận mã</h2>
      <p>Sau khi cổng thanh toán xác nhận giao dịch, đơn hàng chuyển sang trạng thái hoàn tất. Mã đã mua được hiển thị trong mục <strong>Lịch sử mua hàng</strong> của tài khoản.</p>
      <ul><li>Không chia sẻ ảnh chụp code cho người khác.</li><li>Sao chép mã trực tiếp từ lịch sử mua hàng.</li><li>Liên hệ hỗ trợ nếu trạng thái thanh toán chưa cập nhật.</li></ul>',
    N'huong-dan-mua-gift-card',
    N'/Content/Client/img/news/huong-dan-mua-gift-card.jpg',
    DATEADD(DAY, -1, GETDATE())
),
(
    N'Cách nạp Steam Wallet an toàn',
    N'<p>Steam Wallet Code giúp bổ sung số dư để mua game, nội dung mở rộng và vật phẩm trên Steam. Trước khi nạp, bạn cần kiểm tra kỹ khu vực của tài khoản.</p>
      <h2>Kiểm tra khu vực tài khoản</h2>
      <p>Mở Steam, vào phần chi tiết tài khoản và xác nhận quốc gia cửa hàng. Mã không đúng khu vực có thể bị từ chối và mã kỹ thuật số đã hiển thị thường không thể đổi trả.</p>
      <h2>Các bước nhập mã</h2>
      <ol><li>Đăng nhập Steam trên ứng dụng hoặc trình duyệt chính thức.</li><li>Mở mục Redeem a Steam Gift Card or Wallet Code.</li><li>Nhập mã chính xác và xác nhận.</li></ol>
      <h2>Lưu ý bảo mật</h2>
      <p>Không gửi code qua nhóm công khai, không nhập mã vào trang lạ và nên bật Steam Guard. GiftToi không yêu cầu khách hàng cung cấp mật khẩu Steam.</p>',
    N'cach-nap-steam-wallet-an-toan',
    N'/Content/Client/img/news/steam-wallet-an-toan.jpg',
    DATEADD(DAY, -2, GETDATE())
),
(
    N'5 lợi ích nổi bật của Xbox Game Pass',
    N'<p>Xbox Game Pass phù hợp với người muốn trải nghiệm nhiều trò chơi mà không phải mua riêng từng tựa game.</p>
      <h2>Thư viện game đa dạng</h2>
      <p>Người dùng có thể khám phá nhiều thể loại từ hành động, thể thao, chiến thuật đến game độc lập.</p>
      <h2>Chi phí dễ kiểm soát</h2>
      <p>Thay vì thanh toán từng game, bạn trả phí theo thời hạn gói. Điều này phù hợp với người thường xuyên thay đổi trò chơi.</p>
      <h2>Quyền lợi bổ sung</h2>
      <ul><li>Nhiều game được bổ sung định kỳ.</li><li>Một số gói có EA Play và chơi trực tuyến.</li><li>Có ưu đãi khi mua game đang nằm trong thư viện.</li><li>Hỗ trợ cả PC hoặc Xbox tùy loại gói.</li><li>Dễ gia hạn bằng mã kỹ thuật số.</li></ul>
      <p>Trước khi mua, hãy kiểm tra đúng loại Game Pass, khu vực và thời hạn sử dụng.</p>',
    N'loi-ich-xbox-game-pass',
    N'/Content/Client/img/news/xbox-game-pass.jpg',
    DATEADD(DAY, -3, GETDATE())
),
(
    N'PlayStation Plus có gì đáng chú ý?',
    N'<p>PlayStation Plus cung cấp nhiều cấp độ dịch vụ khác nhau. Việc chọn đúng gói giúp bạn tránh trả thêm cho những quyền lợi không sử dụng.</p>
      <h2>Essential</h2><p>Phù hợp với nhu cầu chơi trực tuyến, lưu dữ liệu đám mây và nhận một số game hàng tháng.</p>
      <h2>Extra</h2><p>Bổ sung thư viện game PS4 và PS5 để tải và chơi trong thời gian còn thuê bao.</p>
      <h2>Premium hoặc Deluxe</h2><p>Tùy khu vực, cấp cao nhất có thể thêm game cổ điển, bản dùng thử và một số tính năng nâng cao.</p>
      <h2>Trước khi kích hoạt</h2>
      <ul><li>Kiểm tra khu vực PSN.</li><li>Đảm bảo mã trùng khu vực tài khoản.</li><li>Kiểm tra thuê bao hiện tại để tránh cộng dồn sai gói.</li></ul>',
    N'playstation-plus-co-gi',
    N'/Content/Client/img/news/playstation-plus.jpg',
    DATEADD(DAY, -4, GETDATE())
),
(
    N'Mẹo bảo mật tài khoản giải trí số',
    N'<p>Tài khoản game và giải trí số thường chứa lịch sử thanh toán, vật phẩm và thông tin cá nhân. Chỉ một mật khẩu yếu cũng có thể khiến toàn bộ dữ liệu gặp rủi ro.</p>
      <h2>Dùng mật khẩu riêng</h2><p>Không dùng chung mật khẩu Gmail, Steam, PlayStation, Xbox và các dịch vụ khác. Mỗi tài khoản nên có mật khẩu dài và khác nhau.</p>
      <h2>Bật xác minh hai bước</h2><p>Ưu tiên ứng dụng xác thực hoặc khóa bảo mật. Mã OTP chỉ nhập trên website hoặc ứng dụng chính thức.</p>
      <h2>Nhận biết liên kết giả</h2><ul><li>Kiểm tra tên miền trước khi đăng nhập.</li><li>Không mở tệp lạ được gửi qua tin nhắn.</li><li>Không cung cấp cookie, OTP hoặc mã khôi phục.</li></ul>
      <p>Khi phát hiện đăng nhập bất thường, hãy đổi mật khẩu, đăng xuất khỏi thiết bị lạ và kiểm tra email khôi phục ngay lập tức.</p>',
    N'meo-bao-mat-tai-khoan',
    N'/Content/Client/img/news/bao-mat-tai-khoan.jpg',
    DATEADD(DAY, -5, GETDATE())
),
(
    N'Hướng dẫn thanh toán đơn hàng trên GiftToi',
    N'<p>GiftToi sử dụng cổng thanh toán để đối soát giao dịch và tự động cập nhật trạng thái đơn hàng.</p>
      <h2>Kiểm tra thông tin trước khi trả tiền</h2><p>Đối chiếu mã đơn, số tiền và nội dung thanh toán. Không sửa nội dung khi hệ thống đã tạo sẵn thông tin giao dịch.</p>
      <h2>Sau khi thanh toán</h2><p>Giữ nguyên trang trong lúc cổng thanh toán chuyển về GiftToi. Thời gian xác nhận thường ngắn nhưng có thể chậm hơn khi ngân hàng bảo trì.</p>
      <h2>Khi trạng thái chưa cập nhật</h2><ul><li>Không thanh toán lại ngay.</li><li>Kiểm tra lịch sử giao dịch ngân hàng.</li><li>Lưu ảnh biên lai và mã đơn.</li><li>Liên hệ hỗ trợ để được đối soát.</li></ul>',
    N'thanh-toan-tren-gifttoi',
    N'/Content/Client/img/news/thanh-toan-vnpay.jpg',
    DATEADD(DAY, -6, GETDATE())
),
(
    N'Chọn Gift Card Google Play và Apple đúng khu vực',
    N'<p>Google Play và Apple Gift Card đều áp dụng giới hạn khu vực. Mã đúng mệnh giá nhưng sai quốc gia vẫn có thể không sử dụng được.</p>
      <h2>Google Play</h2><p>Kiểm tra quốc gia trong hồ sơ thanh toán Google. Một số tài khoản yêu cầu phương thức thanh toán hoặc vị trí phù hợp với khu vực của mã.</p>
      <h2>Apple Gift Card</h2><p>Xem quốc gia hoặc vùng của Apple ID trước khi mua. Số dư thường chỉ dùng trong cùng hệ sinh thái và cùng khu vực.</p>
      <h2>Khuyến nghị</h2><ul><li>Không đổi vùng chỉ để thử mã khi chưa hiểu điều kiện.</li><li>Đọc tên sản phẩm và mô tả khu vực.</li><li>Liên hệ hỗ trợ trước khi mua nếu chưa chắc chắn.</li></ul>',
    N'google-play-apple-dung-khu-vuc',
    N'/Content/Client/img/news/google-play-apple.jpg',
    DATEADD(DAY, -7, GETDATE())
),
(
    N'Cách xử lý khi Gift Code báo lỗi',
    N'<p>Khi mã báo lỗi, bạn không nên thử liên tục hoặc đăng công khai code. Hãy lưu lại đầy đủ bằng chứng để kiểm tra chính xác.</p>
      <h2>Kiểm tra lỗi nhập liệu</h2><p>Phân biệt các ký tự dễ nhầm như O và 0, I và 1. Sao chép trực tiếp từ lịch sử mua hàng để tránh thiếu ký tự.</p>
      <h2>Kiểm tra khu vực và sản phẩm</h2><p>Đảm bảo bạn đang nhập mã đúng nền tảng, đúng tài khoản và đúng khu vực được ghi trong mô tả sản phẩm.</p>
      <h2>Gửi yêu cầu hỗ trợ</h2><ul><li>Mã đơn hàng.</li><li>Ảnh thông báo lỗi đầy đủ.</li><li>Thời điểm nhập mã.</li><li>Không che nội dung lỗi nhưng phải che phần code khi đăng công khai.</li></ul>
      <p>GiftToi sẽ kiểm tra lịch sử cấp mã và phối hợp đối soát khi có đủ thông tin.</p>',
    N'xu-ly-gift-code-bao-loi',
    N'/Content/Client/img/news/xu-ly-code-loi.jpg',
    DATEADD(DAY, -8, GETDATE())
);

    /* Map alias cũ để không tạo bài trùng. */
    UPDATE dbo.News
    SET Alias = N'huong-dan-mua-gift-card'
    WHERE Alias = N'huong-dan-mua-ma-game-tu-dong';

    UPDATE dbo.News
    SET Alias = N'thanh-toan-tren-gifttoi'
    WHERE Alias = N'thanh-toan-vnpay-bbecom';

    /* UPDATE trước, INSERT những alias chưa tồn tại.
       Tránh MERGE để script dễ đọc và ít edge-case hơn. */
    UPDATE target
    SET
        target.Title = source.Title,
        target.Description = source.Description,
        target.Image = source.Image,
        target.CreateDate = source.CreateDate
    FROM dbo.News AS target
    INNER JOIN @News AS source
        ON source.Alias = target.Alias;

    INSERT INTO dbo.News
        (Title, Description, Alias, Image, CreateDate)
    SELECT
        source.Title,
        source.Description,
        source.Alias,
        source.Image,
        source.CreateDate
    FROM @News AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.News AS target
        WHERE target.Alias = source.Alias
    );

    PRINT N'Đã đồng bộ bộ tin tức chuẩn GiftToi.';
END;


/* ==================================================================
   6. ACCOUNT DATA QUALITY
   Không tự xóa tài khoản trùng. Chỉ cảnh báo để tránh phá dữ liệu.
   ================================================================== */
IF EXISTS
(
    SELECT 1
    FROM dbo.Account
    WHERE Email IS NOT NULL
      AND LTRIM(RTRIM(Email)) <> ''
    GROUP BY LOWER(LTRIM(RTRIM(Email)))
    HAVING COUNT(*) > 1
)
BEGIN
    PRINT N'WARNING: Có email Account bị trùng. Google Login có thể chọn sai account.';
END;


/* ==================================================================
   7. FINAL HEALTH CHECK
   ================================================================== */
SELECT
    'Account' AS TableName, COUNT(*) AS TotalRows FROM dbo.Account
UNION ALL
SELECT 'Category', COUNT(*) FROM dbo.Category
UNION ALL
SELECT 'Product', COUNT(*) FROM dbo.Product
UNION ALL
SELECT 'Giftcard', COUNT(*) FROM dbo.Giftcard
UNION ALL
SELECT 'News', COUNT(*) FROM dbo.News
UNION ALL
SELECT 'Faq', COUNT(*) FROM dbo.Faq
UNION ALL
SELECT 'SystemConfig', COUNT(*) FROM dbo.SystemConfig
UNION ALL
SELECT 'Order', COUNT(*) FROM dbo.[Order]
UNION ALL
SELECT 'OrderDetail', COUNT(*) FROM dbo.OrderDetail
UNION ALL
SELECT 'PasswordResetOtp', COUNT(*) FROM dbo.PasswordResetOtp;


SELECT
    Id,
    Name,
    CostPrice AS GiaNhap,
    Price AS GiaBanNiemYet,
    PriceSale AS GiaKhuyenMai,
    CASE
        WHEN PriceSale IS NOT NULL
             AND PriceSale > 0
             AND PriceSale < Price
            THEN PriceSale
        ELSE Price
    END AS GiaBanThucTe,
    CASE
        WHEN PriceSale IS NOT NULL
             AND PriceSale > 0
             AND PriceSale < Price
            THEN PriceSale - CostPrice
        ELSE Price - CostPrice
    END AS LoiNhuanDuKien,
    CAST
    (
        (
            (
                CASE
                    WHEN PriceSale IS NOT NULL
                         AND PriceSale > 0
                         AND PriceSale < Price
                        THEN PriceSale
                    ELSE Price
                END
            ) - CostPrice
        ) * 100.0 / NULLIF(CostPrice, 0)
        AS DECIMAL(10,2)
    ) AS TySuatLoiNhuanPhanTram
FROM dbo.Product
ORDER BY Id;


/* Chỉ SELECT ra các lỗi còn lại.
   Kết quả lý tưởng: 0 dòng. */
SELECT
    Id,
    Name,
    CostPrice,
    Price,
    PriceSale,
    N'INVALID_PRICE_LOGIC' AS Problem
FROM dbo.Product
WHERE CostPrice <= 0
   OR Price <= CostPrice
   OR
   (
       PriceSale IS NOT NULL
       AND
       (
           PriceSale <= CostPrice
           OR PriceSale >= Price
       )
   );


SELECT
    OrderCode,
    COUNT(*) AS DuplicateCount
FROM dbo.[Order]
WHERE OrderCode IS NOT NULL
  AND LTRIM(RTRIM(OrderCode)) <> ''
GROUP BY OrderCode
HAVING COUNT(*) > 1;


SELECT
    Code,
    COUNT(*) AS DuplicateCount
FROM dbo.Giftcard
WHERE Code IS NOT NULL
  AND LTRIM(RTRIM(Code)) <> ''
GROUP BY Code
HAVING COUNT(*) > 1;


SELECT
    Email,
    COUNT(*) AS DuplicateCount
FROM dbo.Account
WHERE Email IS NOT NULL
  AND LTRIM(RTRIM(Email)) <> ''
GROUP BY Email
HAVING COUNT(*) > 1;


SELECT
    Id,
    UserName,
    Name,
    Email,
    IsAdmin,
    Status
FROM dbo.Account
ORDER BY Id;


SELECT
    Id,
    Title,
    Alias,
    Image,
    CreateDate
FROM dbo.News
ORDER BY CreateDate DESC, Id DESC;


PRINT N'==============================================================';
PRINT N'GiftToi DATABASE FINAL CLEAN hoàn tất.';
PRINT N'Kiểm tra các result-set cuối: INVALID_PRICE / duplicate phải = 0 dòng.';
PRINT N'==============================================================';


/* ==================================================================
   7. UNICODE / FONT HEALTH CHECK
   ================================================================== */
SELECT
    N'FAQ Unicode sample' AS CheckName,
    Id,
    Question
FROM dbo.Faq
WHERE Id BETWEEN 1 AND 6
ORDER BY Id;

SELECT TOP (10)
    N'Product Unicode sample' AS CheckName,
    Id,
    Name
FROM dbo.Product
ORDER BY Id;

SELECT TOP (10)
    N'News Unicode sample' AS CheckName,
    Id,
    Title
FROM dbo.News
ORDER BY Id;

PRINT N'HOÀN TẤT GiftToi database. Nếu các SELECT trên hiển thị đúng dấu tiếng Việt thì dữ liệu DB đã Unicode đúng.';