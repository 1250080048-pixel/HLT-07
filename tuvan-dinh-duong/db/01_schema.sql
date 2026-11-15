-- ============================================================
-- Hệ thống tư vấn dinh dưỡng có chuyên gia duyệt (HLT-07)
-- Chạy trong SSMS, kết nối tới SQL Server. Chạy lần lượt 01 -> 04.
-- ============================================================
IF DB_ID(N'TuVanDinhDuong') IS NULL CREATE DATABASE TuVanDinhDuong;
GO
USE TuVanDinhDuong;
GO
IF SCHEMA_ID(N'tuvan') IS NULL EXEC(N'CREATE SCHEMA tuvan');
GO

CREATE TABLE tuvan.nguoi_dung (
    ma_nguoi_dung UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_nguoi_dung PRIMARY KEY DEFAULT NEWID(),
    subject_oidc  NVARCHAR(100)    NOT NULL CONSTRAINT uq_nguoi_dung_sub UNIQUE,
    ho_ten        NVARCHAR(100)    NOT NULL,
    vai_tro       NVARCHAR(20)     NOT NULL CONSTRAINT ck_vai_tro CHECK (vai_tro IN (N'NguoiDung', N'ChuyenGiaSoan', N'ChuyenGiaDuyet', N'QuanTriVien'))
);

CREATE TABLE tuvan.ho_so_dinh_duong (
    ma_ho_so              UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_ho_so PRIMARY KEY DEFAULT NEWID(),
    ma_nguoi_dung         UNIQUEIDENTIFIER NOT NULL CONSTRAINT uq_ho_so_nguoi UNIQUE
                          CONSTRAINT fk_ho_so_nguoi REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    chieu_cao             DECIMAL(5,1) NULL CONSTRAINT ck_chieu_cao CHECK (chieu_cao BETWEEN 50 AND 250),
    can_nang              DECIMAL(5,1) NULL CONSTRAINT ck_can_nang  CHECK (can_nang BETWEEN 10 AND 300),
    di_ung                NVARCHAR(500) NULL,
    thuoc_nhom_rui_ro_cao BIT NOT NULL CONSTRAINT df_rui_ro DEFAULT 0   -- BR08
);

CREATE TABLE tuvan.nguon_tham_khao (
    ma_nguon       UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_nguon PRIMARY KEY DEFAULT NEWID(),
    ten_nguon      NVARCHAR(200) NOT NULL,
    don_vi_cong_bo NVARCHAR(200) NOT NULL,
    nam_cong_bo    INT NOT NULL CONSTRAINT ck_nam_cong_bo CHECK (nam_cong_bo BETWEEN 1900 AND 2100),
    dang_hoat_dong BIT NOT NULL CONSTRAINT df_nguon_hd DEFAULT 1
);

CREATE TABLE tuvan.ke_hoach_dinh_duong (
    ma_ke_hoach   UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_ke_hoach PRIMARY KEY DEFAULT NEWID(),
    ma_nguoi_dung UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_kh_nguoi REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    ma_nguoi_soan UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_kh_soan REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    phien_ban     INT NOT NULL CONSTRAINT ck_phien_ban CHECK (phien_ban > 0),
    trang_thai    NVARCHAR(20) NOT NULL CONSTRAINT ck_kh_trang_thai CHECK (trang_thai IN
                  (N'Nhap', N'ChoDuyet', N'DaDuyet', N'BiTraLai', N'DangApDung', N'KetThuc', N'Huy')),
    tu_ngay       DATE NOT NULL,
    den_ngay      DATE NOT NULL,
    CONSTRAINT ck_kh_ngay CHECK (den_ngay >= tu_ngay)
);
-- Mỗi người chỉ có một kế hoạch đang áp dụng (chỉ mục duy nhất có điều kiện)
CREATE UNIQUE INDEX ux_kh_ap_dung ON tuvan.ke_hoach_dinh_duong (ma_nguoi_dung)
    WHERE trang_thai = N'DangApDung';

CREATE TABLE tuvan.khuyen_nghi (
    ma_khuyen_nghi UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_khuyen_nghi PRIMARY KEY DEFAULT NEWID(),
    ma_ke_hoach    UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_kn_kh REFERENCES tuvan.ke_hoach_dinh_duong(ma_ke_hoach),
    noi_dung       NVARCHAR(MAX) NOT NULL,
    thu_tu         INT NOT NULL
);

CREATE TABLE tuvan.khuyen_nghi_nguon (          -- BR04: mỗi khuyến nghị có ít nhất một nguồn
    ma_khuyen_nghi UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_knn_kn REFERENCES tuvan.khuyen_nghi(ma_khuyen_nghi),
    ma_nguon       UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_knn_nguon REFERENCES tuvan.nguon_tham_khao(ma_nguon),
    CONSTRAINT pk_khuyen_nghi_nguon PRIMARY KEY (ma_khuyen_nghi, ma_nguon)
);

CREATE TABLE tuvan.phieu_duyet (
    ma_phieu       UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_phieu PRIMARY KEY DEFAULT NEWID(),
    ma_ke_hoach    UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_pd_kh REFERENCES tuvan.ke_hoach_dinh_duong(ma_ke_hoach),
    ma_nguoi_duyet UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_pd_nguoi REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    ket_qua        NVARCHAR(10) NOT NULL CONSTRAINT ck_ket_qua CHECK (ket_qua IN (N'Duyet', N'TraLai')),
    ly_do          NVARCHAR(MAX) NULL,
    thoi_diem      DATETIME2 NOT NULL CONSTRAINT df_pd_tg DEFAULT SYSUTCDATETIME(),
    CONSTRAINT ck_pd_ly_do CHECK (ket_qua = N'Duyet' OR ly_do IS NOT NULL)
);

