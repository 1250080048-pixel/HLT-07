Tư vấn dinh dưỡng có chuyên gia duyệt

Nền tảng hỗ trợ theo dõi dinh dưỡng cá nhân, trong đó mọi kế hoạch đều do chuyên gia soạn và duyệt trước khi áp dụng. Hệ thống không tự tạo khuyến nghị điều trị.

Đây là đồ án môn học (mã đề tài HLT-07).

Tính năng chính

Khách (người dùng)

Tự đăng ký tài khoản, khai báo hồ sơ (chiều cao, cân nặng, nhóm dị ứng, nhóm rủi ro cao).
Chọn mục tiêu dinh dưỡng (giảm cân, tăng cân lành mạnh, tăng cơ, duy trì). Thực đơn mẫu 7 ngày đã được chuyên gia duyệt.
Nếu có dị ứng, các món chứa nhóm dị ứng được thay bằng món thay thế đã duyệt. Nếu không có món thay, hệ thống chuyển sang kế hoạch riêng.
Nếu thuộc nhóm rủi ro cao, không áp dụng thực đơn chung; gửi yêu cầu kế hoạch riêng.
Nhật ký ăn: tích món đã ăn theo thực đơn của ngày, hoặc ghi món ngoài thực đơn.
Theo dõi tiến độ và tổng calo nạp vào theo ngày (ước tính, đối chiếu với khoảng tham khảo do chuyên gia nhập).

Chuyên gia soạn

Xem yêu cầu chờ từ khách, soạn bản nháp kế hoạch, thêm/xóa món theo từng ngày và bữa, thêm khuyến nghị có nguồn, gửi duyệt.

Chuyên gia duyệt

Xem hồ sơ khách và thực đơn khi thẩm định (mỗi lần xem được ghi nhật ký). Duyệt hoặc trả lại kèm lý do. Không được duyệt kế hoạch do chính mình soạn.

Quản trị viên

Đã có tài khoản và vai trò trong Keycloak; chức năng quản trị trên web chưa được cài đặt.
Công nghệ
Thành phần	Công nghệ
Cơ sở dữ liệu	SQL Server (Express), schema tuvan
API	C# / ASP.NET Core 8, Dapper, Microsoft.Data.SqlClient
Giao diện	React, TypeScript, Vite, react-router, oidc-client-ts
Đăng nhập	Keycloak 25 (OpenID Connect, PKCE)
Triển khai cục bộ	Docker Compose (chỉ chạy Keycloak)
Cấu trúc thư mục
tuvan-dinh-duong/
├── db/                  # Script SQL theo thứ tự 01 → 12
├── api/                 # ASP.NET Core API
│   ├── Data/            # Repository và truy vấn Dapper
│   ├── Workers/         # Worker xử lý hàng đợi nền
│   ├── Program.cs       # Định nghĩa endpoint và phân quyền
│   └── appsettings.json
├── web/                 # React + Vite
│   └── src/
│       ├── pages/       # Các màn hình
│       ├── components/  # Thành phần dùng chung
│       └── lib/         # Tiện ích (ngày tháng)
├── docker-compose.yml   # Keycloak
└── .env.example
Yêu cầu môi trường
Windows 10/11
.NET 8 SDK
Node.js 18 trở lên
SQL Server Express (instance SQLEXPRESS) và SQL Server Management Studio (SSMS)
Docker Desktop
Cài đặt
1. Khởi động Keycloak
bash
cd tuvan-dinh-duong
docker compose up -d

Mở http://localhost:8081, đăng nhập admin (mật khẩu theo KC_ADMIN_PASSWORD trong docker-compose.yml). Sau đó:

Tạo realm tuvan.
Tạo client tuvan-web: OpenID Connect, Client authentication Off, Standard flow On, PKCE S256 (tab Advanced), Valid redirect URIs http://localhost:5173/*, Web origins http://localhost:5173.
Tạo mapper Audience cho client tuvan-web, thêm audience tuvan-api, bật Add to access token.
Sửa mapper vai trò trong scope roles: Token Claim Name là roles, bật Add to ID token và Add to access token.
Tạo các realm role: NguoiDung, ChuyenGiaSoan, ChuyenGiaDuyet, QuanTriVien.
Bật User registration (tab Login) và thêm NguoiDung vào Default roles (tab User registration).
Tạo tài khoản nội bộ và gán vai trò tương ứng, đặt mật khẩu với Temporary tắt:
nguoidung01 → NguoiDung
soan01 → ChuyenGiaSoan
duyet01 → ChuyenGiaDuyet
admin01 → QuanTriVien
2. Tạo cơ sở dữ liệu

