-- DỮ LIỆU MINH HỌA. Không dùng cho người thật.
-- Thay subject_oidc bằng giá trị "sub" của từng tài khoản trong Keycloak sau khi tạo người dùng.
USE TuVanDinhDuong;
GO
INSERT INTO tuvan.nguoi_dung (ma_nguoi_dung, subject_oidc, ho_ten, vai_tro) VALUES
 ('11111111-0000-0000-0000-000000000001', N'REPLACE-nguoidung01-sub', N'Khách minh họa', N'NguoiDung'),
 ('11111111-0000-0000-0000-000000000002', N'REPLACE-soan01-sub',      N'Chuyên gia soạn (minh họa)', N'ChuyenGiaSoan'),
 ('11111111-0000-0000-0000-000000000003', N'REPLACE-duyet01-sub',     N'Chuyên gia duyệt (minh họa)', N'ChuyenGiaDuyet'),
 ('11111111-0000-0000-0000-000000000004', N'REPLACE-admin01-sub',     N'Quản trị viên (minh họa)', N'QuanTriVien');

INSERT INTO tuvan.ho_so_dinh_duong (ma_nguoi_dung, chieu_cao, can_nang, thuoc_nhom_rui_ro_cao)
VALUES ('11111111-0000-0000-0000-000000000001', 165.0, 60.0, 0);

INSERT INTO tuvan.nguon_tham_khao (ma_nguon, ten_nguon, don_vi_cong_bo, nam_cong_bo) VALUES
 ('22222222-0000-0000-0000-000000000001', N'Khuyến nghị dinh dưỡng (minh họa)', N'Viện Dinh dưỡng Quốc gia', 2020);

INSERT INTO tuvan.mon_an (ma_mon_an, ten_mon, nang_luong) VALUES
 ('33333333-0000-0000-0000-000000000001', N'Cơm gạo lứt (minh họa)', 110.0),
 ('33333333-0000-0000-0000-000000000002', N'Ức gà luộc (minh họa)', 165.0);

INSERT INTO tuvan.mau_ke_hoach (ma_mau, nhom_doi_tuong, muc_tieu, phien_ban, trang_thai, nguoi_soan, nguoi_duyet) VALUES
 ('44444444-0000-0000-0000-000000000001', N'NguoiTruongThanhKhoeManh', N'DuyTriCanNang', 1, N'DaDuyetMau',
  '11111111-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000003');

INSERT INTO tuvan.tieu_chi_chon_mau (ma_mau, thuoc_tinh, toan_tu, gia_tri) VALUES
 ('44444444-0000-0000-0000-000000000001', N'nhom_doi_tuong', N'=', N'NguoiTruongThanhKhoeManh'),
 ('44444444-0000-0000-0000-000000000001', N'muc_tieu', N'=', N'DuyTriCanNang');
GO