CREATE TABLE tuvan.mon_an (
    ma_mon_an  UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_mon_an PRIMARY KEY DEFAULT NEWID(),
    ten_mon    NVARCHAR(200) NOT NULL,
    nang_luong DECIMAL(7,1) NULL
);

CREATE TABLE tuvan.nhat_ky_an_uong (
    ma_nhat_ky    UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_nhat_ky PRIMARY KEY DEFAULT NEWID(),
    ma_nguoi_dung UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_nk_nguoi REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    ma_mon_an     UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_nk_mon REFERENCES tuvan.mon_an(ma_mon_an),
    ngay_an       DATE NOT NULL,
    bua_an        NVARCHAR(10) NOT NULL CONSTRAINT ck_bua_an CHECK (bua_an IN (N'Sang', N'Trua', N'Toi', N'Phu')),
    khoi_luong    DECIMAL(7,1) NOT NULL CONSTRAINT ck_khoi_luong CHECK (khoi_luong > 0)
);

CREATE TABLE tuvan.nhat_ky_hieu_chinh (          -- BR09: chỉ thêm, không sửa, không xóa
    ma_nhat_ky      BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT pk_nk_hieu_chinh PRIMARY KEY,
    ten_bang        NVARCHAR(50) NOT NULL,
    ma_ban_ghi      UNIQUEIDENTIFIER NOT NULL,
    hanh_dong       NVARCHAR(30) NOT NULL,
    gia_tri_cu      NVARCHAR(MAX) NULL CONSTRAINT ck_json_cu CHECK (gia_tri_cu IS NULL OR ISJSON(gia_tri_cu) = 1),
    gia_tri_moi     NVARCHAR(MAX) NULL CONSTRAINT ck_json_moi CHECK (gia_tri_moi IS NULL OR ISJSON(gia_tri_moi) = 1),
    nguoi_thuc_hien UNIQUEIDENTIFIER NULL,        -- NULL = hệ thống
    thoi_diem       DATETIME2 NOT NULL CONSTRAINT df_nk_tg DEFAULT SYSUTCDATETIME()
);

CREATE TABLE tuvan.mau_ke_hoach (                -- thư viện mẫu do chuyên gia soạn và duyệt
    ma_mau         UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_mau PRIMARY KEY DEFAULT NEWID(),
    nhom_doi_tuong NVARCHAR(50) NOT NULL,
    muc_tieu       NVARCHAR(50) NOT NULL,
    phien_ban      INT NOT NULL CONSTRAINT ck_mau_pb CHECK (phien_ban > 0),
    trang_thai     NVARCHAR(20) NOT NULL CONSTRAINT ck_mau_tt CHECK (trang_thai IN
                   (N'NhapMau', N'ChoDuyetMau', N'DaDuyetMau', N'BiTraLaiMau', N'NgungDung')),
    nguoi_soan     UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_mau_soan REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    nguoi_duyet    UNIQUEIDENTIFIER NULL     CONSTRAINT fk_mau_duyet REFERENCES tuvan.nguoi_dung(ma_nguoi_dung),
    CONSTRAINT ck_mau_bd CHECK (nguoi_duyet IS NULL OR nguoi_duyet <> nguoi_soan)   -- BR07
);

CREATE TABLE tuvan.tieu_chi_chon_mau (
    ma_tieu_chi UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_tieu_chi PRIMARY KEY DEFAULT NEWID(),
    ma_mau      UNIQUEIDENTIFIER NOT NULL CONSTRAINT fk_tc_mau REFERENCES tuvan.mau_ke_hoach(ma_mau),
    thuoc_tinh  NVARCHAR(50) NOT NULL,
    toan_tu     NVARCHAR(10) NOT NULL CONSTRAINT ck_toan_tu CHECK (toan_tu IN (N'=', N'<=', N'>=', N'IN')),
    gia_tri     NVARCHAR(100) NOT NULL
);

CREATE TABLE tuvan.hang_doi_tu_dong (            -- hàng đợi công việc tự động
    ma_viec       UNIQUEIDENTIFIER NOT NULL CONSTRAINT pk_viec PRIMARY KEY DEFAULT NEWID(),
    loai_viec     NVARCHAR(30) NOT NULL CONSTRAINT ck_loai_viec CHECK (loai_viec IN
                  (N'TaoBanNhap', N'NhacGhiNhatKy', N'TinhTienDo', N'ThongBao')),
    ma_doi_tuong  UNIQUEIDENTIFIER NOT NULL,
    uu_tien       INT NOT NULL CONSTRAINT df_uu_tien DEFAULT 5 CONSTRAINT ck_uu_tien CHECK (uu_tien BETWEEN 1 AND 9),
    trang_thai    NVARCHAR(20) NOT NULL CONSTRAINT df_viec_tt DEFAULT N'Cho' CONSTRAINT ck_viec_tt CHECK (trang_thai IN (N'Cho', N'DangXuLy', N'Xong', N'Loi')),
    so_lan_thu    INT NOT NULL CONSTRAINT df_so_lan DEFAULT 0,
    thoi_diem_tao DATETIME2 NOT NULL CONSTRAINT df_viec_tg DEFAULT SYSUTCDATETIME()
);
CREATE INDEX ix_hang_doi_cho ON tuvan.hang_doi_tu_dong (uu_tien, thoi_diem_tao) WHERE trang_thai = N'Cho';
GO
