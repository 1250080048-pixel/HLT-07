using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TuVan.Api.Data;

/// <summary>
/// Toàn bộ truy vấn SQL của nghiệp vụ kế hoạch.
/// Mọi thao tác ghi đều chạy trong giao dịch và kiểm tra quyền ngay trong câu lệnh UPDATE.
/// </summary>
public sealed class KeHoachRepository(Db db)
{
    // ---------- Người dùng ----------
    public async Task<Guid?> MaNguoiDungTuSubAsync(string sub)
    {
        using var c = await db.OpenAsync();
        return await c.QuerySingleOrDefaultAsync<Guid?>(
            "SELECT ma_nguoi_dung FROM tuvan.nguoi_dung WHERE subject_oidc = @sub", new { sub });
    }

    // ---------- Soạn kế hoạch ----------
    public async Task<Guid> TaoNhapAsync(Guid soan, Guid nguoiDung, DateTime tuNgay, DateTime denNgay)
    {
        using var c = await db.OpenAsync();
        var id = Guid.NewGuid();
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.ke_hoach_dinh_duong
                (ma_ke_hoach, ma_nguoi_dung, ma_nguoi_soan, phien_ban, trang_thai, tu_ngay, den_ngay)
            VALUES (@id, @nguoiDung, @soan, 1, N'Nhap', @tuNgay, @denNgay);",
            new { id, nguoiDung, soan, tuNgay = tuNgay.Date, denNgay = denNgay.Date });
        return id;
    }

    public async Task ThemKhuyenNghiAsync(Guid maKeHoach, Guid soan, string noiDung, int thuTu, IReadOnlyList<Guid> maNguons)
    {
        if (maNguons.Count == 0) throw new NghiepVuException("BR04: khuyến nghị phải có ít nhất một nguồn.");
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();
        // Chỉ người soạn của kế hoạch đang ở trạng thái Nháp mới được thêm khuyến nghị
        var ok = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach AND ma_nguoi_soan = @soan AND trang_thai = N'Nhap';",
            new { maKeHoach, soan }, tx);
        if (ok == 0) throw new NghiepVuException("Không thể sửa: kế hoạch không ở trạng thái Nháp hoặc bạn không phải người soạn.");

        var maKn = Guid.NewGuid();
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.khuyen_nghi (ma_khuyen_nghi, ma_ke_hoach, noi_dung, thu_tu)
            VALUES (@maKn, @maKeHoach, @noiDung, @thuTu);", new { maKn, maKeHoach, noiDung, thuTu }, tx);
        foreach (var nguon in maNguons)
        {
            await c.ExecuteAsync(@"
                INSERT INTO tuvan.khuyen_nghi_nguon (ma_khuyen_nghi, ma_nguon) VALUES (@maKn, @nguon);",
                new { maKn, nguon }, tx);
        }
        tx.Commit();
    }

    public async Task GuiDuyetAsync(Guid maKeHoach, Guid soan)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();
        var soKn = await c.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tuvan.khuyen_nghi WHERE ma_ke_hoach = @maKeHoach", new { maKeHoach }, tx);
        if (soKn == 0) throw new NghiepVuException("Kế hoạch chưa có khuyến nghị nào.");

        var thieuNguon = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.khuyen_nghi kn
             WHERE kn.ma_ke_hoach = @maKeHoach
               AND NOT EXISTS (SELECT 1 FROM tuvan.khuyen_nghi_nguon knn WHERE knn.ma_khuyen_nghi = kn.ma_khuyen_nghi);",
            new { maKeHoach }, tx);
        if (thieuNguon > 0) throw new NghiepVuException("BR04: có khuyến nghị chưa gắn nguồn tham khảo.");

        var n = await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = N'ChoDuyet'
             WHERE ma_ke_hoach = @maKeHoach AND ma_nguoi_soan = @soan AND trang_thai = N'Nhap';",
            new { maKeHoach, soan }, tx);
        if (n == 0) throw new NghiepVuException("Không thể gửi duyệt từ trạng thái hiện tại.");
        tx.Commit();
    }

    public async Task<IEnumerable<KeHoachItem>> KeHoachCuaNguoiSoanAsync(Guid soan)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<KeHoachItem>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung, ma_nguoi_soan AS MaNguoiSoan,
                   phien_ban AS PhienBan, trang_thai AS TrangThai, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_nguoi_soan = @soan
             ORDER BY tu_ngay DESC;", new { soan });
    }

    public async Task<IEnumerable<KhuyenNghiItem>> KhuyenNghiCuaAsync(Guid maKeHoach)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<KhuyenNghiItem>(@"
            SELECT ma_khuyen_nghi AS MaKhuyenNghi, noi_dung AS NoiDung, thu_tu AS ThuTu
              FROM tuvan.khuyen_nghi WHERE ma_ke_hoach = @maKeHoach ORDER BY thu_tu;", new { maKeHoach });
    }

    // ---------- Duyệt ----------
    public async Task<IEnumerable<KeHoachItem>> ChoDuyetAsync(Guid nguoiDuyet)
    {
        using var c = await db.OpenAsync();
        // BR02: không hiển thị kế hoạch do chính người xem soạn
        return await c.QueryAsync<KeHoachItem>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung, ma_nguoi_soan AS MaNguoiSoan,
                   phien_ban AS PhienBan, trang_thai AS TrangThai, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong
             WHERE trang_thai = N'ChoDuyet' AND ma_nguoi_soan <> @nguoiDuyet
             ORDER BY tu_ngay;", new { nguoiDuyet });
    }

    /// <summary>Duyệt hoặc trả lại. Ghi phiếu duyệt và nhật ký hiệu chỉnh trong một giao dịch.</summary>
    public async Task RaQuyetDinhAsync(Guid maKeHoach, Guid nguoiDuyet, bool duyet, string? lyDo)
    {
        if (!duyet && string.IsNullOrWhiteSpace(lyDo)) throw new NghiepVuException("Trả lại phải có lý do.");
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var kh = await c.QuerySingleOrDefaultAsync<KeHoachItem>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung, ma_nguoi_soan AS MaNguoiSoan,
                   phien_ban AS PhienBan, trang_thai AS TrangThai, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach;", new { maKeHoach }, tx);
        if (kh is null) throw new NghiepVuException("Không tìm thấy kế hoạch.");
        if (kh.MaNguoiSoan == nguoiDuyet) throw new NghiepVuException("BR02: người duyệt không được trùng người soạn.");
        if (kh.TrangThai != "ChoDuyet") throw new NghiepVuException("Kế hoạch không ở trạng thái chờ duyệt.");

        var ketQua = duyet ? "Duyet" : "TraLai";
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.phieu_duyet (ma_phieu, ma_ke_hoach, ma_nguoi_duyet, ket_qua, ly_do)
            VALUES (NEWID(), @maKeHoach, @nguoiDuyet, @ketQua, @lyDo);",
            new { maKeHoach, nguoiDuyet, ketQua, lyDo }, tx);

        await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = @trangThai WHERE ma_ke_hoach = @maKeHoach;",
            new { trangThai = duyet ? "DaDuyet" : "BiTraLai", maKeHoach }, tx);

        await c.ExecuteAsync(@"
            INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
            VALUES (N'ke_hoach_dinh_duong', @maKeHoach, @hanhDong, @json, @nguoiDuyet);",
            new
            {
                maKeHoach, nguoiDuyet,
                hanhDong = ketQua,
                json = $"{{\"ket_qua\":\"{ketQua}\"}}"
            }, tx);
        tx.Commit();
    }

    // ---------- Áp dụng ----------
    /// <summary>Áp dụng kế hoạch đã duyệt. Nếu có kế hoạch đang áp dụng, kết thúc nó trước (tránh chồng lấn).</summary>
    public async Task ApDungAsync(Guid maKeHoach, Guid soan)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();
        var kh = await c.QuerySingleOrDefaultAsync<KeHoachItem>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung, ma_nguoi_soan AS MaNguoiSoan,
                   phien_ban AS PhienBan, trang_thai AS TrangThai, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach AND ma_nguoi_soan = @soan;", new { maKeHoach, soan }, tx);
        if (kh is null || kh.TrangThai != "DaDuyet")
            throw new NghiepVuException("Chỉ kế hoạch đã duyệt của bạn mới được áp dụng.");

        await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = N'KetThuc'
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung'
               AND tu_ngay <= @denNgay AND den_ngay >= @tuNgay;",
            new { nguoi = kh.MaNguoiDung, tuNgay = kh.TuNgay, denNgay = kh.DenNgay }, tx);

        await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = N'DangApDung' WHERE ma_ke_hoach = @maKeHoach;",
            new { maKeHoach }, tx);
        tx.Commit();
    }

    // ---------- Người dùng: nhật ký và tiến độ ----------
    public async Task GhiNhatKyAsync(Guid nguoiDung, Guid maMonAn, DateTime ngayAn, string buaAn, decimal khoiLuong)
    {
        using var c = await db.OpenAsync();
        var n = await c.ExecuteAsync(@"
            INSERT INTO tuvan.nhat_ky_an_uong (ma_nhat_ky, ma_nguoi_dung, ma_mon_an, ngay_an, bua_an, khoi_luong)
            SELECT NEWID(), @nguoiDung, m.ma_mon_an, @ngayAn, @buaAn, @khoiLuong
              FROM tuvan.mon_an m WHERE m.ma_mon_an = @maMonAn;",
            new { nguoiDung, maMonAn, ngayAn = ngayAn.Date, buaAn, khoiLuong });
        if (n == 0) throw new NghiepVuException("Món không có trong danh mục. Hãy gửi yêu cầu bổ sung món.");
    }

    public async Task<IEnumerable<TienDoItem>> TienDoAsync(Guid nguoiDung)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<TienDoItem>(@"
            SELECT ma_ke_hoach AS MaKeHoach, tong_ngay AS TongNgay,
                   so_ngay_da_ghi AS SoNgayDaGhi, ty_le_phan_tram AS TyLePhanTram
              FROM tuvan.vw_tien_do WHERE ma_nguoi_dung = @nguoiDung;", new { nguoiDung });
    }

    public async Task<IEnumerable<NguonItem>> NguonDangHoatDongAsync()
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<NguonItem>(@"
            SELECT ma_nguon AS MaNguon, ten_nguon AS TenNguon, don_vi_cong_bo AS DonViCongBo, nam_cong_bo AS NamCongBo
              FROM tuvan.nguon_tham_khao WHERE dang_hoat_dong = 1 ORDER BY nam_cong_bo DESC;");
    }

    public async Task<IEnumerable<MonAnItem>> DanhSachMonAsync()
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MonAnItem>(@"
            SELECT ma_mon_an AS MaMonAn, ten_mon AS TenMon, nang_luong AS NangLuong FROM tuvan.mon_an ORDER BY ten_mon;");
    }
}
