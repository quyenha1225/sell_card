# GiftToi — Website bán Gift Card

GiftToi là website thương mại điện tử bán gift card và mã sản phẩm số, xây dựng bằng ASP.NET MVC 5 trên .NET Framework 4.7.2. Hệ thống gồm storefront, tài khoản khách hàng, giỏ hàng, mua ngay, thanh toán SePay, cấp code tự động, lịch sử giao dịch và trang quản trị.

## Tính năng

### Khách hàng

- Xem sản phẩm theo danh mục, sản phẩm nổi bật và tin tức.
- Xem giá bán, giá khuyến mại và số code còn trong kho.
- Đăng ký, đăng nhập và duy trì đăng nhập bằng Forms Authentication.
- Thêm nhiều sản phẩm vào giỏ hàng và điều chỉnh số lượng.
- “Mua ngay” một sản phẩm và chuyển thẳng tới cổng thanh toán SePay.
- Thanh toán bằng chuyển khoản QR qua SePay.
- Tự động duyệt đơn và lấy code khỏi kho sau khi thanh toán thành công.
- Xem trạng thái đơn, code đã mua và sao chép code tại `/lich-su-mua-hang`.

### Quản trị

- Quản lý tài khoản, danh mục, sản phẩm, gift code, tin tức và FAQ.
- Theo dõi dữ liệu bán hàng từ khu vực `/Admin`.
- Chỉ tài khoản có `IsAdmin = true` mới truy cập được trang quản trị.

### Xác nhận thanh toán

Hệ thống dùng hai cơ chế:

1. SePay gửi IPN đến website khi đơn được thanh toán.
2. Khi khách được chuyển về website, server kiểm tra lại đơn qua API SePay nếu IPN chưa tới.

Đơn chỉ được duyệt khi mã hóa đơn, tiền tệ, số tiền và trạng thái `CAPTURED` khớp. Việc lấy code và cập nhật trạng thái được thực hiện trong database transaction để tránh cấp trùng code.

## Công nghệ

- ASP.NET MVC 5.2.9
- .NET Framework 4.7.2
- Entity Framework 6.4.4 Code First
- Microsoft SQL Server
- IIS Express
- Razor, Bootstrap 5, jQuery và JavaScript
- PagedList.Mvc
- CKEditor/CKFinder cho nội dung quản trị
- SePay Payment Gateway

Project chạy trực tiếp trên Windows. Không cần Node.js và không cần `npm install`; các thư viện frontend đã nằm trong source.

## Yêu cầu hệ thống

- Windows 10/11 hoặc Windows Server.
- Visual Studio 2022 với workload **ASP.NET and web development**.
- .NET Framework 4.7.2 SDK và Targeting Pack.
- SQL Server 2019/2022, SQL Server Express hoặc LocalDB.
- SQL Server Management Studio — không bắt buộc nhưng nên có.
- IIS Express.
- Cloudflared — chỉ cần khi thử IPN SePay từ máy local.

## Cài đặt

### 1. Mở solution

Clone repository, sau đó mở:

```text
BBEcom.sln
```

Nên mở solution thay vì mở riêng `BBEcom.csproj`.

### 2. Khôi phục NuGet package

Trong Visual Studio, nhấn chuột phải vào solution và chọn **Restore NuGet Packages**. Các package được khai báo trong `packages.config`.

### 3. Cấu hình database

Mở `Web.config` và thay chuỗi kết nối `KeyDbContext`:

