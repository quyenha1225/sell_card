/* GIFTTOI - Đồng bộ đúng ảnh News đang có trong Content/Client/img/news
   Hãy backup database trước khi UPDATE. */

SELECT Id, Title, Alias, Image
FROM News
ORDER BY Id;

/* 1. Thanh toán */
UPDATE News
SET Image = '/Content/Client/img/news/thanh-toan-vnpay.jpg'
WHERE Alias LIKE '%thanh-toan%'
   OR Title LIKE N'%Thanh toán%';

/* 2. Bảo mật tài khoản */
UPDATE News
SET Image = '/Content/Client/img/news/bao-mat-tai-khoan.jpg'
WHERE Alias LIKE '%bao-mat%'
   OR Title LIKE N'%bảo mật%';

/* 3. PlayStation Plus */
UPDATE News
SET Image = '/Content/Client/img/news/playstation-plus.jpg'
WHERE Alias LIKE '%playstation-plus%'
   OR Title LIKE N'%PlayStation Plus%';

/* 4. Hướng dẫn mua gift card */
UPDATE News
SET Image = '/Content/Client/img/news/huong-dan-mua-gift-card.jpg'
WHERE Alias LIKE '%huong-dan-mua%'
   OR Title LIKE N'%Hướng dẫn mua%';

/* 5. Google Play / Apple */
UPDATE News
SET Image = '/Content/Client/img/news/google-play-apple.jpg'
WHERE Alias LIKE '%google-play%'
   OR Alias LIKE '%apple%'
   OR Title LIKE N'%Google Play%'
   OR Title LIKE N'%Apple%';

/* 6. Xử lý code lỗi */
UPDATE News
SET Image = '/Content/Client/img/news/xu-ly-code-loi.jpg'
WHERE Alias LIKE '%xu-ly-code%'
   OR Title LIKE N'%code lỗi%'
   OR Title LIKE N'%xử lý code%';

SELECT Id, Title, Alias, Image
FROM News
ORDER BY Id;
