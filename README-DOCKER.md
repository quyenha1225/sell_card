# SellCard – Windows Docker starter (không chứa secret)

## Mục tiêu

- IIS Windows hiện tại tiếp tục chạy ở `http://localhost/BBEcom`.
- Docker thử nghiệm riêng ở `http://localhost:8090`.
- SQL Server `BBEcom` giữ nguyên trên Windows, không tạo/xóa/reset database.
- Docker image chứa bản Publish, KHÔNG chứa `Web.config` bí mật.

## 1. Điều kiện trước khi chạy

1. Windows 11 Pro/Enterprise, Docker Desktop cài kiểu all-users và chạy **Windows containers**. Windows Home không chạy Windows containers với Docker Desktop.
2. Docker Desktop đã mở; `docker info --format '{{.OSType}}'` trả về `windows`.
3. Jenkins đã Publish website vào `C:\JenkinsDeploy\BBEcom` với `bin\BBEcom.dll`.
4. Image cần dung lượng trống đáng kể, và bản Windows Server Core ltsc2022 tương thích host Windows 11.

## 2. Chép file vào source (không chép vào inetpub)

- `deploy/docker/Dockerfile`
- `deploy/docker/start.ps1`
- `scripts/build-docker.ps1`
- `scripts/run-docker.ps1`

Thư mục source là `C:\Users\Admin\Documents\GitHub\sell_card`.

## 3. Tạo image (chưa chạy website)

Trong PowerShell tại thư mục source:

```powershell
.\scripts\build-docker.ps1

docker images sellcard
```

Image dùng Microsoft ASP.NET Framework 4.8.1 + IIS. Script lấy website từ Jenkins Publish, loại trừ Web.config khỏi context.

## 4. Cấu hình Web.config RIÊNG cho Docker

**Không đưa khóa thật vào GitHub, ZIP hay image.** Tạo trên máy:

```powershell
New-Item -ItemType Directory -Force C:\SellCardDocker\config
Copy-Item 'C:\inetpub\wwwroot\BBEcom\Web.config' 'C:\SellCardDocker\config\Web.config'
```

File này vẫn có secret nên chỉ để nội bộ máy. Thay lại các khóa đã từng bị lộ.

### SQL Server

`Server=.\SQLEXPRESS;Integrated Security=True` trong bản IIS KHÔNG dùng nguyên được trong container. `.` là chính container, không phải máy host.

Nếu dùng SQL Auth local: cần bật TCP/IP cho instance SQLEXPRESS, đặt cổng cố định và cấu hình firewall chỉ cho mạng Docker/trusted local. Tạo SQL login chỉ có quyền ứng dụng cần với database BBEcom, KHÔNG dùng sa. Ví dụ format, điền **mật khẩu riêng trên máy**:

```xml
<add name="KeyDbContext"
  connectionString="Server=tcp:host.docker.internal,14330;Database=BBEcom;User ID=SellCardDocker;Password=YOUR_LOCAL_PRIVATE_PASSWORD;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True"
  providerName="System.Data.SqlClient" />
```

Cổng `14330` là ví dụ nếu bạn đã đặt đúng trong SQL Server Configuration Manager. Hãy kiểm tra TCP từ container nếu kết nối lỗi. Không khuyến khích `TrustServerCertificate=True` trên production: cấu hình chứng chỉ TLS đáng tin cậy.

### Google callback

Docker chạy ở root của cổng 8090, không phải `/BBEcom`, nên callback trong Google Console (GiftToi Web → Authorized redirect URIs) là:

`http://localhost:8090/Login/GoogleCallback`

Nếu source controller đã tự sinh callback theo request, không đặt URI cứng cho môi trường khác.

### HTTPS/cookies

Web.config hiện có thể đặt `requireSSL="true"`. Qua `http://localhost:8090` cookie Secure không hoạt động, nên đăng nhập có thể thất bại ngay cả khi trang chủ hiển thị. Nên cấu hình HTTPS cục bộ hoặc reverse proxy trước khi kiểm thử OAuth/thanh toán. Không chuyển sang thanh toán production trên HTTP.

### Dữ liệu tải lên

Ảnh/file người dùng tải lên (CKFinder/App_Data nếu có) không nằm trong image. Nếu chức năng cần chúng, tạo một volume/bind-mount riêng cho dữ liệu và cấp quyền tối thiểu. Không đưa dữ liệu này vào layer image.

## 5. Chạy container khi đã chuẩn bị cấu hình SQL

```powershell
.\scripts\run-docker.ps1

docker ps
docker logs sellcard-web
```

Thử: `http://localhost:8090/Home/Index`.

Nếu báo xung đột tên container `sellcard-web`, xem `docker ps -a`; không xóa dữ liệu/volume để giải quyết lỗi này. Có thể chạy với tên khác qua `-ContainerName sellcard-web-v2` và cổng khác via `-Port 8091`.

## 6. Gắn Jenkins CI/CD sau khi Docker chạy thủ công thành công

Chưa tự chỉnh Jenkinsfile. Trong Jenkinsfile đang có các stage `Checkout → Restore → Build → Test → Publish → Deploy → Verify`, có thể thêm stage sau **Publish**:

```
stage('Docker Build') {
    steps {
        powershell '''
            & "$env:WORKSPACE\\scripts\\build-docker.ps1" `
                -PublishDir $env:PUBLISH_DIR `
                -ImageTag "sellcard:ci-$env:BUILD_NUMBER"
            if ($LASTEXITCODE -ne 0) { throw 'Docker image build failed' }
        '''
    }
}
```

Nếu Jenkins là Windows Service, agent thực thi pipeline cần quyền sử dụng Windows Docker daemon; KHÔNG công khai Docker daemon không xác thực qua cổng TCP. Bước Docker Deploy tự động và rollback chỉ bổ sung sau khi container và SQL đã hoạt động ổn định.

## Lưu ý

Các script và Dockerfile được kiểm tra nội dung tĩnh, nhưng CHƯA được thực thi trên Docker Windows máy bạn.
