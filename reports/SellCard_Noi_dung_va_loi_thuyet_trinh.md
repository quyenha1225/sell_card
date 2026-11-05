# SellCard — Nội dung slide và lời thuyết trình
Bản chỉnh sửa ngày 05/11/2026. Giữ bố cục 15 slide gốc, cập nhật theo source tại workspace. Lời thuyết trình cũng được lưu trong Notes của PowerPoint.
Lưu ý sử dụng: các kịch bản demo là tiêu chí cần thực hiện. Không xem cấu hình pipeline, test rỗng hay Dockerfile là bằng chứng đã chạy thành công. Chưa chạy Jenkins, IIS, Docker hoặc kiểm thử thanh toán trong lần biên soạn này.

## Slide 1

### Nội dung trên slide

- BÁO CÁO ĐỒ ÁN / TRIỂN KHAI PHẦN MỀM
- SELLCARD
- WEBSITE BÁN GIFT CARD & QUY TRÌNH CI/CD
- GitHub · Jenkins · MSTest / VSTest · IIS · Docker Windows
- ASP.NET MVC 5 · .NET Framework 4.7.2 · SQL Server
- Repository: github.com/quyenha1225/sell_card
- BÁO CÁO MÔN HỌC  •  11 / 2026
- CI / CD
- DELIVERY

### Lời thuyết trình

Đề tài trình bày website SellCard, giao diện mang tên GiftToi, cùng quy trình triển khai từ GitHub sang IIS. Ứng dụng dùng ASP.NET MVC 5 trên .NET Framework 4.7.2. Phần chính là CI/CD trên Windows; Docker là hướng mở rộng đã có cấu hình. Báo cáo phân biệt phần đã viết trong mã nguồn với kết quả cần có log vận hành để xác nhận.

## Slide 2

### Nội dung trên slide

- BỐI CẢNH
- Bài toán và yêu cầu triển khai SellCard
- Website bán mã số cần cập nhật đúng phiên bản, bảo toàn cấu hình và dữ liệu đơn hàng.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /02
- 01
- Lấy source
- 02
- Restore
- 03
- Build
- 04
- Test
- 05
- Publish
- 06
- Copy IIS
- Tốn thời gian
- Restore, build và copy thủ công lặp lại; kết quả dễ khác nhau giữa các lần triển khai.
- Rủi ro chất lượng
- Bỏ sót kiểm thử, thiếu DLL hoặc ghi đè cấu hình có thể làm gián đoạn mua hàng.
- Khó truy vết
- Cần gắn bản phát hành với commit, log pipeline và báo cáo kiểm thử để tra cứu.
- MỤC TIÊU: TỰ ĐỘNG BUILD / TEST, PHÊ DUYỆT PHÁT HÀNH VÀ XÁC MINH SAU DEPLOY.

### Lời thuyết trình

Khi triển khai thủ công, người thực hiện phải lấy đúng source, restore, build, test, publish rồi copy sang IIS. Với website bán mã số, một lỗi triển khai có thể làm gián đoạn thanh toán hoặc giao mã. Vì vậy quy trình cần kiểm soát phiên bản, bảo toàn cấu hình riêng và có thông tin truy vết từng lần chạy. Không đưa số liệu tiết kiệm thời gian khi chưa đo thực tế.

## Slide 3

### Nội dung trên slide

- ĐỊNH HƯỚNG
- Mục tiêu, phạm vi và lựa chọn kỹ thuật
- Chuẩn hóa triển khai SellCard từ mã nguồn GitHub đến website IIS trên Windows.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /03
- Mục tiêu CI/CD
- Tự động build và test; phát hành có phê duyệt, sao lưu website và kiểm tra HTTP sau triển khai.
- Phạm vi thực hành
- IIS là đích triển khai chính; Docker Windows có Dockerfile và script, chưa tích hợp vào pipeline.
- LỰA CHỌN KỸ THUẬT
- Ứng dụng
- MVC 5 / .NET Framework 4.7.2 → MSBuild + IIS CLR v4
- Pipeline hiện tại
- Nhánh quyen/admin · 10 stage · phê duyệt trước Deploy
- Phạm vi báo cáo: mã nguồn và cấu hình hiện có; kết quả vận hành cần log hoặc ảnh demo.

