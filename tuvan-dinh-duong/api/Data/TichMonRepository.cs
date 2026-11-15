using Dapper;

namespace TuVan.Api.Data;

public sealed record MonTichItem(Guid MaMonAn, string TenMon, string BuaAn, string KhauPhan, bool DaAn);
public sealed record ThucDonNgay(DateTime Ngay, bool CoKeHoach, bool TrongKhoang, int NgayThu, List<MonTichItem> Mon);
public sealed record KeHoachNgay(Guid MaKeHoach, DateTime TuNgay, DateTime DenNgay);
public sealed record MonTrongKhoa(Guid MaMonAn, string KhauPhan);

/// <summary>
/// Nhật ký theo thực đơn: khách tích món đã ăn trong thực đơn của ngày và bữa đã chọn.
/// Món được tích lưu vào nhat_ky_an_uong với khau_phan khác NULL.
/// </summary>
public sealed class TichMonRepository(Db db)
{
    private static readonly string[] BuaHopLe = { "Sang", "Trua", "Toi", "Phu" };

    private static int NgayThuCua(DateTime tuNgay, DateTime ngay)
    {
        var diff = (int)(ngay.Date - tuNgay.Date).TotalDays;
        return ((diff % 7) + 7) % 7 + 1;
    }

    public async Task<ThucDonNgay> ThucDonNgayAsync(Guid nguoi, DateTime ngay)
    {
        using var c = await db.OpenAsync();

        var kh = await c.QuerySingleOrDefaultAsync<KeHoachNgay>(@"
            SELECT ma_ke_hoach AS MaKeHoach, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung';", new { nguoi });

        if (kh is null)
            return new ThucDonNgay(ngay.Date, false, false, 0, new List<MonTichItem>());
        if (ngay.Date < kh.TuNgay.Date || ngay.Date > kh.DenNgay.Date)
            return new ThucDonNgay(ngay.Date, true, false, 0, new List<MonTichItem>());

        var ngayThu = NgayThuCua(kh.TuNgay, ngay);

        var mon = (await c.QueryAsync<MonTichItem>(@"
            SELECT m.ma_mon_an AS MaMonAn, m.ten_mon AS TenMon, k.bua_an AS BuaAn,
                   k.khau_phan AS KhauPhan,
                   CAST(CASE WHEN EXISTS (
                            SELECT 1 FROM tuvan.nhat_ky_an_uong n
                             WHERE n.ma_nguoi_dung = @nguoi AND n.ngay_an = @ngay
                               AND n.bua_an = k.bua_an AND n.ma_mon_an = k.ma_mon_an
                               AND n.khau_phan IS NOT NULL)
                        THEN 1 ELSE 0 END AS BIT) AS DaAn
              FROM tuvan.ke_hoach_thuc_don k
              JOIN tuvan.mon_an m ON m.ma_mon_an = k.ma_mon_an
             WHERE k.ma_ke_hoach = @maKh AND k.ngay_thu = @ngayThu
             ORDER BY CASE k.bua_an WHEN 'Sang' THEN 1 WHEN 'Trua' THEN 2 WHEN 'Toi' THEN 3 ELSE 4 END,
                      m.ten_mon;",
            new { nguoi, ngay = ngay.Date, maKh = kh.MaKeHoach, ngayThu })).ToList();

        return new ThucDonNgay(ngay.Date, true, true, ngayThu, mon);
    }

    /// <summary>Lưu các món được tích trong một bữa. Món không tích sẽ bị gỡ khỏi nhật ký của bữa đó.</summary>
    public async Task LuuTickAsync(Guid nguoi, DateTime ngay, string bua, IReadOnlyList<Guid> maMons)
    {
        if (!BuaHopLe.Contains(bua)) throw new NghiepVuException("Bữa ăn không hợp lệ.");

        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        var kh = await c.QuerySingleOrDefaultAsync<KeHoachNgay>(@"
            SELECT ma_ke_hoach AS MaKeHoach, tu_ngay AS TuNgay, den_ngay AS DenNgay
              FROM tuvan.ke_hoach_dinh_duong WITH (UPDLOCK, ROWLOCK)
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung';", new { nguoi }, tx);

        if (kh is null) throw new NghiepVuException("Bạn chưa có kế hoạch đang áp dụng.");
        if (ngay.Date < kh.TuNgay.Date || ngay.Date > kh.DenNgay.Date)
            throw new NghiepVuException("Ngày này nằm ngoài thời gian kế hoạch.");

        var ngayThu = NgayThuCua(kh.TuNgay, ngay);

        var hopLe = (await c.QueryAsync<MonTrongKhoa>(@"
            SELECT ma_mon_an AS MaMonAn, khau_phan AS KhauPhan
              FROM tuvan.ke_hoach_thuc_don
             WHERE ma_ke_hoach = @maKh AND ngay_thu = @ngayThu AND bua_an = @bua;",
            new { maKh = kh.MaKeHoach, ngayThu, bua }, tx))
            .ToDictionary(x => x.MaMonAn, x => x.KhauPhan);

        var chon = maMons.Distinct().ToList();
        if (chon.Any(m => !hopLe.ContainsKey(m)))
            throw new NghiepVuException("Có món không thuộc thực đơn của bữa này.");

        await c.ExecuteAsync(@"
            DELETE FROM tuvan.nhat_ky_an_uong
             WHERE ma_nguoi_dung = @nguoi AND ngay_an = @ngay AND bua_an = @bua
               AND khau_phan IS NOT NULL;",
            new { nguoi, ngay = ngay.Date, bua }, tx);

        foreach (var m in chon)
        {
            await c.ExecuteAsync(@"
                INSERT INTO tuvan.nhat_ky_an_uong
                    (ma_nhat_ky, ma_nguoi_dung, ma_mon_an, ngay_an, bua_an, khoi_luong, khau_phan)
                VALUES (NEWID(), @nguoi, @maMon, @ngay, @bua, 100, @khauPhan);",
                new { nguoi, maMon = m, ngay = ngay.Date, bua, khauPhan = hopLe[m] }, tx);
        }

        tx.Commit();
    }
}