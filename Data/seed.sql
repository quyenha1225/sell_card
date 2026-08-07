USE [BBEcom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ================================================================
   1. MIGRATION GIÁ NHẬP / GIÁ VỐN
   ================================================================ */

IF COL_LENGTH('dbo.Product', 'CostPrice') IS NULL
BEGIN
    ALTER TABLE dbo.Product
    ADD CostPrice INT NOT NULL
        CONSTRAINT DF_Product_CostPrice DEFAULT (0);
END;
GO

/* ================================================================
   2. AUTH / OTP MIGRATION
   ================================================================ */

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

/* ================================================================
   3. RESET + SEED DỮ LIỆU DEMO
   ================================================================ */

/*
    BBEcom sample data
    Chạy trong SQL Server Management Studio trên database BBEcom.

    Tài khoản mẫu:
      Admin: admin / Admin@123
      User : user  / User@123

    Ảnh danh mục thương hiệu mới:
      Sao chép 6 ảnh trong gói BBEcom_Category_Images.zip vào:
      /Content/Client/img/categories/
      /Content/Client/img/products/
      /Content/Client/img/blog/
*/

USE [BBEcom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Xóa dữ liệu mẫu cũ theo đúng thứ tự khóa ngoại */
    DELETE FROM [dbo].[OrderDetail];
    DELETE FROM [dbo].[Order];
    DELETE FROM [dbo].[Giftcard];
    DELETE FROM [dbo].[Product];
    DELETE FROM [dbo].[Category];
    DELETE FROM [dbo].[News];
    DELETE FROM [dbo].[Faq];
    DELETE FROM [dbo].[SystemConfig];
    DELETE FROM [dbo].[Account];

    DBCC CHECKIDENT ('dbo.OrderDetail', RESEED, 0);
    DBCC CHECKIDENT ('dbo.[Order]', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Giftcard', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Product', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Category', RESEED, 0);
    DBCC CHECKIDENT ('dbo.News', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Faq', RESEED, 0);
    DBCC CHECKIDENT ('dbo.SystemConfig', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Account', RESEED, 0);

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
         [PriceSale], [Price], [Detail], [Status])
    VALUES
        (1, N'Steam Wallet 100.000đ', 'steam-wallet-100k', 1,
         '/Content/Client/img/products/products1.jpg',
         DATEADD(HOUR, -1, GETDATE()), 95000, 100000,
         N'<h3>Steam Wallet 100.000đ</h3><p>Mã nạp dùng cho tài khoản Steam Việt Nam. Code được gửi tự động sau khi thanh toán thành công.</p><ul><li>Nhận mã ngay</li><li>Không giới hạn thời gian sử dụng</li><li>Hỗ trợ 24/7</li></ul>', 1),

        (2, N'Steam Wallet 200.000đ', 'steam-wallet-200k', 1,
         '/Content/Client/img/products/products2.jpg',
         DATEADD(HOUR, -2, GETDATE()), 189000, 200000,
         N'<h3>Steam Wallet 200.000đ</h3><p>Nạp tiền vào ví Steam nhanh chóng để mua game, DLC và vật phẩm.</p>', 1),

        (3, N'Steam Wallet 500.000đ', 'steam-wallet-500k', 1,
         '/Content/Client/img/products/products3.jpg',
         DATEADD(HOUR, -3, GETDATE()), 469000, 500000,
         N'<h3>Steam Wallet 500.000đ</h3><p>Phiên bản mệnh giá cao, phù hợp mua các game AAA và gói mở rộng.</p>', 1),

        (4, N'PlayStation Store 10 USD', 'playstation-store-10-usd', 2,
         '/Content/Client/img/products/products4.jpg',
         DATEADD(HOUR, -4, GETDATE()), 245000, 259000,
         N'<h3>PlayStation Store 10 USD</h3><p>Mã dành cho tài khoản PlayStation khu vực US. Kiểm tra đúng vùng tài khoản trước khi nạp.</p>', 1),

        (5, N'PlayStation Plus Essential 1 tháng', 'ps-plus-essential-1-thang', 2,
         '/Content/Client/img/products/products5.jpg',
         DATEADD(HOUR, -5, GETDATE()), 229000, 249000,
         N'<h3>PlayStation Plus Essential</h3><p>Chơi multiplayer trực tuyến và nhận game miễn phí hàng tháng.</p>', 1),

        (6, N'PlayStation Plus Extra 3 tháng', 'ps-plus-extra-3-thang', 2,
         '/Content/Client/img/products/products6.jpg',
         DATEADD(HOUR, -6, GETDATE()), 799000, 849000,
         N'<h3>PlayStation Plus Extra</h3><p>Truy cập thư viện hàng trăm trò chơi PlayStation trong 3 tháng.</p>', 1),

        (7, N'Xbox Gift Card 10 USD', 'xbox-gift-card-10-usd', 3,
         '/Content/Client/img/products/products7.jpg',
         DATEADD(HOUR, -7, GETDATE()), 239000, 255000,
         N'<h3>Xbox Gift Card 10 USD</h3><p>Mua game, nội dung bổ sung và ứng dụng trên Microsoft Store.</p>', 1),

        (8, N'Xbox Game Pass Ultimate 1 tháng', 'xbox-game-pass-ultimate-1-thang', 3,
         '/Content/Client/img/products/products8.jpg',
         DATEADD(HOUR, -8, GETDATE()), 289000, 319000,
         N'<h3>Xbox Game Pass Ultimate</h3><p>Bao gồm Game Pass, EA Play và multiplayer trực tuyến.</p>', 1),

        (9, N'Xbox Game Pass PC 3 tháng', 'xbox-game-pass-pc-3-thang', 3,
         '/Content/Client/img/products/products9.jpg',
         DATEADD(HOUR, -9, GETDATE()), 399000, 449000,
         N'<h3>Xbox Game Pass PC</h3><p>Khám phá thư viện game PC phong phú trong 3 tháng.</p>', 1),

        (10, N'Google Play 100.000đ', 'google-play-100k', 4,
         '/Content/Client/img/products/products10.jpg',
         DATEADD(HOUR, -10, GETDATE()), 97000, 100000,
         N'<h3>Google Play 100.000đ</h3><p>Dùng mua ứng dụng, game, phim và vật phẩm trên Google Play.</p>', 1),

        (11, N'Google Play 200.000đ', 'google-play-200k', 4,
         '/Content/Client/img/products/products11.jpg',
         DATEADD(HOUR, -11, GETDATE()), 193000, 200000,
         N'<h3>Google Play 200.000đ</h3><p>Mã nạp Google Play gửi tự động qua hệ thống.</p>', 1),

        (12, N'Apple Gift Card 10 USD', 'apple-gift-card-10-usd', 5,
         '/Content/Client/img/products/products12.jpg',
         DATEADD(HOUR, -12, GETDATE()), 249000, 269000,
         N'<h3>Apple Gift Card 10 USD</h3><p>Dùng cho App Store, Apple Music, iCloud và các dịch vụ Apple tại khu vực US.</p>', 1),

        (13, N'Apple Music 3 tháng', 'apple-music-3-thang', 5,
         '/Content/Client/img/products/products13.jpg',
         DATEADD(HOUR, -13, GETDATE()), 199000, 229000,
         N'<h3>Apple Music 3 tháng</h3><p>Nghe nhạc không quảng cáo với chất lượng cao và tải về ngoại tuyến.</p>', 1),

        (14, N'Spotify Premium 3 tháng', 'spotify-premium-3-thang', 6,
         '/Content/Client/img/products/products14.jpg',
         DATEADD(HOUR, -14, GETDATE()), 159000, 189000,
         N'<h3>Spotify Premium 3 tháng</h3><p>Nghe nhạc không quảng cáo, bỏ qua không giới hạn và nghe offline.</p>', 1),

        (15, N'YouTube Premium 3 tháng', 'youtube-premium-3-thang', 6,
         '/Content/Client/img/products/products15.jpg',
         DATEADD(HOUR, -15, GETDATE()), 179000, 209000,
         N'<h3>YouTube Premium 3 tháng</h3><p>Xem video không quảng cáo, phát nền và sử dụng YouTube Music Premium.</p>', 1),

        (16, N'Netflix Premium 1 tháng', 'netflix-premium-1-thang', 6,
         '/Content/Client/img/products/products16.jpg',
         DATEADD(HOUR, -16, GETDATE()), 219000, 249000,
         N'<h3>Netflix Premium 1 tháng</h3><p>Gói xem phim chất lượng cao. Thông tin sử dụng được gửi sau khi thanh toán.</p>', 1),

        (17, N'Discord Nitro 1 tháng', 'discord-nitro-1-thang', 6,
         '/Content/Client/img/products/products17.jpg',
         DATEADD(HOUR, -17, GETDATE()), 179000, 199000,
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

    /* 5. Tin tức */
    SET IDENTITY_INSERT [dbo].[News] ON;

    INSERT INTO [dbo].[News]
        ([Id], [Title], [Description], [Alias], [Image], [CreateDate])
    VALUES
        (1, N'Hướng dẫn mua mã game tự động',
         N'<p>Chọn sản phẩm, thêm vào giỏ hàng, nhập thông tin thanh toán và nhận code ngay sau khi giao dịch thành công.</p>',
         'huong-dan-mua-ma-game-tu-dong',
         '/Content/Client/img/blog/blog1.jpg',
         DATEADD(DAY, -2, GETDATE())),

        (2, N'Cách nạp Steam Wallet an toàn',
         N'<p>Kiểm tra đúng khu vực tài khoản và không chia sẻ mã đã mua cho người khác.</p>',
         'cach-nap-steam-wallet-an-toan',
         '/Content/Client/img/blog/blog2.jpg',
         DATEADD(DAY, -4, GETDATE())),

        (3, N'5 lợi ích của Xbox Game Pass',
         N'<p>Game Pass giúp người chơi trải nghiệm hàng trăm tựa game với chi phí hợp lý.</p>',
         'loi-ich-xbox-game-pass',
         '/Content/Client/img/blog/blog3.jpg',
         DATEADD(DAY, -6, GETDATE())),

        (4, N'PlayStation Plus có gì đáng chú ý?',
         N'<p>So sánh các gói Essential, Extra và Premium để chọn đúng nhu cầu.</p>',
         'playstation-plus-co-gi',
         '/Content/Client/img/blog/blog4.jpg',
         DATEADD(DAY, -8, GETDATE())),

        (5, N'Mẹo bảo mật tài khoản giải trí số',
         N'<p>Bật xác thực hai bước và sử dụng mật khẩu riêng cho từng nền tảng.</p>',
         'meo-bao-mat-tai-khoan',
         '/Content/Client/img/blog/blog5.jpg',
         DATEADD(DAY, -10, GETDATE())),

        (6, N'Thanh toán VNPay trên BBEcom',
         N'<p>Khách hàng có thể thanh toán qua môi trường VNPay Sandbox để kiểm thử quy trình đặt hàng.</p>',
         'thanh-toan-vnpay-bbecom',
         '/Content/Client/img/blog/blog6.jpg',
         DATEADD(DAY, -12, GETDATE()));

    SET IDENTITY_INSERT [dbo].[News] OFF;

    /* 6. Câu hỏi thường gặp */
    SET IDENTITY_INSERT [dbo].[Faq] ON;

    INSERT INTO [dbo].[Faq] ([Id], [Question], [Answer])
    VALUES
        (1, 'Sau khi thanh toán bao lâu tôi nhận được code?',
         '<p>Hệ thống gửi code tự động ngay khi đơn hàng được xác nhận thanh toán thành công.</p>'),

        (2, 'Tôi nhập code nhưng không sử dụng được thì sao?',
         '<p>Hãy chụp màn hình lỗi và liên hệ hỗ trợ. Không công khai code trên mạng xã hội.</p>'),

        (3, 'Sản phẩm có giới hạn khu vực không?',
         '<p>Một số sản phẩm chỉ dùng cho tài khoản US hoặc Việt Nam. Vui lòng đọc kỹ mô tả trước khi mua.</p>'),

        (4, 'BBEcom có hoàn tiền không?',
         '<p>Code kỹ thuật số đã hiển thị hoặc gửi cho khách thường không thể hoàn lại, trừ khi code lỗi do hệ thống.</p>'),

        (5, 'Tôi có thể thanh toán bằng VNPay không?',
         '<p>Có. Dự án hỗ trợ VNPay Sandbox để mô phỏng thanh toán trực tuyến.</p>'),

        (6, 'Làm sao kiểm tra lịch sử đơn hàng?',
         '<p>Đăng nhập tài khoản, mở trang hồ sơ và chọn mục lịch sử mua hàng.</p>');

    SET IDENTITY_INSERT [dbo].[Faq] OFF;

    /* 7. Cấu hình hệ thống */
    SET IDENTITY_INSERT [dbo].[SystemConfig] ON;

    INSERT INTO [dbo].[SystemConfig] ([Id], [Name], [Value])
    VALUES
        (1, 'SiteName', 'BBEcom'),
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
        (1, 'BBE-DEMO-001', 2, 284000, 1, CAST(DATEADD(DAY, -3, GETDATE()) AS DATE)),
        (2, 'BBE-DEMO-002', 2, 478000, 1, CAST(DATEADD(DAY, -2, GETDATE()) AS DATE)),
        (3, 'BBE-DEMO-003', 2, 179000, 0, CAST(DATEADD(DAY, -1, GETDATE()) AS DATE));

    SET IDENTITY_INSERT [dbo].[Order] OFF;

    SET IDENTITY_INSERT [dbo].[OrderDetail] ON;

    INSERT INTO [dbo].[OrderDetail]
        ([Id], [OrderID], [ProductID], [Price], [Quantity], [Code])
    VALUES
        (1, 1, 1, 95000, 1, 'DEMO-STEAM-100K-001'),
        (2, 1, 10, 97000, 1, 'DEMO-GPLAY-100K-001'),
        (3, 1, 14, 92000, 1, 'DEMO-SPOTIFY-001'),

        (4, 2, 2, 189000, 1, 'DEMO-STEAM-200K-001'),
        (5, 2, 8, 289000, 1, 'DEMO-GAMEPASS-001'),

        (6, 3, 15, 179000, 1, 'DEMO-YOUTUBE-001');

    SET IDENTITY_INSERT [dbo].[OrderDetail] OFF;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

/* Kiểm tra số lượng dữ liệu */
SELECT 'Account' AS TableName, COUNT(*) AS TotalRows FROM [dbo].[Account]
UNION ALL SELECT 'Category', COUNT(*) FROM [dbo].[Category]
UNION ALL SELECT 'Product', COUNT(*) FROM [dbo].[Product]
UNION ALL SELECT 'Giftcard', COUNT(*) FROM [dbo].[Giftcard]
UNION ALL SELECT 'News', COUNT(*) FROM [dbo].[News]
UNION ALL SELECT 'Faq', COUNT(*) FROM [dbo].[Faq]
UNION ALL SELECT 'SystemConfig', COUNT(*) FROM [dbo].[SystemConfig]
UNION ALL SELECT 'Order', COUNT(*) FROM [dbo].[Order]
UNION ALL SELECT 'OrderDetail', COUNT(*) FROM [dbo].[OrderDetail];
GO

/* ================================================================
   4. GÁN GIÁ NHẬP CHO DỮ LIỆU DEMO
      Giá nhập demo = 80% giá thực tế đang bán.
      Sau này Admin nên nhập CostPrice thật cho từng sản phẩm.
   ================================================================ */

UPDATE dbo.Product
SET CostPrice =
    CASE
        WHEN PriceSale IS NOT NULL AND PriceSale > 0
            THEN CAST(PriceSale * 0.80 AS INT)
        WHEN Price > 0
            THEN CAST(Price * 0.80 AS INT)
        ELSE 0
    END
WHERE CostPrice <= 0
   OR CostPrice >=
      CASE
          WHEN PriceSale IS NOT NULL AND PriceSale > 0
              THEN PriceSale
          ELSE Price
      END;
GO

/* Kiểm tra logic giá */
SELECT
    Id,
    Name,
    CostPrice AS GiaNhap,
    Price AS GiaBanNiemYet,
    PriceSale AS GiaKhuyenMai,
    CASE
        WHEN PriceSale IS NOT NULL AND PriceSale > 0
            THEN PriceSale - CostPrice
        ELSE Price - CostPrice
    END AS LoiNhuanDuKien,
    CASE
        WHEN CostPrice > 0 THEN
            CAST(
                (
                    (
                        CASE
                            WHEN PriceSale IS NOT NULL AND PriceSale > 0
                                THEN PriceSale
                            ELSE Price
                        END
                    ) - CostPrice
                ) * 100.0 / CostPrice
                AS DECIMAL(10,2)
            )
        ELSE 0
    END AS TySuatLoiNhuanPhanTram
FROM dbo.Product
ORDER BY Id;
GO

/* ================================================================
   5. NỘI DUNG + ẢNH TIN TỨC CUỐI CÙNG
   ================================================================ */

/*
    GiftToi - Vá nội dung và ảnh tin tức
    Chạy trên đúng database BBEcom.
    Script có thể chạy lại nhiều lần: bài đã có sẽ được cập nhật,
    bài chưa có sẽ được thêm mới.
*/

USE [BBEcom];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

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

/* Chuẩn hóa alias cũ trước khi MERGE để không tạo bài trùng. */
UPDATE [dbo].[News]
SET Alias = N'huong-dan-mua-gift-card'
WHERE Alias = N'huong-dan-mua-ma-game-tu-dong';

UPDATE [dbo].[News]
SET Alias = N'thanh-toan-tren-gifttoi'
WHERE Alias = N'thanh-toan-vnpay-bbecom';

MERGE [dbo].[News] AS Target
USING @News AS Source
    ON Target.Alias = Source.Alias
WHEN MATCHED THEN
    UPDATE SET
        Target.Title = Source.Title,
        Target.Description = Source.Description,
        Target.Image = Source.Image,
        Target.CreateDate = Source.CreateDate
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Title, Description, Alias, Image, CreateDate)
    VALUES
    (
        Source.Title,
        Source.Description,
        Source.Alias,
        Source.Image,
        Source.CreateDate
    );

SELECT Id, Title, Alias, Image, CreateDate
FROM [dbo].[News]
ORDER BY CreateDate DESC, Id DESC;
GO

/* ================================================================
   6. KIỂM TRA CUỐI
   ================================================================ */

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
SELECT 'OrderDetail', COUNT(*) FROM dbo.OrderDetail;
GO

SELECT
    Id,
    UserName,
    Name,
    Email,
    IsAdmin,
    Status
FROM dbo.Account
ORDER BY Id;
GO

SELECT
    Id,
    Title,
    Alias,
    Image,
    CreateDate
FROM dbo.News
ORDER BY CreateDate DESC, Id DESC;
GO