### Lời thuyết trình

Mục tiêu là chuẩn hóa các bước từ commit đến phát hành. Jenkinsfile hiện lấy nhánh quyen/admin và định nghĩa 10 stage. Sau CI, hệ thống yêu cầu người có quyền phê duyệt trước khi sao lưu và triển khai. Đây là continuous delivery có phê duyệt thủ công. Dự án dùng .NET Framework nên công cụ và cấu hình IIS cần phù hợp MVC 5; không dùng giả định của ASP.NET Core.

## Slide 4

### Nội dung trên slide

- HỆ THỐNG ỨNG DỤNG
- Tổng quan SellCard / GiftToi
- Website bán gift card, mã sản phẩm số; chức năng sử dụng để minh họa triển khai end-to-end.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /04
- KHÁCH HÀNG
- Danh mục · giỏ hàng · mua ngay / Đăng nhập thường / Google / Lịch sử đơn · nhận mã đã mua
- QUẢN TRỊ
- Tài khoản · danh mục · sản phẩm / Kho gift code · tin tức · FAQ / Theo dõi dữ liệu bán hàng
- THANH TOÁN
- Xác nhận qua IPN hoặc API / Đối chiếu đơn, tiền, CAPTURED / Transaction chống cấp mã lặp
- LUỒNG MUA HÀNG
- Chọn sản phẩm
- Tạo đơn
- SePay
- IPN / đối soát
- Cấp gift code
- Đơn hợp lệ → cấp mã từ kho → lưu OrderDetail → cập nhật trạng thái trong cùng transaction.

### Lời thuyết trình

Khách hàng xem sản phẩm, đăng nhập, mua ngay hoặc dùng giỏ hàng, thanh toán và nhận mã trong lịch sử đơn. Quản trị viên quản lý sản phẩm, tài khoản và kho code. LoginController còn có Google OAuth và luồng OTP đặt lại mật khẩu; việc gửi email cần cấu hình môi trường. Với SePay, server kiểm tra kết quả qua IPN hoặc API. CompletePaidOrder dùng transaction Serializable, kiểm tra trạng thái đơn để callback lặp không cấp mã lần thứ hai; thiếu kho thì rollback.

## Slide 5

### Nội dung trên slide

- KIẾN TRÚC
- Kiến trúc ứng dụng và công nghệ
- Luồng xử lý: Razor View → Controller → DAO / Entity Framework → SQL Server.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /05
- PRESENTATION
- Razor View · Bootstrap 5 · jQuery
- APPLICATION
- MVC 5 · C# · Forms Auth · Google OAuth
- DATA ACCESS
- Entity Framework 6.4.4 · Models / DAO
- SQL SERVER
- Account · Product · Order · Giftcard
- SEPAY GATEWAY
- Xác nhận thanh toán IPN / API
- Dữ liệu: Account → Order → OrderDetail → Product; Giftcard lưu kho mã theo ProductId.

### Lời thuyết trình

Razor View hiển thị giao diện. Controller xử lý yêu cầu, lớp DAO và Entity Framework truy cập SQL Server. Account liên kết đơn mua, Order chứa các OrderDetail và từng chi tiết tham chiếu Product. Giftcard chứa mã trong kho theo sản phẩm. Khi hoàn tất đơn, mã được đưa vào OrderDetail và bản ghi kho tương ứng được xóa trong cùng transaction. SePay là hệ thống ngoài, cần kiểm tra callback phía server.

## Slide 6

### Nội dung trên slide

