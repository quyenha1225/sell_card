GIFTTOI - PATCH TIM KIEM, SAP XEP, PHAN TRANG VA DON GIAO DIEN
==============================================================

CAC FILE DA SUA DAY DU
----------------------
1. App_Start/RouteConfig.cs
   - Them duong dan /tim-kiem.

2. Controllers/ProductController.cs
   - Them action Search.
   - Them sap xep, pageSize va phan trang cho danh muc.
   - Kiem tra danh muc/san pham khong ton tai hoac bi tat.

3. Models/DAO/ProductDAO.cs
   - Tim theo ten san pham, alias va ten danh muc.
   - Sap xep: uu dai, moi nhat, gia tang/giam, ten A-Z/Z-A.
   - Chi hien san pham Status = true o storefront.
   - Nap Category va Giftcard de View khong bi loi lazy loading.

4. Models/DAO/CategoryDAO.cs
   - Dem dung san pham dang hoat dong.
   - Sua thu tu OrderBy/Take va bo sung kiem tra null.

5. Views/Layout/NavbarMenu.cshtml
   - Menu an toan khi ViewBag null.
   - Khong con href="#" cho dropdown.
   - Them muc Tat ca san pham.

6. Views/Shared/_Layout.cshtml
   - Tim kiem hoat dong o header va popup tren moi trang.
   - Xoa noi dung mau FREE SHIP ngay 30/2, ngon ngu gia va modal mau du thua.
   - Don footer, chi mo ta phuong thuc thanh toan dang dung.

7. Views/Home/Index.cshtml
   - Form tim kiem that.
   - Cac nut Shop Now/banner/brand khong con tro den #.
   - Viet hoa va don noi dung trang chu.

8. Views/Product/Category.cshtml
   - Dropdown sap xep va so san pham moi trang hoat dong that.

9. Views/Shared/Products.cshtml
   - Danh sach danh muc giu sort/pageSize khi chuyen trang.
   - Chi hien san pham dang hoat dong, card gon va an toan null.

10. Views/Product/Search.cshtml
    - File moi: trang ket qua tim kiem, sap xep va phan trang.

CACH CAI
--------
1. Tat Visual Studio/IIS Express neu project dang chay.
2. Sao luu project hoac commit Git truoc khi thay doi.
3. Giai nen ZIP vao THU MUC GOC cua project, noi co BBEcom.sln va BBEcom.csproj.
4. Chon Replace the files in the destination.
5. Mo PowerShell tai thu muc project va chay:

   Set-ExecutionPolicy -Scope Process Bypass
   .\apply_project_file_patch.ps1

6. Mo BBEcom.sln bang Visual Studio.
7. Build > Clean Solution.
8. Build > Rebuild Solution.
9. Chay project va kiem tra:
   - /tim-kiem
   - Tim tu header.
   - Sap xep gia tang/giam.
   - Chuyen trang tim kiem.
   - Chuyen trang danh muc ma van giu sap xep.

NEU KHONG MUON CHAY POWERSHELL
------------------------------
Mo BBEcom.csproj bang trinh soan thao va them dong sau ngay sau Category.cshtml:

    <Content Include="Views\Product\Search.cshtml" />

Luu y: Website van co the chay local khi file Search.cshtml chua nam trong csproj,
nhung Visual Studio/Publish co the khong dong goi file moi. Vi vay nen them dong tren.

PHUC HOI
--------
- Git: git restore <duong-dan-file>
- Hoac dung BBEcom.csproj.backup_before_search_patch de phuc hoi project file.