Mở SSMS, kết nối localhost\SQLEXPRESS bằng Windows Authentication, rồi chạy lần lượt các file trong thư mục db/:

01_schema.sql
02_triggers.sql
03_views_procs.sql
04_seed_minh_hoa.sql
05_huong_dan_chung.sql
06_muc_tieu_thuc_don.sql
07_bat_dau_theo_muc_tieu.sql
08_ke_hoach_rieng.sql
09_thay_mon_di_ung.sql
10_thuc_don_theo_ngay.sql
11_calo.sql
12_tich_mon.sql

Sau khi chạy 04_seed_minh_hoa.sql, cập nhật subject_oidc cho bốn tài khoản mẫu bằng ID người dùng lấy từ Keycloak (trường ID ở tab Details). Ví dụ:

sql
UPDATE TuVanDinhDuong.tuvan.nguoi_dung
   SET subject_oidc = N'<ID-keycloak-cua-nguoidung01>'
 WHERE ma_nguoi_dung = '11111111-0000-0000-0000-000000000001';

Lặp lại cho soan01 (…0002), duyet01 (…0003), admin01 (…0004).

3. Cấu hình API

Mở api/appsettings.json và kiểm tra:

json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost\\SQLEXPRESS;Database=TuVanDinhDuong;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Oidc": {
    "Authority": "http://localhost:8081/realms/tuvan",
    "Audience": "tuvan-api"
  },
  "Cors": {
    "Origin": "http://localhost:5173"
  }
}

Lưu ý: trong JSON, dấu \ phải viết thành \\.

4. Cài đặt web
bash
cd web
npm install
Chạy chương trình

Cần mở ba thứ theo thứ tự: Keycloak, API, web.

Terminal 1: API

bash
cd api
dotnet run

Chờ đến khi thấy Now listening on: http://localhost:5000.

Terminal 2: web

bash
cd web
npm run dev

Chờ đến khi thấy Local: http://localhost:5173/.

Mở http://localhost:5173 trong trình duyệt. Đăng nhập bằng một trong các tài khoản đã tạo, hoặc bấm Register để tạo tài khoản khách mới.

Kiểm tra nhanh
Tài khoản	Trang nên thấy
nguoidung01	Tổng quan, Mục tiêu, Nhật ký ăn, Kế hoạch riêng, Hồ sơ
soan01	Yêu cầu chờ, Soạn kế hoạch, Thực đơn kế hoạch
duyet01	Hàng đợi duyệt
Xử lý lỗi thường gặp
failed to connect to the docker API: Docker Desktop chưa chạy. Mở Docker Desktop và chờ đến khi hiện Engine running.
API không kết nối được SQL Server: kiểm tra ConnectionStrings:SqlServer dùng đúng localhost\\SQLEXPRESS.
Lỗi 403 khi vào trang chuyên gia: kiểm tra vai trò của tài khoản trong Keycloak và mapper roles, sau đó đăng xuất và đăng nhập lại để token mới được cấp.
Lỗi 404 ở trang Nhật ký: file TichMonRepository.cs chưa có trong api/Data, hoặc endpoint /api/nhat-ky/thuc-don chưa được khai báo trong Program.cs.
Trang Chọn mục tiêu báo “Cần khai báo hồ sơ”: khách phải lưu hồ sơ trước khi chọn mục tiêu.
Lưu ý an toàn
Dữ liệu trong dự án là dữ liệu minh họa. Kcal, mốc calo, nguồn tham khảo và thực đơn mẫu cần được chuyên gia đối chiếu với nguồn đã kiểm chứng trước khi dùng thật.
Cấu hình Keycloak trong hướng dẫn là cấu hình chạy thử trên localhost (RequireHttpsMetadata = false, Keycloak dev mode). Không dùng nguyên cấu hình này khi triển khai thật.
Hồ sơ khách là dữ liệu sức khỏe. Không đưa dữ liệu thật vào môi trường thử nghiệm.
