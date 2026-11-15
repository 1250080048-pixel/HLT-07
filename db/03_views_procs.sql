USE TuVanDinhDuong;
GO
-- Tiến độ theo kế hoạch đang áp dụng
CREATE OR ALTER VIEW tuvan.vw_tien_do AS
SELECT k.ma_ke_hoach,
       k.ma_nguoi_dung,
       DATEDIFF(DAY, k.tu_ngay, k.den_ngay) + 1 AS tong_ngay,
       COUNT(DISTINCT n.ngay_an)                 AS so_ngay_da_ghi,
       CAST(100.0 * COUNT(DISTINCT n.ngay_an)
            / NULLIF(DATEDIFF(DAY, k.tu_ngay, k.den_ngay) + 1, 0) AS DECIMAL(5,1)) AS ty_le_phan_tram
  FROM tuvan.ke_hoach_dinh_duong k
  LEFT JOIN tuvan.nhat_ky_an_uong n
         ON n.ma_nguoi_dung = k.ma_nguoi_dung
        AND n.ngay_an BETWEEN k.tu_ngay AND k.den_ngay
 WHERE k.trang_thai = N'DangApDung'
 GROUP BY k.ma_ke_hoach, k.ma_nguoi_dung, k.tu_ngay, k.den_ngay;
GO

-- Lấy việc từ hàng đợi an toàn khi chạy song song (READPAST + UPDLOCK)
CREATE OR ALTER PROCEDURE tuvan.sp_lay_viec_tu_dong
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    DECLARE @viec TABLE (ma_viec UNIQUEIDENTIFIER, loai_viec NVARCHAR(30), ma_doi_tuong UNIQUEIDENTIFIER);
    UPDATE v SET trang_thai = N'DangXuLy', so_lan_thu = so_lan_thu + 1
    OUTPUT inserted.ma_viec, inserted.loai_viec, inserted.ma_doi_tuong INTO @viec
    FROM (SELECT TOP (10) ma_viec FROM tuvan.hang_doi_tu_dong WITH (UPDLOCK, READPAST, ROWLOCK)
           WHERE trang_thai = N'Cho' AND thoi_diem_tao <= SYSUTCDATETIME()
           ORDER BY uu_tien, thoi_diem_tao) AS pick
    JOIN tuvan.hang_doi_tu_dong v ON v.ma_viec = pick.ma_viec;
    COMMIT;
    SELECT ma_viec, loai_viec, ma_doi_tuong FROM @viec;
END;
GO

-- Tạo bản nháp từ mẫu đã duyệt (BR06, BR08)
CREATE OR ALTER PROCEDURE tuvan.sp_tao_ban_nhap_tu_mau
    @ma_nguoi_dung UNIQUEIDENTIFIER,
    @ma_nguoi_soan UNIQUEIDENTIFIER,
    @nhom_doi_tuong NVARCHAR(50),
    @muc_tieu NVARCHAR(50),
    @so_ngay INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM tuvan.ho_so_dinh_duong WHERE ma_nguoi_dung = @ma_nguoi_dung AND thuoc_nhom_rui_ro_cao = 1)
    BEGIN
        THROW 50002, N'BR08: nhom rui ro cao phai soan thu cong.', 1;
    END;
    DECLARE @ma_mau UNIQUEIDENTIFIER = (
        SELECT TOP (1) m.ma_mau
          FROM tuvan.mau_ke_hoach m
         WHERE m.trang_thai = N'DaDuyetMau'
           AND m.nhom_doi_tuong = @nhom_doi_tuong
           AND m.muc_tieu = @muc_tieu
         ORDER BY m.phien_ban DESC);
    IF @ma_mau IS NULL THROW 50003, N'Khong co mau da duyet khop.', 1;

    DECLARE @ma_kh UNIQUEIDENTIFIER = NEWID();
    INSERT INTO tuvan.ke_hoach_dinh_duong (ma_ke_hoach, ma_nguoi_dung, ma_nguoi_soan, phien_ban, trang_thai, tu_ngay, den_ngay)
    VALUES (@ma_kh, @ma_nguoi_dung, @ma_nguoi_soan, 1, N'Nhap', CAST(SYSUTCDATETIME() AS DATE), DATEADD(DAY, @so_ngay, CAST(SYSUTCDATETIME() AS DATE)));

    INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
    VALUES (N'ke_hoach_dinh_duong', @ma_kh, N'TaoTuMau', CONCAT(N'{"ma_mau":"', CAST(@ma_mau AS NVARCHAR(36)), N'"}'), NULL);

    SELECT @ma_kh AS ma_ke_hoach;
END;
GO