```xml
<connectionStrings>
  <add name="KeyDbContext"
       connectionString="Server=.\SQLEXPRESS;Database=BBEcom;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Tên server thường dùng:

| SQL Server | Giá trị `Server` |
|---|---|
| SQL Server Express | `.\SQLEXPRESS` |
| LocalDB | `(localdb)\MSSQLLocalDB` |
| Default instance | `localhost` hoặc `.` |
| Instance tùy chỉnh | `TEN_MAY\TEN_INSTANCE` |

Chuỗi ví dụ sử dụng Windows Authentication. Tài khoản Windows chạy Visual Studio cần có quyền tạo và truy cập database.

### 4. Tạo database

Mở **Tools → NuGet Package Manager → Package Manager Console**, chọn Default project là `BBEcom`, rồi chạy:

```powershell
Update-Database
```

Migration trong thư mục `Migrations` sẽ tạo schema database. Nếu cần dữ liệu mẫu, kiểm tra script `Data/seed.sql` trước khi chạy trên đúng database.

### 5. Chạy bằng Visual Studio

1. Chọn cấu hình `Debug`.
2. Chọn `IIS Express`.
3. Nhấn `F5` hoặc `Ctrl + F5`.

SSL port trong project hiện được cấu hình là:

```text
https://localhost:44315
```

Visual Studio có thể cấp port khác tùy máy.

## Chạy bằng dòng lệnh

### Build

Trong Developer PowerShell for Visual Studio:

```powershell
msbuild BBEcom.sln /t:Build /p:Configuration=Debug
```

Hoặc dùng đường dẫn Visual Studio 2022 Community:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" BBEcom.sln /t:Build /p:Configuration=Debug
```

### Chạy IIS Express

Ví dụ chạy tại cổng `5050`:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\duong-dan\den\sell_card" /port:5050
```

Truy cập:

```text
http://localhost:5050
```

Không đóng tiến trình IIS Express khi đang sử dụng website.

## Cấu hình SePay

### App settings

Không đưa credential thật vào source control. Cấu hình trong `Web.config` theo mẫu:

```xml
<add key="SePayEnvironment" value="sandbox" />
<add key="SePayMerchantId" value="YOUR_MERCHANT_ID" />
<add key="SePaySecretKey" value="YOUR_MERCHANT_SECRET_KEY" />
<add key="SePayPublicBaseUrl" value="https://YOUR_PUBLIC_HTTPS_DOMAIN" />
```

Giá trị `SePayEnvironment`:

| Môi trường | Giá trị |
|---|---|
| Thử nghiệm | `sandbox` |
| Thực tế | `production` |

`SePayPublicBaseUrl` chỉ chứa domain gốc, không thêm `/thanh-toan/sepay/ipn`.

Đúng:

```text
https://example.trycloudflare.com
```

Sai:

```text
https://example.trycloudflare.com/thanh-toan/sepay/ipn
```

### IPN trên Merchant SePay

Cấu hình:

```text
URL: https://YOUR_PUBLIC_HTTPS_DOMAIN/thanh-toan/sepay/ipn
Method: POST
Content-Type: application/json
Authentication: SECRET_KEY
```

Secret Key ở Merchant SePay phải trùng `SePaySecretKey`. Endpoint hợp lệ trả `HTTP 200` khi payload và xác thực đúng.

### Cloudflare Tunnel khi chạy local

IIS Express chỉ chấp nhận hostname local. Vì vậy phải đặt lại Host header khi tạo tunnel:

```powershell
cloudflared tunnel --url http://127.0.0.1:5050 --http-host-header localhost:5050
```

Cloudflared sẽ trả một URL dạng:

```text
https://random-words.trycloudflare.com
```

Sau đó:

1. Gán URL này vào `SePayPublicBaseUrl`.
2. Cấu hình IPN thành `https://random-words.trycloudflare.com/thanh-toan/sepay/ipn`.
3. Khởi động lại website nếu vừa thay `Web.config`.
4. Gửi test IPN từ dashboard SePay.

Quick Tunnel không có uptime guarantee và tạo domain mới sau mỗi lần chạy lại. Mỗi lần domain đổi phải cập nhật cả `Web.config` lẫn Merchant SePay. Production nên dùng domain cố định, hosting thật hoặc Cloudflare Named Tunnel.

### Luồng thanh toán

```text
Khách chọn sản phẩm
  → Thêm vào giỏ hoặc Mua ngay
  → Website tạo đơn chờ thanh toán
  → Chuyển sang SePay
  → Khách thanh toán
  → SePay gửi IPN
  → Website đối chiếu mã đơn và số tiền
  → Duyệt đơn + lấy gift code trong transaction
  → Khách xem code tại Lịch sử giao dịch
```

