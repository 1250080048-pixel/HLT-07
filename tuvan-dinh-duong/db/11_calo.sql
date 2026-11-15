USE TuVanDinhDuong;
GO
-- 1. Chuẩn hóa: kcal trên 100 g
IF COL_LENGTH(N'tuvan.mon_an', N'kcal_100g') IS NULL
    ALTER TABLE tuvan.mon_an ADD kcal_100g DECIMAL(7,1) NULL;
GO
UPDATE tuvan.mon_an SET kcal_100g = nang_luong WHERE kcal_100g IS NULL;
GO
-- Ước lượng minh họa các món mà nang_luong cũ là kcal theo khẩu phần.
-- Chuyên gia phải đối chiếu lại với bảng thành phần thực phẩm trước khi dùng thật.
UPDATE tuvan.mon_an SET kcal_100g = 65  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000003';
UPDATE tuvan.mon_an SET kcal_100g = 265 WHERE ma_mon_an = '33333333-0000-0000-0000-000000000004';
UPDATE tuvan.mon_an SET kcal_100g = 70  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000005';
UPDATE tuvan.mon_an SET kcal_100g = 120 WHERE ma_mon_an = '33333333-0000-0000-0000-000000000007';
UPDATE tuvan.mon_an SET kcal_100g = 155 WHERE ma_mon_an = '33333333-0000-0000-0000-000000000013';
UPDATE tuvan.mon_an SET kcal_100g = 30  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000006';
UPDATE tuvan.mon_an SET kcal_100g = 60  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000009';
UPDATE tuvan.mon_an SET kcal_100g = 50  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000010';
UPDATE tuvan.mon_an SET kcal_100g = 25  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000011';
UPDATE tuvan.mon_an SET kcal_100g = 200 WHERE ma_mon_an = '33333333-0000-0000-0000-000000000012';
UPDATE tuvan.mon_an SET kcal_100g = 100 WHERE ma_mon_an = '33333333-0000-0000-0000-000000000008';
UPDATE tuvan.mon_an SET kcal_100g = 40  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000015';
UPDATE tuvan.mon_an SET kcal_100g = 86  WHERE ma_mon_an = '33333333-0000-0000-0000-000000000014';
GO
-- 2. Mốc tham chiếu do chuyên gia duyệt, theo mục tiêu (dữ liệu minh họa, cần thay bằng nguồn đã kiểm chứng)
IF OBJECT_ID(N'tuvan.calo_tham_chieu') IS NULL
CREATE TABLE tuvan.calo_tham_chieu (
    ma_muc_tieu  NVARCHAR(30) NOT NULL CONSTRAINT pk_calo_tc PRIMARY KEY CONSTRAINT fk_calo_mt REFERENCES tuvan.muc_tieu(ma_muc_tieu),
    kcal_min     INT NOT NULL,
    kcal_max     INT NOT NULL,
    nguon        NVARCHAR(200) NOT NULL,
    nguoi_duyet  UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_calo_duyet REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    ngay_duyet   DATETIME2 NOT NULL CONSTRAINT df_calo_ngay DEFAULT SYSUTCDATETIME(),
    CONSTRAINT ck_calo_khoang CHECK (kcal_min > 0 AND kcal_min <= kcal_max)
);
GO
INSERT INTO tuvan.calo_tham_chieu (ma_muc_tieu, kcal_min, kcal_max, nguon, nguoi_duyet)
SELECT v.mt, v.lo, v.hi, v.nguon, '11111111-0000-0000-0000-000000000003'
FROM (VALUES
    (N'GiamCan', 1400, 1800, N'Minh họa: chuyên gia thay bằng nguồn đã kiểm chứng'),
    (N'TangCan', 2200, 2600, N'Minh họa: chuyên gia thay bằng nguồn đã kiểm chứng'),
    (N'TangCo',  2000, 2400, N'Minh họa: chuyên gia thay bằng nguồn đã kiểm chứng'),
    (N'DuyTri',  1800, 2200, N'Minh họa: chuyên gia thay bằng nguồn đã kiểm chứng')
) AS v(mt, lo, hi, nguon)
WHERE NOT EXISTS (SELECT 1 FROM tuvan.calo_tham_chieu c WHERE c.ma_muc_tieu = v.mt);
GO
-- 3. Thông báo cố định do chuyên gia viết sẵn. Hệ thống không sinh nội dung mới.
IF OBJECT_ID(N'tuvan.thong_bao_calo') IS NULL
CREATE TABLE tuvan.thong_bao_calo (
    loai       NVARCHAR(10) NOT NULL CONSTRAINT pk_tb_calo PRIMARY KEY CONSTRAINT ck_tb_loai CHECK (loai IN (N'thap', N'cao')),
    noi_dung   NVARCHAR(500) NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM tuvan.thong_bao_calo)
INSERT INTO tuvan.thong_bao_calo (loai, noi_dung) VALUES
 (N'thap', N'Tổng năng lượng ghi nhận hôm nay thấp hơn khoảng tham khảo. Nếu lặp lại nhiều ngày, hãy trao đổi với chuyên gia.'),
 (N'cao',  N'Tổng năng lượng ghi nhận hôm nay cao hơn khoảng tham khảo. Nếu lặp lại nhiều ngày, hãy trao đổi với chuyên gia.');
GO
-- 4. Tổng calo nạp vào theo ngày của mỗi khách
CREATE OR ALTER VIEW tuvan.vw_calo_theo_ngay AS
SELECT n.ma_nguoi_dung,
       n.ngay_an,
       SUM(m.kcal_100g * n.khoi_luong / 100.0) AS tong_kcal
  FROM tuvan.nhat_ky_an_uong n
  JOIN tuvan.mon_an m ON m.ma_mon_an = n.ma_mon_an
 GROUP BY n.ma_nguoi_dung, n.ngay_an;
GO