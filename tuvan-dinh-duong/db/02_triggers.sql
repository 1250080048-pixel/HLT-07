USE TuVanDinhDuong;
GO
-- BR09: nhật ký hiệu chỉnh không được sửa hoặc xóa
CREATE OR ALTER TRIGGER tuvan.tg_chan_sua_nhat_ky ON tuvan.nhat_ky_hieu_chinh
INSTEAD OF UPDATE, DELETE AS
BEGIN
    THROW 50001, N'Nhat ky hieu chinh khong duoc sua hoac xoa.', 1;
END;
GO
-- Đưa việc vào hàng đợi khi kế hoạch được tạo hoặc đổi trạng thái
CREATE OR ALTER TRIGGER tuvan.tg_ke_hoach_hang_doi ON tuvan.ke_hoach_dinh_duong
AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    -- Bản nháp mới: vào hàng đợi cho chuyên gia duyệt
    INSERT INTO tuvan.hang_doi_tu_dong (loai_viec, ma_doi_tuong, uu_tien)
    SELECT N'TaoBanNhap', i.ma_ke_hoach, 3
      FROM inserted i
     WHERE i.trang_thai = N'Nhap'
       AND NOT EXISTS (SELECT 1 FROM deleted d WHERE d.ma_ke_hoach = i.ma_ke_hoach);
    -- Kết quả duyệt: thông báo cho khách hàng
    INSERT INTO tuvan.hang_doi_tu_dong (loai_viec, ma_doi_tuong, uu_tien)
    SELECT N'ThongBao', i.ma_ke_hoach, 2
      FROM inserted i
      JOIN deleted d ON d.ma_ke_hoach = i.ma_ke_hoach
     WHERE i.trang_thai <> d.trang_thai
       AND i.trang_thai IN (N'DaDuyet', N'BiTraLai');
END;
GO