Nếu redirect về website xảy ra trước IPN, action thành công sẽ gọi API SePay để kiểm tra lại đơn. Trang kết quả cũng kiểm tra trạng thái định kỳ trong thời gian ngắn.

## Các URL chính

| Chức năng | URL |
|---|---|
| Trang chủ | `/` |
| Đăng nhập | `/dang-nhap` |
| Đăng ký | `/dang-ky` |
| Trang cá nhân | `/trang-ca-nhan` |
| Giỏ hàng | `/gio-hang` |
| Mua ngay | `/mua-ngay` — POST |
| Thanh toán | `/thanh-toan` |
| Lịch sử giao dịch | `/lich-su-mua-hang` |
| Tin tức | `/tin-tuc` |
| IPN SePay | `/thanh-toan/sepay/ipn` — POST |
| Kết quả SePay | `/thanh-toan/sepay/thanh-cong` |
| Quản trị | `/Admin` |

## Hiệu năng

- `compilation debug` được tắt trong `Web.config` để tránh overhead khi chạy.
- IIS bật nén response và cache file tĩnh trong 7 ngày.
- Menu header được cache 5 phút.
- Lịch sử giao dịch tải order, order detail và product trong một truy vấn eager-loading.
- CSS/JavaScript tùy chỉnh dùng query version để tránh trình duyệt giữ giao diện cũ sau khi cập nhật.

Lần truy cập đầu sau khi build hoặc IIS recycle có thể chậm vì ASP.NET khởi động application và biên dịch Razor. Các request sau sẽ nhanh hơn. Quick Tunnel luôn có thêm network latency và không phản ánh chính xác tốc độ của application local.

## Kiểm thử sau khi cài

1. Mở trang chủ và một trang chi tiết sản phẩm.
2. Đăng ký và đăng nhập tài khoản khách hàng.
3. Thêm sản phẩm vào giỏ và kiểm tra số lượng trên header.
4. Nhấn “Mua ngay” và xác nhận website chuyển thẳng sang SePay.
5. Thanh toán sandbox, kiểm tra trang kết quả chuyển sang thành công.
6. Mở `/lich-su-mua-hang`, xác nhận đơn đã thanh toán và có code.
7. Kiểm tra số gift code trong kho đã giảm đúng số lượng.
8. Gửi lại cùng một IPN và xác nhận hệ thống không cấp code lần thứ hai.
9. Đăng nhập tài khoản admin và mở `/Admin`.

## Xử lý lỗi

### `400 Bad Request — Invalid Hostname`

Tunnel đang gửi public hostname vào IIS Express. Chạy lại cloudflared với:

```powershell
--http-host-header localhost:5050
```

### Đơn đứng ở “Đang xác nhận thanh toán”

Kiểm tra:

- IPN URL trên SePay có đúng domain hiện tại không.
- Quick Tunnel có còn chạy không.
- Auth type có phải `SECRET_KEY` không.
- Secret Key trên SePay và `Web.config` có trùng không.
- SePay có nhận đơn là `CAPTURED` không.
- Endpoint IPN có trả `HTTP 200` không.
- Kho còn đủ gift code để cấp không.

### Đăng nhập rồi vẫn bị yêu cầu đăng nhập

- Phải đăng nhập và mua hàng trên cùng một hostname.
- Cookie của `localhost` không dùng chung với domain `trycloudflare.com`.
- Không đổi domain tunnel giữa lúc đăng nhập và thanh toán.
- Kiểm tra trình duyệt có chặn cookie không.

### Website chậm

- Thử trực tiếp bằng URL local để tách độ trễ ứng dụng khỏi tunnel.
- Lần đầu sau build/recycle là cold start và sẽ chậm hơn.
- Không dùng Quick Tunnel để đánh giá hiệu năng production.
- Kiểm tra SQL Server, truy vấn database và các request bên ngoài trong Network tab.

### Không kết nối được SQL Server

- Kiểm tra service SQL Server trong `services.msc`.
- Kiểm tra instance trong connection string.
- Thử cùng thông tin bằng SSMS.
- Đảm bảo Windows user có quyền với database `BBEcom`.

