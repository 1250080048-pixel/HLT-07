using Dapper;

namespace TuVan.Api.Data;

public sealed record CaloNgayRow(DateTime Ngay, decimal TongKcal);
public sealed record ThongBaoCalo(string Loai, string NoiDung);
public sealed record ThamChieuCalo(int KcalMin, int KcalMax, string? Nguon);
public sealed record CaloNgay(DateTime Ngay, decimal TongKcal, string? TrangThai, string? ThongBao);
public sealed record CaloKetQua(bool AnSo, string? LyDoAnSo, string? MaMucTieu, ThamChieuCalo? ThamChieu, List<CaloNgay> Ngay);

/// <summary>
/// Tổng calo nạp vào theo ngày. Hệ thống không tự tính mục tiêu kcal.
/// Mốc tham chiếu và thông báo đều do chuyên gia duyệt. Nhóm rủi ro cao bị ẩn số liệu.
/// </summary>
public sealed class CaloRepository(Db db)
{
    public async Task<CaloKetQua> LayAsync(Guid nguoi, DateTime tuNgay, DateTime denNgay)
    {
        using var c = await db.OpenAsync();

        var ruiRo = await c.QuerySingleOrDefaultAsync<bool?>(
            "SELECT thuoc_nhom_rui_ro_cao FROM tuvan.ho_so_dinh_duong WHERE ma_nguoi_dung = @nguoi;", new { nguoi });
        if (ruiRo == true)
            return new CaloKetQua(true,
                "Hồ sơ của bạn thuộc nhóm cần tư vấn riêng nên không hiển thị số liệu calo.",
                null, null, new List<CaloNgay>());

        var maMuc = await c.QuerySingleOrDefaultAsync<string?>(@"
            SELECT ma_muc_tieu FROM tuvan.ke_hoach_dinh_duong
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'DangApDung';", new { nguoi });

        ThamChieuCalo? tc = null;
        if (maMuc is not null)
            tc = await c.QuerySingleOrDefaultAsync<ThamChieuCalo>(@"
                SELECT kcal_min AS KcalMin, kcal_max AS KcalMax, nguon AS Nguon
                  FROM tuvan.calo_tham_chieu WHERE ma_muc_tieu = @maMuc;", new { maMuc });

        var dong = (await c.QueryAsync<CaloNgayRow>(@"
            SELECT ngay_an AS Ngay, tong_kcal AS TongKcal
              FROM tuvan.vw_calo_theo_ngay
             WHERE ma_nguoi_dung = @nguoi AND ngay_an BETWEEN @tuNgay AND @denNgay
             ORDER BY ngay_an;", new { nguoi, tuNgay = tuNgay.Date, denNgay = denNgay.Date })).ToList();

        var thongBao = (await c.QueryAsync<ThongBaoCalo>(
            "SELECT loai AS Loai, noi_dung AS NoiDung FROM tuvan.thong_bao_calo;")).ToList();
        var tbThap = thongBao.FirstOrDefault(x => x.Loai == "thap")?.NoiDung;
        var tbCao = thongBao.FirstOrDefault(x => x.Loai == "cao")?.NoiDung;

        var ngay = dong.Select(r =>
        {
            if (tc is null) return new CaloNgay(r.Ngay, r.TongKcal, null, null);
            if (r.TongKcal < tc.KcalMin) return new CaloNgay(r.Ngay, r.TongKcal, "thap", tbThap);
            if (r.TongKcal > tc.KcalMax) return new CaloNgay(r.Ngay, r.TongKcal, "cao", tbCao);
            return new CaloNgay(r.Ngay, r.TongKcal, "trong", null);
        }).ToList();

        return new CaloKetQua(false, null, maMuc, tc, ngay);
    }
}