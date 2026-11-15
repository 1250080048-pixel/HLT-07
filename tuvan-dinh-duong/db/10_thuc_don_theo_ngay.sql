USE TuVanDinhDuong;
GO
IF COL_LENGTH(N'tuvan.thuc_don_mau', N'ngay_thu') IS NULL
    ALTER TABLE tuvan.thuc_don_mau ADD ngay_thu INT NOT NULL
        CONSTRAINT df_tdm_ngay DEFAULT 1
        CONSTRAINT ck_tdm_ngay CHECK (ngay_thu BETWEEN 1 AND 7);
IF COL_LENGTH(N'tuvan.ke_hoach_thuc_don', N'ngay_thu') IS NULL
    ALTER TABLE tuvan.ke_hoach_thuc_don ADD ngay_thu INT NOT NULL
        CONSTRAINT df_khtd_ngay DEFAULT 1
        CONSTRAINT ck_khtd_ngay CHECK (ngay_thu BETWEEN 1 AND 7);
GO
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'pk_thuc_don_mau')
    ALTER TABLE tuvan.thuc_don_mau DROP CONSTRAINT pk_thuc_don_mau;
ALTER TABLE tuvan.thuc_don_mau ADD CONSTRAINT pk_thuc_don_mau
    PRIMARY KEY (ma_muc_tieu, ngay_thu, bua_an, ma_mon_an);
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'pk_ke_hoach_thuc_don')
    ALTER TABLE tuvan.ke_hoach_thuc_don DROP CONSTRAINT pk_ke_hoach_thuc_don;
ALTER TABLE tuvan.ke_hoach_thuc_don ADD CONSTRAINT pk_ke_hoach_thuc_don
    PRIMARY KEY (ma_ke_hoach, ngay_thu, bua_an, ma_mon_an);
GO
-- Ngày 2 đến 7: mỗi bữa lấy 1 món theo vòng xoay từ các món của ngày 1
;WITH goc AS (
    SELECT ma_muc_tieu, bua_an, ma_mon_an, khau_phan, nguoi_duyet,
           ROW_NUMBER() OVER (PARTITION BY ma_muc_tieu, bua_an ORDER BY ma_mon_an) AS k,
           COUNT(*) OVER (PARTITION BY ma_muc_tieu, bua_an) AS n
      FROM tuvan.thuc_don_mau
     WHERE ngay_thu = 1
)
INSERT INTO tuvan.thuc_don_mau (ma_muc_tieu, ngay_thu, bua_an, ma_mon_an, khau_phan, nguoi_duyet)
SELECT g.ma_muc_tieu, d.ngay, g.bua_an, g.ma_mon_an, g.khau_phan, g.nguoi_duyet
  FROM goc g
  CROSS JOIN (VALUES (2), (3), (4), (5), (6), (7)) AS d(ngay)
 WHERE g.k = ((d.ngay - 1) % g.n) + 1
   AND NOT EXISTS (
        SELECT 1 FROM tuvan.thuc_don_mau x
         WHERE x.ma_muc_tieu = g.ma_muc_tieu AND x.ngay_thu = d.ngay
           AND x.bua_an = g.bua_an AND x.ma_mon_an = g.ma_mon_an);
GO