### `Update-Database` không được nhận diện

- Restore NuGet package.
- Chọn đúng Default project `BBEcom`.
- Build solution rồi chạy lại lệnh.

### Thiếu .NET Framework 4.7.2

Cài trong Visual Studio Installer:

```text
.NET Framework 4.7.2 SDK
.NET Framework 4.7.2 Targeting Pack
```

### Cảnh báo thiếu CKFinder DLL

Project cũ có một số CKFinder reference trỏ tới DLL không còn trong source. Phần storefront và thanh toán vẫn có thể build. Muốn sử dụng đầy đủ upload trong Admin cần cài đúng phiên bản CKFinder có giấy phép và cập nhật `HintPath` trong `BBEcom.csproj`.

## Cấu trúc thư mục

```text
App_Start/             Route, filter và bundle configuration
Areas/Admin/           Controllers, views và assets trang quản trị
Common/                Session model, helper, mã hóa và tiện ích
Controllers/           Controllers phía khách hàng
Models/DAO/            Data access layer
Models/EF/             Entity Framework entities và DbContext
Models/ViewModels/     View models cho thanh toán
Migrations/            Entity Framework migrations
Views/                 Razor views storefront
Content/Client/        CSS, JavaScript, font và hình storefront
Content/Admin/         CSS, JavaScript và CKEditor/CKFinder Admin
Data/                  Script và dữ liệu hỗ trợ
Web.config             Database, authentication, SePay và IIS config
```

## Bảo mật

- Không commit Merchant Secret Key, API key hoặc credential production.
- Nếu secret từng xuất hiện trong Git history, ảnh chụp hoặc log, hãy thu hồi và tạo key mới.
- Không duyệt đơn chỉ dựa vào URL redirect từ trình duyệt.
- Luôn xác nhận thanh toán qua IPN hoặc API SePay.
- Chỉ cấp code sau khi đối chiếu đúng đơn, tiền tệ và số tiền.
- Production phải dùng HTTPS và domain cố định.

## License

Xem [LICENSE.txt](LICENSE.txt).


## Current Jenkins pipeline

See [README-JENKINS.md](README-JENKINS.md) for current setup, approval, Build/Test reports and rollback. Publish output is now `<WORKSPACE>\release\site`. Default runs perform CI only; enable `DEPLOY_IIS` to request an approved IIS release or `BUILD_DOCKER` to build an image.

The IIS/OAuth notes below describe the previous pipeline; shared publish paths and old stage numbering are superseded by README-JENKINS.md.

## CI/CD Jenkins → IIS (một source dùng cho cả IIS Express và IIS)

**Không gỡ IIS, không xóa database `BBEcom`, không sửa code trực tiếp trong `C:\inetpub\wwwroot\BBEcom`.**

- **Source:** `C:\Users\Admin\Documents\GitHub\sell_card` (đẩy lên GitHub).
- **Publish trung gian:** `C:\JenkinsDeploy\BBEcom` (Jenkins tạo mỗi lần build).
- **Website IIS:** `C:\inetpub\wwwroot\BBEcom` (chỉ Jenkins triển khai).
- **Backup tự động:** `C:\JenkinsBackups\BBEcom\<BUILD_NUMBER>` (chứa cả Web.config và dữ liệu riêng, bảo vệ quyền truy cập).

### Trình tự thiết lập một lần

