from pathlib import Path
from xml.dom import minidom
from copy import deepcopy
import zipfile
import re

ROOT = Path(__file__).resolve().parent
SOURCE = Path(r'C:\Users\Admin\Downloads\Slide_bao_cao_SellCard_CICD_Jenkins_IIS_Docker_15_slides.pptx')
OUTPUT = ROOT / 'SellCard_Bao_cao_CICD_Hoan_thien_15_slide.pptx'
A = 'http://schemas.openxmlformats.org/drawingml/2006/main'
P = 'http://schemas.openxmlformats.org/presentationml/2006/main'

# Shape indices refer to the original deck; retain its diagrams and visual design.
UPDATES = {
1: {3:'BÁO CÁO ĐỒ ÁN / TRIỂN KHAI PHẦN MỀM',5:'WEBSITE BÁN GIFT CARD & QUY TRÌNH CI/CD',6:'GitHub · Jenkins · MSTest / VSTest · IIS · Docker Windows',8:'ASP.NET MVC 5 · .NET Framework 4.7.2 · SQL Server',12:'DELIVERY'},
2: {2:'Bài toán và yêu cầu triển khai SellCard',3:'Website bán mã số cần cập nhật đúng phiên bản, bảo toàn cấu hình và dữ liệu đơn hàng.',38:'Restore, build và copy thủ công lặp lại; kết quả dễ khác nhau giữa các lần triển khai.',42:'Bỏ sót kiểm thử, thiếu DLL hoặc ghi đè cấu hình có thể làm gián đoạn mua hàng.',46:'Cần gắn bản phát hành với commit, log pipeline và báo cáo kiểm thử để tra cứu.',48:'MỤC TIÊU: TỰ ĐỘNG BUILD / TEST, PHÊ DUYỆT PHÁT HÀNH VÀ XÁC MINH SAU DEPLOY.'},
3: {2:'Mục tiêu, phạm vi và lựa chọn kỹ thuật',3:'Chuẩn hóa triển khai SellCard từ mã nguồn GitHub đến website IIS trên Windows.',10:'Tự động build và test; phát hành có phê duyệt, sao lưu website và kiểm tra HTTP sau triển khai.',14:'IIS là đích triển khai chính; Docker Windows có Dockerfile và script, chưa tích hợp vào pipeline.',15:'LỰA CHỌN KỸ THUẬT',17:'Ứng dụng',18:'MVC 5 / .NET Framework 4.7.2 → MSBuild + IIS CLR v4',20:'Pipeline hiện tại',21:'Nhánh quyen/admin · 10 stage · phê duyệt trước Deploy',22:'Phạm vi báo cáo: mã nguồn và cấu hình hiện có; kết quả vận hành cần log hoặc ảnh demo.'},
4: {10:'Danh mục · giỏ hàng · mua ngay\nĐăng nhập thường / Google\nLịch sử đơn · nhận mã đã mua',14:'Tài khoản · danh mục · sản phẩm\nKho gift code · tin tức · FAQ\nTheo dõi dữ liệu bán hàng',18:'Xác nhận qua IPN hoặc API\nĐối chiếu đơn, tiền, CAPTURED\nTransaction chống cấp mã lặp',38:'Đơn hợp lệ → cấp mã từ kho → lưu OrderDetail → cập nhật trạng thái trong cùng transaction.'},
5: {3:'Luồng xử lý: Razor View → Controller → DAO / Entity Framework → SQL Server.',12:'MVC 5 · C# · Forms Auth · Google OAuth',18:'Account · Product · Order · Giftcard',21:'Xác nhận thanh toán IPN / API',27:'Dữ liệu: Account → Order → OrderDetail → Product; Giftcard lưu kho mã theo ProductId.'},
6: {3:'Jenkins dùng công cụ build Windows; IIS và SQL Server phục vụ môi trường demo.',21:'MSTest qua VSTest · báo cáo TRX',27:'Dockerfile + script build / run',29:'HTTPS Tunnel',30:'Đưa webhook tới Jenkins local',33:'NuGet: C:\\Tools\\NuGet\\nuget.exe  |  Publish: C:\\JenkinsDeploy\\BBEcom  |  IIS: C:\\inetpub\\wwwroot\\BBEcom'},
7: {3:'Push vào nhánh quyen/admin kích hoạt CI khi webhook và Jenkins job được cấu hình đúng.',18:'TUNNEL',29:'CI → duyệt → CD',32:'Payload URL: https://<jenkins-public>/github-webhook/\nContent type: application/json · sự kiện Push',35:'Pipeline script from SCM → Jenkinsfile\nBật GitHub hook trigger for GITScm polling',36:'Luồng phát hành: review / merge → CI → Approve & Deploy → Backup IIS → Deploy → Verify.'},
8: {2:'Jenkins Pipeline: 10 stage, 7 nhóm xử lý',3:'Checkout → Find Build Tools → Restore → Build → Test → Publish → Approval → Backup IIS → Deploy → Verify.',15:'TOOLS',16:'Tìm MSBuild',21:'RESTORE',22:'NuGet / dotnet',27:'BUILD / TEST',28:'MSBuild / VSTest',39:'RELEASE',40:'Duyệt / backup',45:'DEPLOY / VERIFY',46:'IIS / HTTP 200',48:'Build hoặc Test thất bại',49:'Dừng pipeline; các stage phát hành phía sau bị bỏ qua.',51:'Build / Test / Publish thành công',52:'Chờ duyệt → sao lưu → triển khai → kiểm tra HTTP.',53:'Approval: quyenha2512, tối đa 24 giờ; lưu TestResults/**/*.trx sau mỗi lần chạy.'},
9: {3:'Pipeline đã có bước chạy test; chất lượng kiểm thử phụ thuộc vào các assertion thực tế.',10:'NuGet + test PackageReference',12:'nuget restore + dotnet restore',13:'Khôi phục thư viện web và test',20:'Kết quả: tạo DLL ở cấu hình Release',24:'MSTest · BBEcom.Tests.dll',27:'Xuất TestResults/SellCard.trx',29:'Hiện tại TestMethod1() chưa có assertion: test xanh chưa chứng minh nghiệp vụ đúng.'},
10: {2:'Phê duyệt, sao lưu và triển khai IIS',3:'Delivery có kiểm soát: Publish → Approval → Backup IIS → Deploy → Verify.',12:'Kiểm tra Web.config, bin/BBEcom.dll và Global.asax',18:'CLR v4.0 / Integrated · Verify /BBEcom/Home/Index\nHTTP 200; thử lại tối đa 5 lần khi website khởi động',22:'Backup theo BUILD_NUMBER; giữ Web.config / dữ liệu riêng; dùng app_offline.htm khi copy. Chưa có rollback tự động.'},
11: {2:'Kịch bản demo và tiêu chí nghiệm thu',3:'Dùng ba tình huống để kiểm chứng đường thành công và khả năng chặn bản lỗi.',11:'Đổi nội dung → Push → CI → duyệt phát hành.',12:'Verify HTTP 200; IIS hiển thị nội dung mới.',17:'Tạo lỗi cú pháp ở nhánh demo có kiểm soát.',18:'Build đỏ; các stage phát hành bị bỏ qua.',23:'Thêm assertion có chủ đích thất bại trong test.',24:'Test đỏ; Publish và Deploy không chạy.',26:'Commit / webhook · stage + approval · TRX · backup · IIS trước/sau · đơn hàng / mã đã cấp'},
12: {3:'Bộ khởi tạo Docker đã có trong repo; cần chạy thực tế để xác nhận image và website.',11:'Kiểm tra engine Windows',14:'Build image từ bản Publish',17:'Nạp Web.config bên ngoài',20:'Run container · 8090:80',23:'Kiểm tra HTTP, SQL và đăng nhập',26:'ASP.NET Framework 4.8.1\nWindows Server Core LTSC2022',29:'ĐÃ CÓ: Dockerfile + start/build/run script. CHƯA XÁC NHẬN: build/run thành công; Jenkins chưa có Docker stage.'},
13: {3:'IIS phù hợp môi trường hiện tại; Docker là hướng đóng gói và tái lập môi trường.',21:'Ít bước, dễ kiểm tra trên Windows',22:'Version image, tái lập runtime',24:'Quyền SQL, khóa DLL, cấu hình riêng',25:'Image lớn, tương thích host, mạng',28:'Có thể dùng Windows Authentication',29:'host.docker.internal; cấu hình SQL riêng',31:'Đích Deploy / Verify trong Jenkinsfile',32:'Đã có cấu hình; cần chạy xác minh'},
14: {1:'KẾT QUẢ VÀ GIỚI HẠN',2:'Những phần đã triển khai và cần hoàn thiện',3:'Đánh giá theo mã nguồn hiện tại; không suy diễn trạng thái chạy từ cấu hình sẵn có.',9:'ỨNG DỤNG / PIPELINE',10:'Có storefront, Admin, SePay; Jenkinsfile 10 stage, approval, backup và HTTP verify.',13:'KIỂM THỬ / MINH CHỨNG',14:'Test hiện là khung rỗng; cần assertion nghiệp vụ và log demo thành công / thất bại.',17:'DOCKER / KHÔI PHỤC',18:'Có Dockerfile và script; chưa có Docker stage hoặc cơ chế rollback tự động.',20:'ƯU TIÊN: test nghiệp vụ → demo 3 kịch bản → lưu minh chứng IIS / TRX → kiểm chứng Docker và rollback.'},
15: {2:'SellCard: triển khai có kiểm soát\nTỰ ĐỘNG HÓA ĐI CÙNG XÁC MINH',5:'Kiểm thử nghiệp vụ',6:'Đơn, thanh toán, kho mã, callback lặp',9:'Khôi phục phát hành',10:'Backup đã có; bổ sung rollback',13:'Nghiệm thu end-to-end',14:'CI → duyệt → IIS → mua / nhận mã',17:'Mở rộng Docker',18:'Build / run → SQL → tích hợp CI',20:'Căn cứ: mã nguồn sell_card · Jenkinsfile · BBEcom.Tests/Test1.cs · README-DOCKER.md'}
}