- TOOLCHAIN
- Môi trường và công cụ triển khai
- Jenkins dùng công cụ build Windows; IIS và SQL Server phục vụ môi trường demo.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /06
- Git / GitHub
- Repository & Pull Request
- Jenkins + Plugins
- Điều phối pipeline / trigger
- NuGet CLI
- Restore packages.config
- MSBuild
- Biên dịch Release .NET Fx
- VSTest
- MSTest qua VSTest · báo cáo TRX
- IIS + SQL Server
- Host website & dữ liệu
- Docker Windows
- Dockerfile + script build / run
- HTTPS Tunnel
- Đưa webhook tới Jenkins local
- Đường dẫn thực tế
- NuGet: C:\Tools\NuGet\nuget.exe  |  Publish: C:\JenkinsDeploy\BBEcom  |  IIS: C:\inetpub\wwwroot\BBEcom

### Lời thuyết trình

Máy demo Windows cung cấp MSBuild, .NET Framework targeting pack, NuGet, VSTest, IIS và SQL Server. NuGet khôi phục packages.config của web; dotnet restore khôi phục PackageReference của project test, không có nghĩa web đã chuyển sang ASP.NET Core. Jenkins điều phối các công cụ này. Tunnel HTTPS chỉ cần khi GitHub phải gọi Jenkins ở máy local; không khẳng định đang dùng ngrok nếu chưa có bằng chứng cấu hình.

## Slide 7

### Nội dung trên slide

- TỰ ĐỘNG KÍCH HOẠT
- GitHub Webhook kích hoạt Jenkins
- Push vào nhánh quyen/admin kích hoạt CI khi webhook và Jenkins job được cấu hình đúng.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /07
- DEVELOPER
- Commit / Push
- GITHUB
- Push Event
- TUNNEL
- HTTPS Tunnel
- JENKINS
- Webhook Trigger
- PIPELINE
- CI → duyệt → CD
- CẤU HÌNH GITHUB
- Payload URL: https://<jenkins-public>/github-webhook/ / Content type: application/json · sự kiện Push
- CẤU HÌNH JENKINS
- Pipeline script from SCM → Jenkinsfile / Bật GitHub hook trigger for GITScm polling
- Luồng phát hành: review / merge → CI → Approve & Deploy → Backup IIS → Deploy → Verify.

### Lời thuyết trình

GitHub gửi Push event tới endpoint /github-webhook/ của Jenkins khi webhook được cấu hình. Jenkins job dùng Pipeline script from SCM và đọc Jenkinsfile. Nhánh checkout trong file hiện là quyen/admin. Review và bảo vệ nhánh cần thiết lập trên GitHub; Jenkinsfile không tự tạo các quy tắc đó. Sau CI, người được chỉ định nhấn Approve & Deploy mới bắt đầu CD. Minh chứng phù hợp gồm webhook delivery, commit và build tương ứng, cùng màn hình phê duyệt.

## Slide 8

### Nội dung trên slide

- CI / CD
- Jenkins Pipeline: 10 stage, 7 nhóm xử lý
- Checkout → Find Build Tools → Restore → Build → Test → Publish → Approval → Backup IIS → Deploy → Verify.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /08
- 01
- CHECKOUT
- Lấy mã
- 02
- TOOLS
- Tìm MSBuild
- 03
- RESTORE
- NuGet / dotnet
- 04
- BUILD / TEST
- MSBuild / VSTest
- 05
- PUBLISH
- Artifact
- 06
- RELEASE
- Duyệt / backup
- 07
- DEPLOY / VERIFY
- IIS / HTTP 200
- Build hoặc Test thất bại
- Dừng pipeline; các stage phát hành phía sau bị bỏ qua.
- Build / Test / Publish thành công
- Chờ duyệt → sao lưu → triển khai → kiểm tra HTTP.
- Approval: quyenha2512, tối đa 24 giờ; lưu TestResults/**/*.trx sau mỗi lần chạy.

### Lời thuyết trình

