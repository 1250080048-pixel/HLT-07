USE TuVanDinhDuong;
GO
-- Khẩu phần ghi từ thực đơn (có giá trị = món được tích từ thực đơn; NULL = ghi tự do)
IF COL_LENGTH(N'tuvan.nhat_ky_an_uong', N'khau_phan') IS NULL
    ALTER TABLE tuvan.nhat_ky_an_uong ADD khau_phan NVARCHAR(100) NULL;
GO