NOTES = [
'Đề tài trình bày website SellCard, giao diện mang tên GiftToi, cùng quy trình triển khai từ GitHub sang IIS. Ứng dụng dùng ASP.NET MVC 5 trên .NET Framework 4.7.2. Phần chính là CI/CD trên Windows; Docker là hướng mở rộng đã có cấu hình. Báo cáo phân biệt phần đã viết trong mã nguồn với kết quả cần có log vận hành để xác nhận.',
'Khi triển khai thủ công, người thực hiện phải lấy đúng source, restore, build, test, publish rồi copy sang IIS. Với website bán mã số, một lỗi triển khai có thể làm gián đoạn thanh toán hoặc giao mã. Vì vậy quy trình cần kiểm soát phiên bản, bảo toàn cấu hình riêng và có thông tin truy vết từng lần chạy. Không đưa số liệu tiết kiệm thời gian khi chưa đo thực tế.',
'Mục tiêu là chuẩn hóa các bước từ commit đến phát hành. Jenkinsfile hiện lấy nhánh quyen/admin và định nghĩa 10 stage. Sau CI, hệ thống yêu cầu người có quyền phê duyệt trước khi sao lưu và triển khai. Đây là continuous delivery có phê duyệt thủ công. Dự án dùng .NET Framework nên công cụ và cấu hình IIS cần phù hợp MVC 5; không dùng giả định của ASP.NET Core.',
'Khách hàng xem sản phẩm, đăng nhập, mua ngay hoặc dùng giỏ hàng, thanh toán và nhận mã trong lịch sử đơn. Quản trị viên quản lý sản phẩm, tài khoản và kho code. LoginController còn có Google OAuth và luồng OTP đặt lại mật khẩu; việc gửi email cần cấu hình môi trường. Với SePay, server kiểm tra kết quả qua IPN hoặc API. CompletePaidOrder dùng transaction Serializable, kiểm tra trạng thái đơn để callback lặp không cấp mã lần thứ hai; thiếu kho thì rollback.',
'Razor View hiển thị giao diện. Controller xử lý yêu cầu, lớp DAO và Entity Framework truy cập SQL Server. Account liên kết đơn mua, Order chứa các OrderDetail và từng chi tiết tham chiếu Product. Giftcard chứa mã trong kho theo sản phẩm. Khi hoàn tất đơn, mã được đưa vào OrderDetail và bản ghi kho tương ứng được xóa trong cùng transaction. SePay là hệ thống ngoài, cần kiểm tra callback phía server.',
'Máy demo Windows cung cấp MSBuild, .NET Framework targeting pack, NuGet, VSTest, IIS và SQL Server. NuGet khôi phục packages.config của web; dotnet restore khôi phục PackageReference của project test, không có nghĩa web đã chuyển sang ASP.NET Core. Jenkins điều phối các công cụ này. Tunnel HTTPS chỉ cần khi GitHub phải gọi Jenkins ở máy local; không khẳng định đang dùng ngrok nếu chưa có bằng chứng cấu hình.',
'GitHub gửi Push event tới endpoint /github-webhook/ của Jenkins khi webhook được cấu hình. Jenkins job dùng Pipeline script from SCM và đọc Jenkinsfile. Nhánh checkout trong file hiện là quyen/admin. Review và bảo vệ nhánh cần thiết lập trên GitHub; Jenkinsfile không tự tạo các quy tắc đó. Sau CI, người được chỉ định nhấn Approve & Deploy mới bắt đầu CD. Minh chứng phù hợp gồm webhook delivery, commit và build tương ứng, cùng màn hình phê duyệt.',
'Pipeline có 10 stage thực tế: Checkout, Find Build Tools, Restore, Build, Test, Publish, Approval, Backup IIS, Deploy, Verify. Sơ đồ gom chúng thành 7 nhóm để dễ nhìn. Find Build Tools tìm MSBuild; Test tìm VSTest. Approval giới hạn 24 giờ cho tài khoản quyenha2512; timeout tổng pipeline là 26 giờ. Khi lệnh build hoặc test trả lỗi, pipeline dừng trước phát hành. File TRX được archive ở phần post, nhưng cấu hình cho phép không có artifact nên vẫn cần kiểm tra báo cáo thực tế.',
'Restore xử lý hai kiểu dependency: packages.config của ứng dụng cũ và PackageReference của test. Build biên dịch Release bằng MSBuild. Test chạy DLL net472 bằng VSTest và yêu cầu tạo SellCard.trx. Tuy nhiên BBEcom.Tests/Test1.cs chỉ có TestMethod1 với thân rỗng, không có assertion và project test chưa tham chiếu project ứng dụng. Vì vậy test pass hiện chỉ cho thấy khung chạy được, chưa kiểm chứng nghiệp vụ. Hướng bổ sung là tách logic có thể kiểm thử, kiểm tra giỏ hàng, số tiền, thiếu kho và callback lặp; các trường hợp giao dịch cần integration test với database thử nghiệm.',
'Publish tạo bản trung gian tại C:\\JenkinsDeploy\\BBEcom và kiểm tra Web.config, BBEcom.dll, Global.asax. Sau khi được duyệt, Backup IIS sao lưu website vào C:\\JenkinsBackups\\BBEcom\\<BUILD_NUMBER>. Deploy tạo app_offline.htm, copy bản publish rồi xóa file bảo trì trong finally. Lệnh copy giữ Web.config và loại trừ App_Data, userfiles, uploads; không chạy migration hay reset database. Backup website không phải backup SQL Server. Verify gọi /BBEcom/Home/Index, chấp nhận HTTP 200, tối đa 5 lần; chưa kiểm chứng đăng nhập, thanh toán hoặc cấp mã. Rollback tự động chưa được triển khai.',
'TC01: đổi một nội dung dễ nhận biết, push vào nhánh pipeline theo quy trình review, chờ CI, duyệt deploy rồi chụp website IIS đã đổi. TC02: trên nhánh hoặc job demo riêng, tạo lỗi cú pháp để chứng minh Build fail và các stage sau bị bỏ qua. TC03: thêm assertion thất bại để chứng minh Test fail chặn Publish và Deploy; không cần làm hỏng logic production. Mỗi ca cần commit, log stage và kết quả mong đợi. Với nghiệp vụ, demo thêm thanh toán thử nghiệm, mã được cấp và callback lặp không cấp thêm. Đây là kịch bản nghiệm thu, chưa phải khẳng định đã chạy đủ.',
'Dockerfile dùng mcr.microsoft.com/dotnet/framework/aspnet:4.8.1-windowsservercore-ltsc2022, có IIS sẵn. Script build lấy artifact Publish, loại Web.config và một số dữ liệu riêng khỏi build context. Script run nhận cấu hình ngoài; start.ps1 chép cấu hình vào thư mục website lúc khởi động rồi chạy ServiceMonitor. Port mặc định 8090 ánh xạ vào 80. SQL Server ở host cần địa chỉ và xác thực riêng, thường qua host.docker.internal với cổng đã cấu hình; localhost trong container không phải host. Cần kiểm tra tương thích Windows, HTTP, SQL, HTTPS/cookie và OAuth. Repo chưa có Docker stage, chưa có bằng chứng build/run thực tế trong phiên rà soát.',
'Hai hướng đều chạy cùng ứng dụng MVC 5 và đều cần cấu hình kết nối dữ liệu. IIS trực tiếp có ít bước hơn trong môi trường Windows hiện tại, nhưng phụ thuộc runtime và quyền trên host. Docker ghi lại runtime trong image, thuận tiện gắn phiên bản, đổi lại image lớn, cần host tương thích và cấu hình mạng. Container không tự giải quyết lưu trữ dữ liệu; file tải lên cần volume, database cần sao lưu riêng. Vì vậy triển khai IIS là phạm vi chính, Docker là phần mở rộng cần kiểm chứng.',
'Kết quả xác nhận bằng đọc source là có ứng dụng, tích hợp SePay, Jenkinsfile 10 stage, bước phê duyệt, sao lưu và verify HTTP. Có Dockerfile và các script hỗ trợ. Chưa được coi là kết quả vận hành: tất cả stage xanh, webhook đang hoạt động, website đang chạy hoặc Docker đã build thành công, vì phiên rà soát chưa chạy các thành phần đó. Điểm còn thiếu rõ ràng trong source là test nghiệp vụ và rollback tự động. Không tiếp tục dùng khẳng định build #37/#38 thành công nếu không kèm log hoặc ảnh tương ứng.',
'Đồ án đã hình thành quy trình phát hành có kiểm soát từ GitHub qua Jenkins đến IIS, phù hợp stack MVC 5. Bước tiếp theo là bổ sung assertion và integration test, chạy đủ ba kịch bản demo, lưu log cùng commit và xác minh nghiệp vụ sau deploy. Về vận hành, bổ sung quy trình rollback, backup database và môi trường staging. Với Docker, cần hoàn tất build/run thủ công, xác nhận SQL và các chức năng rồi mới thêm stage vào Jenkins. Kết thúc bằng demo thực tế và minh chứng, không đưa tỷ lệ thành công hay mức cải thiện chưa đo.'
]

