# Jenkins CI/CD cho SellCard

Luồng: Checkout đúng commit của job → Restore → Build Release → Test → Publish → lưu kết quả → Docker Build (tùy chọn) → kiểm tra nhánh → xét duyệt → backup IIS → deploy → kiểm tra HTTP. Nếu copy hoặc kiểm tra HTTP thất bại, script thử phục hồi file ứng dụng cũ; build vẫn báo thất bại.

## Thiết lập một lần

1. Dùng **Pipeline script from SCM**, repository của dự án, branch `*/quyen/admin`, Script Path `Jenkinsfile`. Có thể dùng Multibranch Pipeline; PR chỉ chạy CI. Không chạy pipeline PR không tin cậy trên agent có quyền production.
2. Pipeline hiện chọn **`built-in`**, node tích hợp trên máy Windows đang chạy Jenkins và IIS của bạn; không cần thêm label `sellcard-windows`. Node cần Online và có ít nhất một executor. Cài Git, .NET SDK tương thích project test, Visual Studio Build Tools với Web Build Tools, .NET Framework 4.7.2 targeting pack, VSTest và NuGet ở `C:\Tools\NuGet\nuget.exe`. Account Jenkins cần quyền ghi IIS và `C:\JenkinsBackups\BBEcom`. Khi chuyển sang agent riêng, đổi label trong Jenkinsfile sang agent đó.
3. Cài plugin **Pipeline**, **Git**, **JUnit**, **Pipeline: Input Step**, **Timestamper**, **Lockable Resources**. Tất cả job deploy cùng website phải dùng lock `sellcard-iis-production` và cùng agent; lock không bảo vệ thao tác copy thủ công bên ngoài Jenkins.
4. Giữ cấu hình thật tại `C:\inetpub\wwwroot\BBEcom\Web.config`. Chuẩn bị IIS và database trước. Đảm bảo `http://localhost/BBEcom/Home/Index` trả HTTP 200 trực tiếp, không redirect. Nếu khác, sửa `WEBSITE_URL` trong Jenkinsfile cho đúng địa chỉ kiểm tra nội bộ.
5. Tài khoản được duyệt mặc định là **`quyenha2512`**. Đổi `submitter` nếu username Jenkins thực tế khác. Giới hạn quyền Configure/Replay và quyền sửa Jenkinsfile; quản trị viên Jenkins vẫn có thể duyệt theo cơ chế của Jenkins.
6. Trên GitHub, bảo vệ nhánh `quyen/admin`: yêu cầu PR, reviewer và các CI checks trước merge. Jenkinsfile không tự bật branch protection hay webhook. Cấu hình GitHub webhook trong job/plugin nếu muốn tự chạy khi push/merge; không mặc định job sẽ tự chạy chỉ vì có Jenkinsfile.

## Chạy và duyệt

- Lần đầu chạy job để Jenkins nhận parameters. Mặc định chỉ CI, không thay website.
- Chọn **Build with Parameters → DEPLOY_IIS=true** để yêu cầu triển khai IIS. Sau khi CI thành công, người duyệt có 24 giờ để xem **Test Result**, **Artifacts**, commit và nhấn **Approve & Deploy**. Hủy hoặc hết hạn thì không bắt đầu deploy.
- Job giữ Windows executor trong thời gian chờ duyệt; nên dành agent riêng cho release. Build cùng job xếp hàng, không chạy chồng nhau.
- Mỗi build dùng workspace riêng `sellcard-runs/<BUILD_TAG>` dưới workspace root của agent. Sau khi lưu Artifacts, Jenkins thử dọn workspace; nếu file bị giữ hoặc thiếu quyền thì ghi cảnh báo và giữ thư mục để kiểm tra, không làm mất kết quả Build/Test. `buildDiscarder` không tự dọn các workspace còn sót này.
- Pipeline kiểm tra commit vẫn là đầu nhánh release trước và sau duyệt. Nếu nhánh đã có commit mới, phải chạy và duyệt bản mới.
- **BUILD_DOCKER=true** tạo image `sellcard:ci-<BUILD_NUMBER>` bằng script hiện có; cần Docker Windows containers. Tùy chọn này chỉ build image, chưa tự thay container đang chạy. Xem `README-DOCKER.md` để cấu hình SQL và Web.config ngoài image.

## Xem kết quả

Mở Jenkins → job → build `#...`:

| Mục | Nội dung |
| --- | --- |
| Console Output | Log restore/build/test/deploy; đường dẫn backup và kết quả phục hồi |
| Test Result | Danh sách test và lịch sử Passed/Failed, lấy từ báo cáo JUnit chuyển từ TRX |
| Artifacts → TestResults/SellCard.trx | Báo cáo MSTest gốc; có thể mở bằng Visual Studio |
| Artifacts → release/site/bin/BBEcom.dll | DLL sau Publish, có fingerprint để theo dõi |
| Artifacts → release-info.txt | Commit và SHA256 của DLL |
| Artifacts → release-approval.txt | Commit, người duyệt và URL build |

Test report được lưu ngay sau Test, DLL được lưu trước khi xét duyệt. Không archive cả thư mục publish vì có thể chứa Web.config bí mật. Publish chuyển từ thư mục dùng chung `C:\JenkinsDeploy\BBEcom` sang `<WORKSPACE>\release\site`, tránh các job ghi đè nhau. Nếu chạy Docker thủ công sau pipeline, truyền `-PublishDir` tương ứng.

## Giới hạn cần biết

- Test thất bại, bị skip, không có test hoặc thiếu TRX đều chặn release. `BBEcom.Tests/Test1.cs` hiện chứa test rỗng: Passed chưa chứng minh nghiệp vụ đúng. Cần bổ sung assertion và test nghiệp vụ trước khi coi đây là quality gate đủ cho production.
- Deploy giữ nguyên Web.config, App_Data, userfiles và uploads. Không thực hiện migration, tạo hoặc reset database. Copy `/E` giữ file cũ; chưa tự dọn tài nguyên thừa để tránh xóa dữ liệu riêng chưa được phân loại.
- Rollback phục hồi file ứng dụng và xóa file mới mà lần deploy vừa thêm; không phục hồi database/uploads. HTTP 200 chỉ kiểm tra khả năng phục vụ trang, chưa kiểm tra đầy đủ đăng nhập/thanh toán/SQL.
- Nếu rollback thất bại, site giữ `app_offline.htm`, cần quản trị viên phục hồi từ backup ghi trong log. Backup chứa cấu hình riêng: bảo vệ quyền NTFS và quản lý dung lượng/retention trên máy agent. Không đưa backup lên Artifacts.
- Mất điện hoặc kill tiến trình có thể ngăn rollback/finally chạy; phải kiểm tra site và backup thủ công. Chưa xác nhận deploy thực tế chỉ bằng việc chỉnh source.

Tham khảo: [JUnit reports](https://www.jenkins.io/doc/pipeline/steps/junit/), [Input và quyền người duyệt](https://plugins.jenkins.io/pipeline-input-step), [Artifacts và fingerprint](https://www.jenkins.io/doc/pipeline/steps/core/).
