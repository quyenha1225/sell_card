/* GiftToi Part 1 - kiểm tra và đồng bộ đường dẫn ảnh.
   Chạy trên đúng database. Hãy backup trước khi UPDATE. */

SELECT DB_NAME() AS CurrentDatabase;

SELECT Id, Name, Alias, Images
FROM Product
ORDER BY Id;

/* Chuẩn hóa thư mục Images cũ thành img. */
UPDATE Product
SET Images = REPLACE(
    Images,
    '/Content/Client/Images/',
    '/Content/Client/img/'
)
WHERE Images LIKE '%/Content/Client/Images/%';

/* Ví dụ cập nhật theo Alias:
UPDATE Product
SET Images = '/Content/Client/img/products/' + Alias + '.png'
WHERE Alias IS NOT NULL
  AND Alias <> '';
*/

/* Tin tức */
SELECT Id, Title, Alias, Image
FROM News
ORDER BY Id;

UPDATE News
SET Image = REPLACE(
    Image,
    '/Content/Client/Images/',
    '/Content/Client/img/'
)
WHERE Image LIKE '%/Content/Client/Images/%';
