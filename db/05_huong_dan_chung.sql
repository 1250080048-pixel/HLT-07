USE TuVanDinhDuong;
GO
IF OBJECT_ID(N'tuvan.mau_khuyen_nghi') IS NULL
CREATE TABLE tuvan.mau_khuyen_nghi (
    ma_mau    UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mkn_mau REFERENCES tuvan.mau_ke_hoach(ma_mau),
    thu_tu    INT NOT NULL CONSTRAINT ck_mkn_tt CHECK (thu_tu > 0),
    noi_dung  NVARCHAR(MAX) NOT NULL,
    ma_nguon  UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mkn_nguon REFERENCES tuvan.nguon_tham_khao(ma_nguon),
    CONSTRAINT pk_mau_khuyen_nghi PRIMARY KEY (ma_mau, thu_tu)
);
GO
IF OBJECT_ID(N'tuvan.yeu_cau_tu_van') IS NULL
CREATE TABLE tuvan.yeu_cau_tu_van (
    ma_yeu_cau    UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_yeu_cau PRIMARY KEY DEFAULT NEWID(),
    ma_nguoi_dung UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_yc_nguoi REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    loai          NVARCHAR(30) NOT NULL CONSTRAINT ck_yc_loai CHECK (loai IN (N'GiamCan')),
    trang_thai    NVARCHAR(20) NOT NULL CONSTRAINT df_yc_tt DEFAULT N'ChoChuyenGia'
                  CONSTRAINT ck_yc_tt CHECK (trang_thai IN (N'ChoChuyenGia', N'DaXuLy')),
    thoi_diem     DATETIME2 NOT NULL CONSTRAINT df_yc_tg DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS (SELECT 1 FROM tuvan.mau_ke_hoach WHERE nhom_doi_tuong = N'GiamCanChung')
BEGIN
    DECLARE @m UNIQUEIDENTIFIER = '55555555-0000-0000-0000-000000000001';
    INSERT INTO tuvan.mau_ke_hoach (ma_mau, nhom_doi_tuong, muc_tieu, phien_ban, trang_thai, nguoi_soan, nguoi_duyet)
    VALUES (@m, N'GiamCanChung', N'GiamCan', 1, N'DaDuyetMau',
            '11111111-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000003');

    INSERT INTO tuvan.mau_khuyen_nghi (ma_mau, thu_tu, noi_dung, ma_nguon) VALUES
(@m, 1, N'Ưu tiên rau xanh và thực phẩm ít năng lượng trong mỗi bữa ăn.', '22222222-0000-0000-0000-000000000001'),    (@m, 2, N'Uống đủ nước; hạn chế đồ uống có đường.', '22222222-0000-0000-0000-000000000001'),
    (@m, 3, N'Vận động đều đặn và tăng dần cường độ. Đây là hướng dẫn chung, không thay thế tư vấn cá nhân.', '22222222-0000-0000-0000-000000000001');
END
GO