Pipeline có 10 stage thực tế: Checkout, Find Build Tools, Restore, Build, Test, Publish, Approval, Backup IIS, Deploy, Verify. Sơ đồ gom chúng thành 7 nhóm để dễ nhìn. Find Build Tools tìm MSBuild; Test tìm VSTest. Approval giới hạn 24 giờ cho tài khoản quyenha2512; timeout tổng pipeline là 26 giờ. Khi lệnh build hoặc test trả lỗi, pipeline dừng trước phát hành. File TRX được archive ở phần post, nhưng cấu hình cho phép không có artifact nên vẫn cần kiểm tra báo cáo thực tế.

## Slide 9

### Nội dung trên slide

- QUALITY GATES
- Đi sâu Restore → Build → Test
- Pipeline đã có bước chạy test; chất lượng kiểm thử phụ thuộc vào các assertion thực tế.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /09
- RESTORE
- NuGet + test PackageReference
- nuget restore + dotnet restore
- Khôi phục thư viện web và test
- BUILD
- MSBuild Release
- msbuild BBEcom.sln /p:Configuration=Release
- Kết quả: tạo DLL ở cấu hình Release
- TEST
- MSTest · BBEcom.Tests.dll
- vstest.console.exe <test-assembly>
- Xuất TestResults/SellCard.trx
- Hiện tại TestMethod1() chưa có assertion: test xanh chưa chứng minh nghiệp vụ đúng.

### Lời thuyết trình

Restore xử lý hai kiểu dependency: packages.config của ứng dụng cũ và PackageReference của test. Build biên dịch Release bằng MSBuild. Test chạy DLL net472 bằng VSTest và yêu cầu tạo SellCard.trx. Tuy nhiên BBEcom.Tests/Test1.cs chỉ có TestMethod1 với thân rỗng, không có assertion và project test chưa tham chiếu project ứng dụng. Vì vậy test pass hiện chỉ cho thấy khung chạy được, chưa kiểm chứng nghiệp vụ. Hướng bổ sung là tách logic có thể kiểm thử, kiểm tra giỏ hàng, số tiền, thiếu kho và callback lặp; các trường hợp giao dịch cần integration test với database thử nghiệm.

## Slide 10

### Nội dung trên slide

- CONTINUOUS DELIVERY
- Phê duyệt, sao lưu và triển khai IIS
- Delivery có kiểm soát: Publish → Approval → Backup IIS → Deploy → Verify.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /10
- PUBLISH FILESYSTEM
- Source + MSBuild
- → C:\JenkinsDeploy\BBEcom
- Kiểm tra Web.config, bin/BBEcom.dll và Global.asax
- DEPLOY / VERIFY
- IIS Application: /BBEcom
- Path: C:\inetpub\wwwroot\BBEcom
- CLR v4.0 / Integrated · Verify /BBEcom/Home/Index / HTTP 200; thử lại tối đa 5 lần khi website khởi động
- Backup theo BUILD_NUMBER; giữ Web.config / dữ liệu riêng; dùng app_offline.htm khi copy. Chưa có rollback tự động.

### Lời thuyết trình

Publish tạo bản trung gian tại C:\JenkinsDeploy\BBEcom và kiểm tra Web.config, BBEcom.dll, Global.asax. Sau khi được duyệt, Backup IIS sao lưu website vào C:\JenkinsBackups\BBEcom\<BUILD_NUMBER>. Deploy tạo app_offline.htm, copy bản publish rồi xóa file bảo trì trong finally. Lệnh copy giữ Web.config và loại trừ App_Data, userfiles, uploads; không chạy migration hay reset database. Backup website không phải backup SQL Server. Verify gọi /BBEcom/Home/Index, chấp nhận HTTP 200, tối đa 5 lần; chưa kiểm chứng đăng nhập, thanh toán hoặc cấp mã. Rollback tự động chưa được triển khai.

## Slide 11

### Nội dung trên slide

