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