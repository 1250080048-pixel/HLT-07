USE TuVanDinhDuong;
GO
IF OBJECT_ID(N'tuvan.muc_tieu') IS NULL
CREATE TABLE tuvan.muc_tieu (
    ma_muc_tieu    NVARCHAR(30)  NOT NULL CONSTRAINT pk_muc_tieu PRIMARY KEY,
    ten_hien_thi   NVARCHAR(100) NOT NULL,
    mo_ta          NVARCHAR(500) NULL,
    dang_hoat_dong BIT NOT NULL CONSTRAINT df_mt_hd DEFAULT 1
);
GO
IF COL_LENGTH(N'tuvan.mon_an', N'nhom_thuc_pham') IS NULL
    ALTER TABLE tuvan.mon_an ADD nhom_thuc_pham NVARCHAR(50) NULL;
GO
IF OBJECT_ID(N'tuvan.thuc_don_mau') IS NULL
CREATE TABLE tuvan.thuc_don_mau (
    ma_muc_tieu NVARCHAR(30) NOT NULL CONSTRAINT fk_tdm_mt REFERENCES tuvan.muc_tieu(ma_muc_tieu),
    bua_an      NVARCHAR(10) NOT NULL CONSTRAINT ck_tdm_bua CHECK (bua_an IN (N'Sang', N'Trua', N'Toi', N'Phu')),
    ma_mon_an   UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_tdm_mon REFERENCES tuvan.mon_an(ma_mon_an),
    khau_phan   NVARCHAR(100) NOT NULL,
    nguoi_duyet UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_tdm_duyet REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    CONSTRAINT pk_thuc_don_mau PRIMARY KEY (ma_muc_tieu, bua_an, ma_mon_an)
);
GO
IF OBJECT_ID(N'tuvan.ke_hoach_thuc_don') IS NULL
CREATE TABLE tuvan.ke_hoach_thuc_don (
    ma_ke_hoach UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_khtd_kh REFERENCES tuvan.ke_hoach_dinh_duong(ma_ke_hoach),
    bua_an      NVARCHAR(10) NOT NULL CONSTRAINT ck_khtd_bua CHECK (bua_an IN (N'Sang', N'Trua', N'Toi', N'Phu')),
    ma_mon_an   UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_khtd_mon REFERENCES tuvan.mon_an(ma_mon_an),
    khau_phan   NVARCHAR(100) NOT NULL,
    CONSTRAINT pk_ke_hoach_thuc_don PRIMARY KEY (ma_ke_hoach, bua_an, ma_mon_an)
);
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'ck_yc_loai')
    ALTER TABLE tuvan.yeu_cau_tu_van DROP CONSTRAINT ck_yc_loai;
IF COL_LENGTH(N'tuvan.yeu_cau_tu_van', N'ma_muc_tieu') IS NULL
    ALTER TABLE tuvan.yeu_cau_tu_van ADD ma_muc_tieu NVARCHAR(30) NULL
        CONSTRAINT fk_yc_mt REFERENCES tuvan.muc_tieu(ma_muc_tieu);
ALTER TABLE tuvan.yeu_cau_tu_van ADD CONSTRAINT ck_yc_loai
    CHECK (loai IN (N'GiamCan', N'MucTieu', N'KeHoachRieng'));
