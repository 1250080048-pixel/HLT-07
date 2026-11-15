USE TuVanDinhDuong;
GO
IF COL_LENGTH(N'tuvan.ke_hoach_dinh_duong', N'ma_muc_tieu') IS NULL
    ALTER TABLE tuvan.ke_hoach_dinh_duong ADD ma_muc_tieu NVARCHAR(30) NULL
        CONSTRAINT fk_kh_mt REFERENCES tuvan.muc_tieu(ma_muc_tieu);
GO
IF NOT EXISTS (SELECT 1 FROM tuvan.nguoi_dung WHERE ma_nguoi_dung = '11111111-0000-0000-0000-000000000099')
    INSERT INTO tuvan.nguoi_dung (ma_nguoi_dung, subject_oidc, ho_ten, vai_tro)
    VALUES ('11111111-0000-0000-0000-000000000099', N'he-thong', N'Hệ thống (tự động)', N'QuanTriVien');
GO