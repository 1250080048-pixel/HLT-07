using Dapper;

namespace TuVan.Api.Data;

public sealed record KeHoachDuyet(Guid MaKeHoach, Guid MaNguoiDung, Guid MaNguoiSoan, int PhienBan,
    string TrangThai, DateTime TuNgay, DateTime DenNgay, string? MaMucTieu);
public sealed record HoSoKhach(string HoTen, decimal? ChieuCao, decimal? CanNang, string? DiUng, bool ThuocNhomRuiRoCao);
public sealed record MonDuyet(string BuaAn, Guid MaMonAn, string TenMon, string KhauPhan, string NhomThucPham, decimal? NangLuong);
public sealed record KhuyenNghiNguon(Guid MaKhuyenNghi, string NoiDung, int ThuTu, string? TenNguon, string? DonViCongBo, int? NamCongBo);

/// <summary>Dữ liệu chuyên gia duyệt cần để thẩm định kế hoạch.</summary>
public sealed class DuyetRepository(Db db)
{
    public async Task<KeHoachDuyet?> KeHoachAsync(Guid maKeHoach)
    {
        using var c = await db.OpenAsync();
        return await c.QuerySingleOrDefaultAsync<KeHoachDuyet>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung, ma_nguoi_soan AS MaNguoiSoan,
                   phien_ban AS PhienBan, trang_thai AS TrangThai, tu_ngay AS TuNgay, den_ngay AS DenNgay,
                   ma_muc_tieu AS MaMucTieu
              FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_ke_hoach = @maKeHoach;", new { maKeHoach });
    }

    public async Task<HoSoKhach?> HoSoKhachAsync(Guid maNguoiDung)
    {
        using var c = await db.OpenAsync();
        return await c.QuerySingleOrDefaultAsync<HoSoKhach>(@"
            SELECT n.ho_ten AS HoTen, h.chieu_cao AS ChieuCao, h.can_nang AS CanNang,
                   h.di_ung AS DiUng,
                   CAST(ISNULL(h.thuoc_nhom_rui_ro_cao, 0) AS BIT) AS ThuocNhomRuiRoCao
              FROM tuvan.nguoi_dung n
              LEFT JOIN tuvan.ho_so_dinh_duong h ON h.ma_nguoi_dung = n.ma_nguoi_dung
             WHERE n.ma_nguoi_dung = @maNguoiDung;", new { maNguoiDung });
    }

    public async Task<IEnumerable<MonDuyet>> ThucDonAsync(Guid maKeHoach)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MonDuyet>(@"
            SELECT k.bua_an AS BuaAn, m.ma_mon_an AS MaMonAn, m.ten_mon AS TenMon,
                   k.khau_phan AS KhauPhan, ISNULL(m.nhom_thuc_pham, N'') AS NhomThucPham,
                   m.nang_luong AS NangLuong
              FROM tuvan.ke_hoach_thuc_don k
              JOIN tuvan.mon_an m ON m.ma_mon_an = k.ma_mon_an
             WHERE k.ma_ke_hoach = @maKeHoach
             ORDER BY CASE k.bua_an WHEN 'Sang' THEN 1 WHEN 'Trua' THEN 2 WHEN 'Toi' THEN 3 ELSE 4 END,
                      m.ten_mon;", new { maKeHoach });
    }

    public async Task<IEnumerable<KhuyenNghiNguon>> KhuyenNghiAsync(Guid maKeHoach)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<KhuyenNghiNguon>(@"
            SELECT kn.ma_khuyen_nghi AS MaKhuyenNghi, kn.noi_dung AS NoiDung, kn.thu_tu AS ThuTu,
                   n.ten_nguon AS TenNguon, n.don_vi_cong_bo AS DonViCongBo, n.nam_cong_bo AS NamCongBo
              FROM tuvan.khuyen_nghi kn
              LEFT JOIN tuvan.khuyen_nghi_nguon knn ON knn.ma_khuyen_nghi = kn.ma_khuyen_nghi
              LEFT JOIN tuvan.nguon_tham_khao n ON n.ma_nguon = knn.ma_nguon
             WHERE kn.ma_ke_hoach = @maKeHoach
             ORDER BY kn.thu_tu;", new { maKeHoach });
    }

    /// <summary>Ghi vết mỗi lần chuyên gia xem hồ sơ và thực đơn của khách (nhật ký chỉ thêm, không sửa).</summary>
    public async Task GhiNhanXemAsync(Guid maKeHoach, Guid nguoiXem)
    {
        using var c = await db.OpenAsync();
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
            VALUES (N'ke_hoach_dinh_duong', @maKeHoach, N'XemChiTiet', N'{""muc"":""HoSoVaThucDon""}', @nguoiXem);",
            new { maKeHoach, nguoiXem });
    }
}