- MINH CHỨNG
- Kịch bản demo và tiêu chí nghiệm thu
- Dùng ba tình huống để kiểm chứng đường thành công và khả năng chặn bản lỗi.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /11
- TC01
- DEPLOY THÀNH CÔNG
- Đổi nội dung → Push → CI → duyệt phát hành.
- Verify HTTP 200; IIS hiển thị nội dung mới.
- TC02
- BUILD THẤT BẠI
- Tạo lỗi cú pháp ở nhánh demo có kiểm soát.
- Build đỏ; các stage phát hành bị bỏ qua.
- TC03
- TEST THẤT BẠI
- Thêm assertion có chủ đích thất bại trong test.
- Test đỏ; Publish và Deploy không chạy.
- MINH CHỨNG CẦN CHỤP
- Commit / webhook · stage + approval · TRX · backup · IIS trước/sau · đơn hàng / mã đã cấp

### Lời thuyết trình

TC01: đổi một nội dung dễ nhận biết, push vào nhánh pipeline theo quy trình review, chờ CI, duyệt deploy rồi chụp website IIS đã đổi. TC02: trên nhánh hoặc job demo riêng, tạo lỗi cú pháp để chứng minh Build fail và các stage sau bị bỏ qua. TC03: thêm assertion thất bại để chứng minh Test fail chặn Publish và Deploy; không cần làm hỏng logic production. Mỗi ca cần commit, log stage và kết quả mong đợi. Với nghiệp vụ, demo thêm thanh toán thử nghiệm, mã được cấp và callback lặp không cấp thêm. Đây là kịch bản nghiệm thu, chưa phải khẳng định đã chạy đủ.

## Slide 12

### Nội dung trên slide

- MỞ RỘNG THỬ NGHIỆM
- Docker cho ASP.NET MVC 5: Windows Containers
- Bộ khởi tạo Docker đã có trong repo; cần chạy thực tế để xác nhận image và website.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /12
- QUY TRÌNH
- 01
- Kiểm tra engine Windows
- 02
- Build image từ bản Publish
- 03
- Nạp Web.config bên ngoài
- 04
- Run container · 8090:80
- 05
- Kiểm tra HTTP, SQL và đăng nhập
- BASE IMAGE
- ASP.NET Framework 4.8.1 / Windows Server Core LTSC2022
- Docker: Windows Containers
- ĐÃ CÓ: Dockerfile + start/build/run script. CHƯA XÁC NHẬN: build/run thành công; Jenkins chưa có Docker stage.

### Lời thuyết trình

Dockerfile dùng mcr.microsoft.com/dotnet/framework/aspnet:4.8.1-windowsservercore-ltsc2022, có IIS sẵn. Script build lấy artifact Publish, loại Web.config và một số dữ liệu riêng khỏi build context. Script run nhận cấu hình ngoài; start.ps1 chép cấu hình vào thư mục website lúc khởi động rồi chạy ServiceMonitor. Port mặc định 8090 ánh xạ vào 80. SQL Server ở host cần địa chỉ và xác thực riêng, thường qua host.docker.internal với cổng đã cấu hình; localhost trong container không phải host. Cần kiểm tra tương thích Windows, HTTP, SQL, HTTPS/cookie và OAuth. Repo chưa có Docker stage, chưa có bằng chứng build/run thực tế trong phiên rà soát.

## Slide 13

### Nội dung trên slide

- PHÂN TÍCH
- So sánh triển khai IIS và Docker Windows
- IIS phù hợp môi trường hiện tại; Docker là hướng đóng gói và tái lập môi trường.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /13
- TIÊU CHÍ
- IIS TRỰC TIẾP
- DOCKER WINDOWS
- Triển khai
- Copy Publish vào host
- Build image từ artifact
- Runtime
- IIS + .NET Framework trên máy
- IIS + .NET Framework trong image
- Ưu điểm
- Ít bước, dễ kiểm tra trên Windows
- Version image, tái lập runtime
- Thách thức
- Quyền SQL, khóa DLL, cấu hình riêng
- Image lớn, tương thích host, mạng
- SQL Server
- Có thể dùng Windows Authentication
- host.docker.internal; cấu hình SQL riêng
- Trạng thái
- Đích Deploy / Verify trong Jenkinsfile
- Đã có cấu hình; cần chạy xác minh

