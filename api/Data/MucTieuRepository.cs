using System.Data;
using Dapper;

namespace TuVan.Api.Data;

public sealed record MucTieuItem(string MaMucTieu, string TenHienThi, string? MoTa);
public sealed record MonChiTiet(Guid MaMonAn, string TenMon, string NhomThucPham, decimal? NangLuong);
public sealed record MonTrongThucDon(int NgayThu, string BuaAn, Guid MaMonAn, string TenMon, string KhauPhan, string NhomThucPham, decimal? NangLuong);
public sealed record MonTrongKeHoach(int NgayThu, string BuaAn, Guid MaMonAn, string TenMon, string KhauPhan, string NhomThucPham, decimal? NangLuong, string? TenMonGoc);
public sealed record KeHoachHienTai(Guid MaKeHoach, DateTime TuNgay, DateTime DenNgay, string TrangThai);
public sealed record KeHoachMucTieu(Guid MaKeHoach, Guid MaNguoiDung, DateTime TuNgay, DateTime DenNgay, string TrangThai, string MaMucTieu);
public sealed record ChonKetQua(string Loai, Guid? MaKeHoach, Guid? MaYeuCau, string ThongBao);

public sealed record HoSoRuiRo(bool ThuocNhomRuiRoCao, string? DiUngNhom);
public sealed record MonGocRow(int NgayThu, string BuaAn, Guid MaMonAn, string TenMon, string KhauPhan, string? DauHieuDiUng);
public sealed record MonThayRow(Guid MaMonGoc, Guid MaMonThay, string NhomDiUng, string KhauPhan, string? DauHieuDiUng);

public sealed class MucTieuRepository(Db db)
{
    /// <summary>Tài khoản "hệ thống" đứng tên bản kế hoạch tự áp dụng từ thực đơn mẫu đã duyệt.</summary>
    public static readonly Guid HeThongId = Guid.Parse("11111111-0000-0000-0000-000000000099");

    // ---------- Danh mục ----------
    public async Task<IEnumerable<MucTieuItem>> DanhSachMucTieuAsync()
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MucTieuItem>(@"
            SELECT ma_muc_tieu AS MaMucTieu, ten_hien_thi AS TenHienThi, mo_ta AS MoTa
              FROM tuvan.muc_tieu WHERE dang_hoat_dong = 1 ORDER BY ten_hien_thi;");
    }

    public async Task<IEnumerable<MonChiTiet>> DanhSachMonAsync()
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MonChiTiet>(@"
            SELECT ma_mon_an AS MaMonAn, ten_mon AS TenMon,
                   ISNULL(nhom_thuc_pham, N'') AS NhomThucPham, nang_luong AS NangLuong
              FROM tuvan.mon_an ORDER BY ten_mon;");
    }

    /// <summary>Thực đơn mẫu của mục tiêu, gồm đủ 7 ngày.</summary>
    public async Task<IEnumerable<MonTrongThucDon>> ThucDonMauAsync(string maMucTieu)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MonTrongThucDon>(@"
            SELECT t.ngay_thu AS NgayThu, t.bua_an AS BuaAn, m.ma_mon_an AS MaMonAn, m.ten_mon AS TenMon,
                   t.khau_phan AS KhauPhan, ISNULL(m.nhom_thuc_pham, N'') AS NhomThucPham,
                   m.nang_luong AS NangLuong
              FROM tuvan.thuc_don_mau t
              JOIN tuvan.mon_an m ON m.ma_mon_an = t.ma_mon_an
             WHERE t.ma_muc_tieu = @maMucTieu
             ORDER BY t.ngay_thu,
                      CASE t.bua_an WHEN 'Sang' THEN 1 WHEN 'Trua' THEN 2 WHEN 'Toi' THEN 3 ELSE 4 END,
                      m.ten_mon;", new { maMucTieu });
    }

