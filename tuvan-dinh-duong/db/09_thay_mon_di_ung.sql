USE TuVanDinhDuong;
GO
IF COL_LENGTH(N'tuvan.ho_so_dinh_duong', N'di_ung_nhom') IS NULL
    ALTER TABLE tuvan.ho_so_dinh_duong ADD di_ung_nhom NVARCHAR(200) NULL;
IF COL_LENGTH(N'tuvan.mon_an', N'dau_hieu_di_ung') IS NULL
    ALTER TABLE tuvan.mon_an ADD dau_hieu_di_ung NVARCHAR(200) NULL;
IF COL_LENGTH(N'tuvan.ke_hoach_thuc_don', N'ma_mon_goc') IS NULL
    ALTER TABLE tuvan.ke_hoach_thuc_don ADD ma_mon_goc UNIQUEIDENTIFIER NULL
        CONSTRAINT fk_khtd_mon_goc REFERENCES tuvan.mon_an(ma_mon_an);
GO
IF OBJECT_ID(N'tuvan.mon_thay_the') IS NULL
CREATE TABLE tuvan.mon_thay_the (
    ma_mon_goc  UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mtt_goc REFERENCES tuvan.mon_an(ma_mon_an),
    ma_mon_thay UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mtt_thay REFERENCES tuvan.mon_an(ma_mon_an),
    nhom_di_ung NVARCHAR(30) NOT NULL,
    khau_phan   NVARCHAR(100) NOT NULL,
    nguoi_duyet UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mtt_duyet REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    CONSTRAINT ck_mtt_khac CHECK (ma_mon_goc <> ma_mon_thay),
    CONSTRAINT pk_mon_thay_the PRIMARY KEY (ma_mon_goc, ma_mon_thay, nhom_di_ung)
);
GO
-- Gắn nhóm dị ứng cho món minh họa
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'HaiSan'     WHERE ma_mon_an = '33333333-0000-0000-0000-000000000007';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'Trung,Gluten' WHERE ma_mon_an = '33333333-0000-0000-0000-000000000004';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'Trung'      WHERE ma_mon_an = '33333333-0000-0000-0000-000000000013';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'Sua'        WHERE ma_mon_an = '33333333-0000-0000-0000-000000000009';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'DauNanh'    WHERE ma_mon_an = '33333333-0000-0000-0000-000000000015';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'DauNanh'    WHERE ma_mon_an = '33333333-0000-0000-0000-000000000008';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'Gluten'     WHERE ma_mon_an = '33333333-0000-0000-0000-000000000005';
UPDATE tuvan.mon_an SET dau_hieu_di_ung = N'Gluten'     WHERE ma_mon_an = '33333333-0000-0000-0000-000000000003';
GO
-- Món thay thế đã duyệt (người duyệt: duyet01). Khẩu phần là khẩu phần của món thay.
INSERT INTO tuvan.mon_thay_the (ma_mon_goc, ma_mon_thay, nhom_di_ung, khau_phan, nguoi_duyet)
SELECT v.goc, v.thay, v.nhom, v.kp, '11111111-0000-0000-0000-000000000003'
FROM (VALUES
    ('33333333-0000-0000-0000-000000000007', '33333333-0000-0000-0000-000000000002', N'HaiSan',  N'120 g'),
    ('33333333-0000-0000-0000-000000000007', '33333333-0000-0000-0000-000000000008', N'HaiSan',  N'120 g'),
    ('33333333-0000-0000-0000-000000000008', '33333333-0000-0000-0000-000000000002', N'DauNanh', N'120 g'),
    ('33333333-0000-0000-0000-000000000013', '33333333-0000-0000-0000-000000000002', N'Trung',   N'100 g'),
    ('33333333-0000-0000-0000-000000000009', '33333333-0000-0000-0000-000000000015', N'Sua',     N'1 ly'),
    ('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000001', N'Gluten',  N'1 chén'),
    ('33333333-0000-0000-0000-000000000004', '33333333-0000-0000-0000-000000000001', N'Trung',   N'1 chén'),
    ('33333333-0000-0000-0000-000000000005', '33333333-0000-0000-0000-000000000001', N'Gluten',  N'1 chén'),
    ('33333333-0000-0000-0000-000000000003', '33333333-0000-0000-0000-000000000001', N'Gluten',  N'2 chén')
) AS v(goc, thay, nhom, kp)
WHERE NOT EXISTS (
    SELECT 1 FROM tuvan.mon_thay_the t
     WHERE t.ma_mon_goc = CAST(v.goc AS UNIQUEIDENTIFIER)
       AND t.ma_mon_thay = CAST(v.thay AS UNIQUEIDENTIFIER)
       AND t.nhom_di_ung = v.nhom);
GO