def replace_text(doc, shape, value):
    body = shape.getElementsByTagNameNS(P, 'txBody')[0]
    old_paragraphs = [c for c in body.childNodes if c.nodeType == c.ELEMENT_NODE and c.localName == 'p']
    template = old_paragraphs[0]
    runs = template.getElementsByTagNameNS(A, 'r')
    run_template = runs[0] if runs else None
    for p in old_paragraphs:
        body.removeChild(p)
    for line in value.split('\n'):
        p = doc.createElementNS(A, 'a:p')
        for child in template.childNodes:
            if child.nodeType == child.ELEMENT_NODE and child.localName == 'pPr':
                p.appendChild(child.cloneNode(True))
        r = doc.createElementNS(A, 'a:r')
        if run_template:
            props = run_template.getElementsByTagNameNS(A, 'rPr')
            if props:
                r.appendChild(props[0].cloneNode(True))
        t = doc.createElementNS(A, 'a:t')
        t.appendChild(doc.createTextNode(line))
        r.appendChild(t)
        p.appendChild(r)
        body.appendChild(p)

markdown = ['# SellCard — Nội dung slide và lời thuyết trình',
    'Bản chỉnh sửa ngày 05/11/2026. Giữ bố cục 15 slide gốc, cập nhật theo source tại workspace. Lời thuyết trình cũng được lưu trong Notes của PowerPoint.',
    'Lưu ý sử dụng: các kịch bản demo là tiêu chí cần thực hiện. Không xem cấu hình pipeline, test rỗng hay Dockerfile là bằng chứng đã chạy thành công. Chưa chạy Jenkins, IIS, Docker hoặc kiểm thử thanh toán trong lần biên soạn này.']

