# Tư vấn dinh dưỡng có chuyên gia duyệt (HLT-07)

Cấu trúc:
- `db/` : script T-SQL chạy trong SQL Server Management Studio (SSMS), database `TuVanDinhDuong`, schema `tuvan`.
- `api/` : ASP.NET Core Web API, truy cập dữ liệu bằng SQL qua Dapper.
- `web/` : React + Vite + TypeScript, mở bằng VS Code.
- `docker-compose.yml` : chạy Keycloak (đăng nhập OIDC).

## Thứ tự chạy
1. Trong SSMS, mở và chạy lần lượt `db/01_schema.sql`, `02_triggers.sql`, `03_views_procs.sql`, `04_seed_minh_hoa.sql`.
2. `docker compose up -d` để chạy Keycloak tại http://localhost:8081.
3. Trong Keycloak: tạo realm `tuvan`, client `tuvan-web` (public, PKCE S256, redirect `http://localhost:5173/*`), tạo 4 người dùng.
   Mỗi người dùng gán một vai trò trong: `NguoiDung`, `ChuyenGiaSoan`, `ChuyenGiaDuyet`, `QuanTriVien`.
   Thêm mapper để đưa realm roles lên claim `roles` ở mức gốc của token.
4. Lấy `sub` của từng người dùng Keycloak, thay giá trị `REPLACE-...-sub` trong `04_seed_minh_hoa.sql` (hoặc cập nhật trực tiếp trong bảng `tuvan.nguoi_dung`).
5. Trong `api/appsettings.json`, chỉnh chuỗi kết nối tới SQL Server của bạn.
6. Mở thư mục gốc `tuvan-dinh-duong` bằng VS Code (File → Open Folder).
   - Terminal 1: `cd api` rồi `dotnet run` (API chạy tại http://localhost:5000).
   - Terminal 2: `cd web` rồi `npm install` và `npm run dev` (web chạy tại http://localhost:5173).
   - Cài các extension trong `.vscode/extensions.json` khi VS Code gợi ý.

## Lưu ý kỹ thuật
- Keycloak phải đưa vai trò vào claim `roles` ở mức gốc: vào Client scopes → roles → Mappers, tạo mapper "User Realm Role" với Token Claim Name = `roles`, và bật "Add to access token".
- Trang "Soạn kế hoạch" hiện nhập trực tiếp mã người dùng (GUID). Khi có thời gian, nên thêm API tìm khách theo tên.
- Chưa được chạy kiểm thử tự động. Hãy chạy SQL theo đúng thứ tự trước, rồi mới chạy API.

## Kiểm tra nhanh
- Đăng nhập bằng `soan01`: tạo kế hoạch nháp, gắn khuyến nghị có nguồn, gửi duyệt.
- Đăng nhập bằng `duyet01`: vào hàng đợi, duyệt hoặc trả lại. Không duyệt được kế hoạch do chính mình soạn (BR02).
