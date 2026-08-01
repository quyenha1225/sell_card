GIFTTOI - NEWS IMAGE FIX
=========================

LỖI GỐC
-------
Ảnh thật nằm tại:
    Content\Client\img\news

Nhưng CommonHelper cũ lại tìm tại:
    Content\Client\img\blog

CÁC FILE ĐƯỢC ÁNH XẠ
--------------------
- bao-mat-tai-khoan.jpg
- google-play-apple.jpg
- huong-dan-mua-gift-card.jpg
- playstation-plus.jpg
- thanh-toan-vnpay.jpg
- xu-ly-code-loi.jpg

CÁCH ÁP DỤNG
------------
1. Giải nén ZIP vào thư mục gốc project:
       D:\CMC_Uni\HocKi_5\sell_card
2. Chọn Replace files in destination.
3. Nếu cần đồng bộ DB, chạy:
       Data\NEWS_IMAGE_SYNC.sql
4. Trong Visual Studio:
       Reload Project
       Build -> Clean Solution
       Build -> Rebuild Solution
5. Trình duyệt:
       Ctrl + Shift + R

KIỂM TRA
--------
Mở trực tiếp:
    /Content/Client/img/news/bao-mat-tai-khoan.jpg
    /Content/Client/img/news/playstation-plus.jpg

Nếu ảnh mở được thì đường dẫn tĩnh đã đúng.


CHẠY TỰ ĐỘNG
------------
Sau khi giải nén và chép đè, mở PowerShell tại thư mục project:

    Set-ExecutionPolicy -Scope Process Bypass
    .\apply_news_fix.ps1

Script sẽ:
- Include news-image-fix.css vào BBEcom.csproj.
- Include toàn bộ ảnh thật trong Content\Client\img\news.
- Gắn CSS News vào Views\Home\Index.cshtml.