with zipfile.ZipFile(SOURCE) as zin, zipfile.ZipFile(OUTPUT, 'w', zipfile.ZIP_DEFLATED) as zout:
    for info in zin.infolist():
        data = zin.read(info.filename)
        slide_match = re.fullmatch(r'ppt/slides/slide(\d+)\.xml', info.filename)
        note_match = re.fullmatch(r'ppt/notesSlides/notesSlide(\d+)\.xml', info.filename)
        if slide_match:
            number = int(slide_match.group(1))
            doc = minidom.parseString(data)
            shapes = doc.getElementsByTagNameNS(P, 'sp')
            for index, value in UPDATES.get(number, {}).items():
                replace_text(doc, shapes[index], value)
            if number > 1 and number < 15:
                replace_text(doc, shapes[5], 'SELLCARD | GITHUB → JENKINS → IIS · DOCKER MỞ RỘNG')
            markdown.append(f'\n## Slide {number}\n\n### Nội dung trên slide\n')
            for shape in shapes:
                strings = [t.firstChild.data if t.firstChild else '' for t in shape.getElementsByTagNameNS(A, 't')]
                if strings:
                    markdown.append('- ' + ' / '.join(strings))
            markdown.append('\n### Lời thuyết trình\n\n' + NOTES[number-1])
            data = doc.toxml(encoding='UTF-8')
        elif note_match:
            number = int(note_match.group(1))
            doc = minidom.parseString(data)
            for shape in doc.getElementsByTagNameNS(P, 'sp'):
                placeholders = shape.getElementsByTagNameNS(P, 'ph')
                if placeholders and placeholders[0].getAttribute('type') == 'body':
                    replace_text(doc, shape, NOTES[number-1])
                    break
            else:
                raise ValueError(f'Notes body missing: {number}')
            data = doc.toxml(encoding='UTF-8')
        zout.writestr(info, data)

(ROOT / 'SellCard_Noi_dung_va_loi_thuyet_trinh.md').write_text('\n'.join(markdown), encoding='utf-8')
with zipfile.ZipFile(OUTPUT) as z:
    assert z.testzip() is None
    for name in z.namelist():
        if name.endswith('.xml') or name.endswith('.rels'):
            minidom.parseString(z.read(name))
    assert sum(bool(re.fullmatch(r'ppt/slides/slide\d+\.xml', n)) for n in z.namelist()) == 15
    for i in range(1,16):
        note_doc = minidom.parseString(z.read(f'ppt/notesSlides/notesSlide{i}.xml'))
        note_text = '\n'.join(t.firstChild.data if t.firstChild else '' for t in note_doc.getElementsByTagNameNS(A, 't'))
        assert NOTES[i-1] in note_text
print(f'Created and validated: {OUTPUT}')
print('15 slides; 15 speaker notes; all XML parts parse successfully.')