### Lời thuyết trình

Hai hướng đều chạy cùng ứng dụng MVC 5 và đều cần cấu hình kết nối dữ liệu. IIS trực tiếp có ít bước hơn trong môi trường Windows hiện tại, nhưng phụ thuộc runtime và quyền trên host. Docker ghi lại runtime trong image, thuận tiện gắn phiên bản, đổi lại image lớn, cần host tương thích và cấu hình mạng. Container không tự giải quyết lưu trữ dữ liệu; file tải lên cần volume, database cần sao lưu riêng. Vì vậy triển khai IIS là phạm vi chính, Docker là phần mở rộng cần kiểm chứng.

## Slide 14

### Nội dung trên slide

- KẾT QUẢ VÀ GIỚI HẠN
- Những phần đã triển khai và cần hoàn thiện
- Đánh giá theo mã nguồn hiện tại; không suy diễn trạng thái chạy từ cấu hình sẵn có.
- SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG
- /14
- ỨNG DỤNG / PIPELINE
- Có storefront, Admin, SePay; Jenkinsfile 10 stage, approval, backup và HTTP verify.
- KIỂM THỬ / MINH CHỨNG
- Test hiện là khung rỗng; cần assertion nghiệp vụ và log demo thành công / thất bại.
- DOCKER / KHÔI PHỤC
- Có Dockerfile và script; chưa có Docker stage hoặc cơ chế rollback tự động.
- ƯU TIÊN: test nghiệp vụ → demo 3 kịch bản → lưu minh chứng IIS / TRX → kiểm chứng Docker và rollback.

### Lời thuyết trình

Kết quả xác nhận bằng đọc source là có ứng dụng, tích hợp SePay, Jenkinsfile 10 stage, bước phê duyệt, sao lưu và verify HTTP. Có Dockerfile và các script hỗ trợ. Chưa được coi là kết quả vận hành: tất cả stage xanh, webhook đang hoạt động, website đang chạy hoặc Docker đã build thành công, vì phiên rà soát chưa chạy các thành phần đó. Điểm còn thiếu rõ ràng trong source là test nghiệp vụ và rollback tự động. Không tiếp tục dùng khẳng định build #37/#38 thành công nếu không kèm log hoặc ảnh tương ứng.

## Slide 15

### Nội dung trên slide

- KẾT LUẬN / HƯỚNG PHÁT TRIỂN
- SellCard: triển khai có kiểm soát / TỰ ĐỘNG HÓA ĐI CÙNG XÁC MINH
- 01
- Kiểm thử nghiệp vụ
- Đơn, thanh toán, kho mã, callback lặp
- 02
- Khôi phục phát hành
- Backup đã có; bổ sung rollback
- 03
- Nghiệm thu end-to-end
- CI → duyệt → IIS → mua / nhận mã
- 04
- Mở rộng Docker
- Build / run → SQL → tích hợp CI
- Căn cứ: mã nguồn sell_card · Jenkinsfile · BBEcom.Tests/Test1.cs · README-DOCKER.md

### Lời thuyết trình

Đồ án đã hình thành quy trình phát hành có kiểm soát từ GitHub qua Jenkins đến IIS, phù hợp stack MVC 5. Bước tiếp theo là bổ sung assertion và integration test, chạy đủ ba kịch bản demo, lưu log cùng commit và xác minh nghiệp vụ sau deploy. Về vận hành, bổ sung quy trình rollback, backup database và môi trường staging. Với Docker, cần hoàn tất build/run thủ công, xác nhận SQL và các chức năng rồi mới thêm stage vào Jenkins. Kết thúc bằng demo thực tế và minh chứng, không đưa tỷ lệ thành công hay mức cải thiện chưa đo.