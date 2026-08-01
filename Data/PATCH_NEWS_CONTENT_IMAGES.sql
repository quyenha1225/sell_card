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