1. Đảm bảo `Default Web Site` có IIS Application `/BBEcom`, app pool .NET CLR v4.0, Integrated; SQL Server `SQLEXPRESS` đã chạy. Không tạo hoặc reset database khi Deploy.
2. Sao lưu **Web.config trên IIS** và xác nhận nó dùng `Server=.\SQLEXPRESS;Database=BBEcom;Integrated Security=True` và các credentials riêng. File cấu hình này được Jenkins **giữ nguyên**, không tự chép đè.
3. Cấu hình HTTPS cho IIS nếu muốn đăng nhập bằng cookie `Secure` / `requireSSL=true`. URL HTTP localhost chỉ phục vụ kiểm tra giao diện ban đầu.
4. Chép `LoginController.cs` đã sửa vào source, giữ các tài nguyên dùng `Url.Content("~/...")` hoặc `Url.Action(...)`, build thử IIS Express. **Không đưa mật khẩu, API key hoặc `Web.config` thật vào GitHub.**
5. Thay `Jenkinsfile` trong source bằng bản mới rồi commit các file code. Trên GitHub, bảo vệ nhánh `quyen/admin` bằng Pull Request + review trước merge.
6. Trong Jenkins job `BanPhanMemBanQuyen-CICD`, dùng *Pipeline script from SCM*, nhánh `*/quyen/admin`, đường dẫn `Jenkinsfile`; cấu hình GitHub webhook trigger. GitHub không thể gọi URL `localhost:8080`, cần endpoint HTTPS công khai được bảo vệ (ví dụ tunnel phù hợp).
7. Khi PR được duyệt và merge: GitHub kích hoạt CI; sau Build/Test/Publish, Jenkins chờ tài khoản `quyenha2512` nhấn **Approve & Deploy** rồi sao lưu website, Deploy và Verify.
8. Sau Deploy, kiểm tra `http://localhost/BBEcom/Home/Index`, trang chi tiết, giỏ hàng và database. Khi đã có HTTPS, thử đăng nhập Google thực tế.

### Redirect URI Google OAuth

Đăng ký **Authorized redirect URIs** (không phải *Authorized JavaScript origins*) trong đúng OAuth Client `GiftToi Web`:

- IIS Express: `https://localhost:44315/Login/GoogleCallback`
- IIS HTTP local: `http://localhost/BBEcom/Login/GoogleCallback`
- Nếu cấu hình IIS HTTPS: `https://localhost/BBEcom/Login/GoogleCallback` (đúng port/binding thực tế)

`BuildGoogleRedirectUri()` trong controller sẽ sinh callback theo request và virtual path khi chạy localhost; không dùng `GoogleRedirectUri` cố định nữa. Cả request authorize và token exchange sử dụng cùng hàm. Khi test reverse proxy/tunnel thực tế cần cấu hình hostname + proxy tin cậy, không tùy tiện mở tin cậy mọi header forwarded từ Internet.

### Bảo vệ cấu hình và dữ liệu

- File `LOCAL_ONLY_DO_NOT_COMMIT/Web.config` (nếu bạn nhận kèm bản vá) chứa cấu hình **riêng do bạn cung cấp**. Chỉ copy vào máy phát triển nếu cần, không commit. Bản Web.config của IIS vẫn giữ nguyên trong quá trình Deploy.
- **Không chạy `git add .` mù quáng.** Chỉ `git add Jenkinsfile Controllers/LoginController.cs README.md` và các file source đã chủ động kiểm tra; dùng `git status --staged` trước khi commit.
- File `Web.config.EXAMPLE_NOT_FOR_RUNTIME` đã ẩn secret và **không dùng để chạy thực tế**.
- Nếu các khóa JWT/SMTP/Google/SePay đã bị commit/chia sẻ, phải thay mới và loại khỏi lịch sử/nguồn public theo quy trình bảo mật.
- Jenkins `/E` chỉ copy cập nhật, **không xóa file cũ**; để dọn sạch tệp stale phải lên kế hoạch đồng bộ chọn lọc và sao lưu, không chạy `/MIR` khi chưa tách dữ liệu người dùng.
- Việc dùng cùng database giữa IIS Express và IIS trên máy học tập giúp dữ liệu đồng nhất, nhưng trong sản xuất nên tách database test và production.

### Hoàn tác nếu bản mới gặp lỗi

Vào backup `C:\JenkinsBackups\BBEcom\<BUILD_NUMBER>` tương ứng, cho website tạm offline rồi phục hồi bản sao lưu, **không xóa SQL**. Chỉ người quản trị máy/IIS được thao tác. Nếu dữ liệu đã thay đổi sau thời điểm backup, kiểm tra kỹ trước khi khôi phục các thư mục người dùng.