    // ---------- Khách chọn mục tiêu ----------
    /// <summary>
    /// 1. Nhóm rủi ro cao: gửi yêu cầu kế hoạch riêng.
    /// 2. Có dị ứng: thay từng món dị ứng bằng món thay đã duyệt (cùng ngày, cùng bữa).
    ///    Không tìm được món thay an toàn: gửi yêu cầu kế hoạch riêng.
    /// 3. Không có vấn đề: sao chép nguyên thực đơn 7 ngày đã duyệt.
    /// </summary>
    public async Task<ChonKetQua> ChonMucTieuAsync(Guid nguoi, string maMucTieu)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var tenMuc = await c.ExecuteScalarAsync<string?>(
            "SELECT ten_hien_thi FROM tuvan.muc_tieu WHERE ma_muc_tieu = @maMucTieu AND dang_hoat_dong = 1;",
            new { maMucTieu }, tx) ?? throw new NghiepVuException("Mục tiêu không tồn tại.");

        var dangCo = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, HOLDLOCK)
             WHERE ma_nguoi_dung = @nguoi AND trang_thai IN (N'Nhap', N'ChoDuyet', N'DangApDung');",
            new { nguoi }, tx);
        if (dangCo > 0)
            throw new NghiepVuException("Bạn đang có kế hoạch chưa kết thúc. Hãy chờ kế hoạch đó hoàn tất.");

        var hs = await c.QuerySingleOrDefaultAsync<HoSoRuiRo>(@"
            SELECT thuoc_nhom_rui_ro_cao AS ThuocNhomRuiRoCao, di_ung_nhom AS DiUngNhom
              FROM tuvan.ho_so_dinh_duong WHERE ma_nguoi_dung = @nguoi;", new { nguoi }, tx)
            ?? throw new NghiepVuException("Vui lòng khai báo hồ sơ trước khi chọn mục tiêu.");

        if (hs.ThuocNhomRuiRoCao)
        {
            var kqRieng = await TaoYeuCauRiengAsync(c, tx, nguoi,
                $"Khách chọn mục tiêu \"{tenMuc}\" và khai báo nhóm cần tư vấn riêng; cần kế hoạch riêng.",
                "Hồ sơ của bạn thuộc nhóm cần tư vấn riêng. Yêu cầu kế hoạch riêng đã được gửi tới chuyên gia.");
            tx.Commit();
            return kqRieng;
        }

        var diUng = TachNhom(hs.DiUngNhom);

        var monGoc = (await c.QueryAsync<MonGocRow>(@"
            SELECT t.ngay_thu AS NgayThu, t.bua_an AS BuaAn, m.ma_mon_an AS MaMonAn, m.ten_mon AS TenMon,
                   t.khau_phan AS KhauPhan, m.dau_hieu_di_ung AS DauHieuDiUng
              FROM tuvan.thuc_don_mau t
              JOIN tuvan.mon_an m ON m.ma_mon_an = t.ma_mon_an
             WHERE t.ma_muc_tieu = @maMucTieu
             ORDER BY t.ngay_thu;", new { maMucTieu }, tx)).ToList();
        if (monGoc.Count == 0) throw new NghiepVuException("Mục tiêu này chưa có thực đơn đã duyệt.");

        var thay = (await c.QueryAsync<MonThayRow>(@"
            SELECT r.ma_mon_goc AS MaMonGoc, r.ma_mon_thay AS MaMonThay, r.nhom_di_ung AS NhomDiUng,
                   r.khau_phan AS KhauPhan, m.dau_hieu_di_ung AS DauHieuDiUng
              FROM tuvan.mon_thay_the r
              JOIN tuvan.mon_an m ON m.ma_mon_an = r.ma_mon_thay
             WHERE r.ma_mon_goc IN (SELECT ma_mon_an FROM tuvan.thuc_don_mau WHERE ma_muc_tieu = @maMucTieu);",
            new { maMucTieu }, tx)).ToList();

        var dong = new List<(int Ngay, string Bua, Guid MaMon, string KhauPhan, Guid? MaGoc)>();
        var thayDoi = new List<(int Ngay, Guid Goc, Guid Moi, string Nhom)>();
        var khongThay = new List<string>();

        foreach (var g in monGoc)
        {
            var kich = TachNhom(g.DauHieuDiUng).Where(diUng.Contains).ToList();
            if (kich.Count == 0)
            {
                dong.Add((g.NgayThu, g.BuaAn, g.MaMonAn, g.KhauPhan, null));
                continue;
            }

            var chon = thay.Where(x => x.MaMonGoc == g.MaMonAn)
                           .GroupBy(x => x.MaMonThay)
                           .Select(nhom => nhom.ToList())
                           .FirstOrDefault(rows =>
                               !TachNhom(rows[0].DauHieuDiUng).Any(diUng.Contains) &&
                               kich.All(t => rows.Any(r => r.NhomDiUng == t)));

            if (chon is null)
            {
                khongThay.Add($"ngày {g.NgayThu}: {g.TenMon}");
                continue;
            }

            dong.Add((g.NgayThu, g.BuaAn, chon[0].MaMonThay, chon[0].KhauPhan, g.MaMonAn));
            foreach (var t in kich) thayDoi.Add((g.NgayThu, g.MaMonAn, chon[0].MaMonThay, t));
        }

        if (khongThay.Count > 0)
        {
            var ghi = "Khách có khai báo dị ứng; không có món thay an toàn cho: " + string.Join("; ", khongThay);
            if (ghi.Length > 500) ghi = ghi[..500];
            var kqRieng = await TaoYeuCauRiengAsync(c, tx, nguoi, ghi,
                "Một số món trong mục tiêu không có món thay an toàn với khai báo của bạn. Yêu cầu kế hoạch riêng đã được gửi tới chuyên gia.");
            tx.Commit();
            return kqRieng;
        }

        var id = Guid.NewGuid();
        var tuNgay = DateTime.Today;
        var denNgay = tuNgay.AddDays(6);

        await c.ExecuteAsync(@"
            INSERT INTO tuvan.ke_hoach_dinh_duong
                (ma_ke_hoach, ma_nguoi_dung, ma_nguoi_soan, phien_ban, trang_thai, tu_ngay, den_ngay, ma_muc_tieu)
            VALUES (@id, @nguoi, @heThong, 1, N'DangApDung', @tuNgay, @denNgay, @maMucTieu);",
            new { id, nguoi, heThong = HeThongId, tuNgay = tuNgay.Date, denNgay = denNgay.Date, maMucTieu }, tx);

        var daCo = new HashSet<(int, string, Guid)>();
        foreach (var d in dong)
        {
            if (!daCo.Add((d.Ngay, d.Bua, d.MaMon))) continue;
            await c.ExecuteAsync(@"
                INSERT INTO tuvan.ke_hoach_thuc_don (ma_ke_hoach, ngay_thu, bua_an, ma_mon_an, khau_phan, ma_mon_goc)
                VALUES (@id, @ngay, @bua, @maMon, @khauPhan, @maGoc);",
                new { id, ngay = d.Ngay, bua = d.Bua, maMon = d.MaMon, khauPhan = d.KhauPhan, maGoc = d.MaGoc }, tx);
        }

        // BR09: ghi vết việc áp dụng tự động và từng lần thay món
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
            VALUES (N'ke_hoach_dinh_duong', @id, N'ApDungTuMauDaDuyet', @json, NULL);",
            new { id, json = $"{{\"ma_muc_tieu\":\"{maMucTieu}\",\"nguon\":\"thuc_don_mau_da_duyet\"}}" }, tx);

        foreach (var t in thayDoi)
        {
            await c.ExecuteAsync(@"
                INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
                VALUES (N'ke_hoach_dinh_duong', @id, N'ThayMonDiUng', @json, NULL);",
                new { id, json = $"{{\"ngay\":{t.Ngay},\"mon_goc\":\"{t.Goc}\",\"mon_thay\":\"{t.Moi}\",\"nhom\":\"{t.Nhom}\"}}" }, tx);
        }

        tx.Commit();

        var soMonThay = dong.Count(d => d.MaGoc is not null);
        var thongBao = soMonThay == 0
            ? "Đã áp dụng kế hoạch 7 ngày theo mục tiêu. Hãy ghi nhật ký mỗi ngày để theo dõi tiến độ."
            : $"Đã áp dụng kế hoạch 7 ngày. {soMonThay} món đã được thay bằng món an toàn đã duyệt theo khai báo dị ứng của bạn.";
        return new ChonKetQua("ApDung", id, null, thongBao);
    }

    private static async Task<ChonKetQua> TaoYeuCauRiengAsync(IDbConnection c, IDbTransaction tx, Guid nguoi, string ghiChu, string thongBao)
    {
        var dangCho = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.yeu_cau_tu_van
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'ChoChuyenGia';", new { nguoi }, tx);
        if (dangCho > 0) throw new NghiepVuException("Bạn đã có yêu cầu đang chờ chuyên gia.");

        var yeuCau = Guid.NewGuid();
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.yeu_cau_tu_van (ma_yeu_cau, ma_nguoi_dung, loai, ghi_chu)
            VALUES (@yeuCau, @nguoi, N'KeHoachRieng', @ghiChu);",
            new { yeuCau, nguoi, ghiChu }, tx);
        return new ChonKetQua("YeuCauRieng", null, yeuCau, thongBao);
    }

    private static HashSet<string> TachNhom(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? new HashSet<string>()
            : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

    public async Task<KeHoachMucTieu?> KeHoachMucTieuGanNhatAsync(Guid nguoi)
    {
        using var c = await db.OpenAsync();
        return await c.QueryFirstOrDefaultAsync<KeHoachMucTieu>(@"
            SELECT TOP 1 ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung,
                   tu_ngay AS TuNgay, den_ngay AS DenNgay, trang_thai AS TrangThai,
                   ma_muc_tieu AS MaMucTieu
              FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_nguoi_dung = @nguoi AND ma_muc_tieu IS NOT NULL
             ORDER BY CASE trang_thai WHEN N'DangApDung' THEN 1 WHEN N'ChoDuyet' THEN 2
                                      WHEN N'DaDuyet' THEN 3 WHEN N'Nhap' THEN 4 ELSE 5 END,
                      tu_ngay DESC;", new { nguoi });
    }

    /// <summary>Kế hoạch theo mục tiêu do chuyên gia duyệt (luồng cũ, vẫn giữ để tương thích).</summary>
    public async Task<bool> KichHoatMauDaDuyetAsync(Guid maKeHoach)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var kh = await c.QuerySingleOrDefaultAsync<KeHoachMucTieu>(@"
            SELECT ma_ke_hoach AS MaKeHoach, ma_nguoi_dung AS MaNguoiDung,
                   tu_ngay AS TuNgay, den_ngay AS DenNgay, trang_thai AS TrangThai,
                   ma_muc_tieu AS MaMucTieu
              FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach AND ma_muc_tieu IS NOT NULL AND trang_thai = N'DaDuyet';",
            new { maKeHoach }, tx);
        if (kh is null) { tx.Commit(); return false; }

        await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = N'KetThuc'
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung'
               AND tu_ngay <= @denNgay AND den_ngay >= @tuNgay;",
            new { nguoi = kh.MaNguoiDung, tuNgay = kh.TuNgay, denNgay = kh.DenNgay }, tx);

        await c.ExecuteAsync(@"
            UPDATE tuvan.ke_hoach_dinh_duong SET trang_thai = N'DangApDung' WHERE ma_ke_hoach = @maKeHoach;",
            new { maKeHoach }, tx);

        tx.Commit();
        return true;
    }

    // ---------- Kế hoạch đang áp dụng (khách) ----------
    public async Task<KeHoachHienTai?> KeHoachDangApDungAsync(Guid nguoi)
    {
        using var c = await db.OpenAsync();
        return await c.QuerySingleOrDefaultAsync<KeHoachHienTai>(@"
            SELECT ma_ke_hoach AS MaKeHoach, tu_ngay AS TuNgay, den_ngay AS DenNgay, trang_thai AS TrangThai
              FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung';", new { nguoi });
    }

    /// <summary>Thực đơn 7 ngày của kế hoạch, kèm tên món gốc nếu món đã được thay vì dị ứng.</summary>
    public async Task<IEnumerable<MonTrongKeHoach>> ThucDonCuaKeHoachAsync(Guid maKeHoach, Guid nguoi)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<MonTrongKeHoach>(@"
            SELECT k.ngay_thu AS NgayThu, k.bua_an AS BuaAn, m.ma_mon_an AS MaMonAn, m.ten_mon AS TenMon,
                   k.khau_phan AS KhauPhan, ISNULL(m.nhom_thuc_pham, N'') AS NhomThucPham,
                   m.nang_luong AS NangLuong, goc.ten_mon AS TenMonGoc
              FROM tuvan.ke_hoach_thuc_don k
              JOIN tuvan.mon_an m ON m.ma_mon_an = k.ma_mon_an
              JOIN tuvan.ke_hoach_dinh_duong h ON h.ma_ke_hoach = k.ma_ke_hoach
              LEFT JOIN tuvan.mon_an goc ON goc.ma_mon_an = k.ma_mon_goc
             WHERE k.ma_ke_hoach = @maKeHoach
               AND (h.ma_nguoi_dung = @nguoi OR h.ma_nguoi_soan = @nguoi)
             ORDER BY k.ngay_thu,
                      CASE k.bua_an WHEN 'Sang' THEN 1 WHEN 'Trua' THEN 2 WHEN 'Toi' THEN 3 ELSE 4 END,
                      m.ten_mon;", new { maKeHoach, nguoi });
    }

    // ---------- Chuyên gia soạn: chỉnh món theo ngày trong kế hoạch nháp ----------
    public async Task ThemMonAsync(Guid maKeHoach, Guid soan, int ngayThu, string bua, Guid maMon, string khauPhan)
    {
        if (ngayThu < 1 || ngayThu > 7) throw new NghiepVuException("Ngày thứ phải từ 1 đến 7.");
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var dungNguoiSoan = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach AND ma_nguoi_soan = @soan AND trang_thai = N'Nhap';",
            new { maKeHoach, soan }, tx);
        if (dungNguoiSoan == 0)
            throw new NghiepVuException("Chỉ người soạn của kế hoạch đang Nháp mới được thêm món.");

        var daCo = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.ke_hoach_thuc_don
             WHERE ma_ke_hoach = @maKeHoach AND ngay_thu = @ngayThu AND bua_an = @bua AND ma_mon_an = @maMon;",
            new { maKeHoach, ngayThu, bua, maMon }, tx);
        if (daCo > 0) throw new NghiepVuException("Món này đã có trong bữa của ngày đó.");

        await c.ExecuteAsync(@"
            INSERT INTO tuvan.ke_hoach_thuc_don (ma_ke_hoach, ngay_thu, bua_an, ma_mon_an, khau_phan)
            VALUES (@maKeHoach, @ngayThu, @bua, @maMon, @khauPhan);",
            new { maKeHoach, ngayThu, bua, maMon, khauPhan }, tx);
        tx.Commit();
    }

    public async Task XoaMonAsync(Guid maKeHoach, Guid soan, int ngayThu, string bua, Guid maMon)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var dungNguoiSoan = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_ke_hoach = @maKeHoach AND ma_nguoi_soan = @soan AND trang_thai = N'Nhap';",
            new { maKeHoach, soan }, tx);
        if (dungNguoiSoan == 0)
            throw new NghiepVuException("Chỉ người soạn của kế hoạch đang Nháp mới được xóa món.");

        var n = await c.ExecuteAsync(@"
            DELETE FROM tuvan.ke_hoach_thuc_don
             WHERE ma_ke_hoach = @maKeHoach AND ngay_thu = @ngayThu AND bua_an = @bua AND ma_mon_an = @maMon;",
            new { maKeHoach, ngayThu, bua, maMon }, tx);
        if (n == 0) throw new NghiepVuException("Không tìm thấy món cần xóa.");
        tx.Commit();
    }
}