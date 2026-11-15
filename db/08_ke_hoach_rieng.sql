USE TuVanDinhDuong;
GO
IF COL_LENGTH(N'tuvan.yeu_cau_tu_van', N'ghi_chu') IS NULL
    ALTER TABLE tuvan.yeu_cau_tu_van ADD ghi_chu NVARCHAR(500) NULL;
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'ck_yc_loai')
    ALTER TABLE tuvan.yeu_cau_tu_van DROP CONSTRAINT ck_yc_loai;
GO
ALTER TABLE tuvan.yeu_cau_tu_van ADD CONSTRAINT ck_yc_loai
    CHECK (loai IN (N'GiamCan', N'MucTieu', N'KeHoachRieng'));
GO