GO
INSERT INTO tuvan.muc_tieu (ma_muc_tieu, ten_hien_thi, mo_ta)
SELECT v.ma, v.ten, v.mota FROM (VALUES
    (N'GiamCan', N'Giảm cân',           N'Thực đơn tham khảo cho giảm cân lành mạnh, kết hợp vận động đều đặn.'),
    (N'TangCan', N'Tăng cân lành mạnh', N'Thực đơn tham khảo tăng năng lượng từ thực phẩm lành mạnh.'),
    (N'TangCo',  N'Tăng cơ',            N'Thực đơn tham khảo giàu đạm, kết hợp luyện tập sức mạnh.'),
    (N'DuyTri',  N'Duy trì cân nặng',   N'Thực đơn tham khảo cân bằng để giữ cân nặng ổn định.')
) AS v(ma, ten, mota)
WHERE NOT EXISTS (SELECT 1 FROM tuvan.muc_tieu m WHERE m.ma_muc_tieu = v.ma);
GO
INSERT INTO tuvan.mon_an (ma_mon_an, ten_mon, nang_luong, nhom_thuc_pham)
SELECT CAST(v.ma AS UNIQUEIDENTIFIER), v.ten, v.nl, v.nhom FROM (VALUES
    ('33333333-0000-0000-0000-000000000001', N'Cơm gạo lứt (minh họa)', 110.0, N'Tinh bột'),
    ('33333333-0000-0000-0000-000000000002', N'Ức gà luộc (minh họa)', 165.0, N'Đạm'),
    ('33333333-0000-0000-0000-000000000003', N'Phở gà ít dầu (minh họa)', 350.0, N'Tinh bột'),
    ('33333333-0000-0000-0000-000000000004', N'Bánh mì trứng nguyên cám (minh họa)', 280.0, N'Tinh bột'),
    ('33333333-0000-0000-0000-000000000005', N'Yến mạch nấu sữa ít đường (minh họa)', 230.0, N'Tinh bột'),
    ('33333333-0000-0000-0000-000000000006', N'Canh rau củ (minh họa)', 60.0, N'Rau củ'),
    ('33333333-0000-0000-0000-000000000007', N'Cá hấp gừng (minh họa)', 180.0, N'Đạm'),
    ('33333333-0000-0000-0000-000000000008', N'Đậu hũ sốt cà chua (minh họa)', 170.0, N'Đạm'),
    ('33333333-0000-0000-0000-000000000009', N'Sữa chua không đường (minh họa)', 90.0, N'Sữa'),
    ('33333333-0000-0000-0000-000000000010', N'Trái cây tươi (minh họa)', 60.0, N'Trái cây'),
    ('33333333-0000-0000-0000-000000000011', N'Rau luộc các loại (minh họa)', 40.0, N'Rau củ'),
    ('33333333-0000-0000-0000-000000000012', N'Thịt bò xào rau (minh họa)', 260.0, N'Đạm'),
    ('33333333-0000-0000-0000-000000000013', N'Trứng luộc (minh họa)', 80.0, N'Đạm'),
    ('33333333-0000-0000-0000-000000000014', N'Khoai lang luộc (minh họa)', 120.0, N'Tinh bột'),
    ('33333333-0000-0000-0000-000000000015', N'Sữa đậu nành không đường (minh họa)', 80.0, N'Sữa')
) AS v(ma, ten, nl, nhom)
WHERE NOT EXISTS (SELECT 1 FROM tuvan.mon_an m WHERE m.ma_mon_an = CAST(v.ma AS UNIQUEIDENTIFIER));
GO
INSERT INTO tuvan.thuc_don_mau (ma_muc_tieu, bua_an, ma_mon_an, khau_phan, nguoi_duyet)
SELECT v.mt, v.bua, CAST(v.mon AS UNIQUEIDENTIFIER), v.kp, '11111111-0000-0000-0000-000000000003'
FROM (VALUES
    (N'GiamCan', N'Sang', '33333333-0000-0000-0000-000000000005', N'1 chén nhỏ'),
    (N'GiamCan', N'Sang', '33333333-0000-0000-0000-000000000013', N'1 quả'),
    (N'GiamCan', N'Trua', '33333333-0000-0000-0000-000000000001', N'1/2 chén'),
    (N'GiamCan', N'Trua', '33333333-0000-0000-0000-000000000002', N'100 g'),
    (N'GiamCan', N'Trua', '33333333-0000-0000-0000-000000000011', N'1 đĩa'),
    (N'GiamCan', N'Toi',  '33333333-0000-0000-0000-000000000006', N'1 chén'),
    (N'GiamCan', N'Toi',  '33333333-0000-0000-0000-000000000007', N'120 g'),
    (N'GiamCan', N'Phu',  '33333333-0000-0000-0000-000000000009', N'1 hộp'),
    (N'GiamCan', N'Phu',  '33333333-0000-0000-0000-000000000010', N'1 phần'),
    (N'TangCan', N'Sang', '33333333-0000-0000-0000-000000000004', N'2 lát'),
    (N'TangCan', N'Sang', '33333333-0000-0000-0000-000000000015', N'1 ly'),
    (N'TangCan', N'Trua', '33333333-0000-0000-0000-000000000001', N'2 chén'),
    (N'TangCan', N'Trua', '33333333-0000-0000-0000-000000000012', N'120 g'),
    (N'TangCan', N'Toi',  '33333333-0000-0000-0000-000000000007', N'120 g'),
    (N'TangCan', N'Toi',  '33333333-0000-0000-0000-000000000014', N'1 củ nhỏ'),
    (N'TangCan', N'Phu',  '33333333-0000-0000-0000-000000000010', N'1 phần'),
    (N'TangCan', N'Phu',  '33333333-0000-0000-0000-000000000009', N'1 hộp'),
    (N'TangCo',  N'Sang', '33333333-0000-0000-0000-000000000013', N'2 quả'),
    (N'TangCo',  N'Sang', '33333333-0000-0000-0000-000000000005', N'1 chén'),
    (N'TangCo',  N'Trua', '33333333-0000-0000-0000-000000000002', N'150 g'),
    (N'TangCo',  N'Trua', '33333333-0000-0000-0000-000000000001', N'1 chén'),
    (N'TangCo',  N'Trua', '33333333-0000-0000-0000-000000000008', N'1 phần'),
    (N'TangCo',  N'Toi',  '33333333-0000-0000-0000-000000000012', N'150 g'),
    (N'TangCo',  N'Toi',  '33333333-0000-0000-0000-000000000006', N'1 chén'),
    (N'TangCo',  N'Phu',  '33333333-0000-0000-0000-000000000009', N'1 hộp'),
    (N'TangCo',  N'Phu',  '33333333-0000-0000-0000-000000000015', N'1 ly'),
    (N'DuyTri',  N'Sang', '33333333-0000-0000-0000-000000000003', N'1 tô'),
    (N'DuyTri',  N'Trua', '33333333-0000-0000-0000-000000000001', N'1 chén'),
    (N'DuyTri',  N'Trua', '33333333-0000-0000-0000-000000000007', N'100 g'),
    (N'DuyTri',  N'Trua', '33333333-0000-0000-0000-000000000011', N'1 đĩa'),
    (N'DuyTri',  N'Toi',  '33333333-0000-0000-0000-000000000008', N'1 phần'),
    (N'DuyTri',  N'Toi',  '33333333-0000-0000-0000-000000000006', N'1 chén'),
    (N'DuyTri',  N'Phu',  '33333333-0000-0000-0000-000000000010', N'1 phần')
) AS v(mt, bua, mon, kp)
WHERE NOT EXISTS (
    SELECT 1 FROM tuvan.thuc_don_mau t
     WHERE t.ma_muc_tieu = v.mt AND t.bua_an = v.bua AND t.ma_mon_an = CAST(v.mon AS UNIQUEIDENTIFIER));
GO