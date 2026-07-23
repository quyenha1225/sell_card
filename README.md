# BBEcom

![GitHub repo size](https://img.shields.io/github/repo-size/dokimkhanh/BBEcom)
![GitHub stars](https://img.shields.io/github/stars/dokimkhanh/BBEcom?style=social)
![GitHub forks](https://img.shields.io/github/forks/dokimkhanh/BBEcom?style=social)
![GitHub issues](https://img.shields.io/github/issues/dokimkhanh/BBEcom)
[![License](https://img.shields.io/github/license/dokimkhanh/BBEcom)](https://opensource.org/licenses/MIT)

BBEcom là một ứng dụng quản lý bán hàng trực tuyến được xây dựng bằng **ASP.NET** với **Entity Framework**, hỗ trợ bán mã game tự động và tích hợp thanh toán qua **VNPay API**.

---

## 📑 Table of Contents

1. [Tính năng chính](#-tính-năng-chính)
2. [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
3. [Cài đặt](#-cài-đặt)
4. [Cấu hình cơ sở dữ liệu](#-cấu-hình-cơ-sở-dữ-liệu)
5. [Cấu hình VNPay](#-cấu-hình-vnpay)
6. [Chạy ứng dụng](#-chạy-ứng-dụng)
7. [Đóng góp](#-đóng-góp)
8. [Screenshots](#-screenshots)
9. [License](#-license)

---


## 🔹 Tính năng chính

* Quản lý sản phẩm, danh mục, và kho hàng.
* Tạo và quản lý hóa đơn mua hàng.
* Thanh toán trực tuyến qua VNPay.
* Cung cấp mã game tự động sau khi thanh toán thành công.
* Quản lý người dùng và quyền truy cập.

---

## 🔹 Yêu cầu hệ thống

* Windows 10/11 hoặc máy chủ Windows.
* [Visual Studio](https://visualstudio.microsoft.com/) (phiên bản hỗ trợ ASP.NET và Entity Framework).
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc SQL Server Express.
* .NET Framework 4.7.2 trở lên.

---

## 🔹 Cài đặt

1. Clone dự án:

```bash
git clone https://github.com/dokimkhanh/BBEcom.git
cd BBEcom
```

2. Mở dự án trong **Visual Studio**.
3. Cài đặt các package cần thiết:
---

## 🔹 Cấu hình cơ sở dữ liệu

1. Tạo cơ sở dữ liệu mới trong SQL Server.
2. Mở file `Web.config` và chỉnh sửa chuỗi kết nối:

```xml
<connectionStrings>
  <add name="KeyDbContext" 
       connectionString="Data Source=SERVER_NAME;Initial Catalog=BBEcom;Integrated Security=True;MultipleActiveResultSets=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> Lưu ý: `Integrated Security=True` sử dụng Windows Authentication.

3. Chạy **Entity Framework Migration** để tạo các bảng (nếu cần):

```powershell
Update-Database
```

---

## 🔹 Cấu hình VNPay

1. Cấu hình trong `Web.config` hoặc thêm biến môi trường trong ứng dụng:

```
vnp_TmnCode=YOUR_MERCHANT_CODE
vnp_HashSecret=YOUR_HASH_SECRET
vnp_Returnurl=http://localhost:PORT/return
```

* Thay `YOUR_MERCHANT_CODE` và `YOUR_HASH_SECRET` bằng thông tin từ VNPay.
* `vnp_Returnurl` là URL xử lý kết quả thanh toán.

---

## 🔹 Chạy ứng dụng

1. Trong Visual Studio, chọn **IIS Express** hoặc cấu hình server.
2. Nhấn **F5** để chạy ứng dụng.
3. Truy cập URL hiển thị trên trình duyệt, ví dụ: `http://localhost:5000`.

---

## 🔹 Đóng góp

* Fork repository.
* Tạo branch mới cho tính năng hoặc sửa lỗi.
* Tạo pull request để hợp nhất vào `master`.

---

## 🔹 Screenshots
![Main](https://i.imgur.com/9MrELOX.png)