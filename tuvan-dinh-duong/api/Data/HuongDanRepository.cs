using Dapper;

namespace TuVan.Api.Data;

public sealed record YeuCauItem(Guid MaYeuCau, Guid MaNguoiDung, string HoTen, string Loai, string? GhiChu, DateTime ThoiDiem);
public sealed record YeuCauCuaToi(Guid MaYeuCau, string Loai, string TrangThai, string? GhiChu, DateTime ThoiDiem);

public sealed class HuongDanRepository(Db db)
{
    /// <summary>Khách gửi yêu cầu để chuyên gia soạn một kế hoạch riêng cho mình.</summary>
    public async Task<Guid> TaoYeuCauKeHoachRiengAsync(Guid nguoi, string ghiChu)
    {
        var noiDung = (ghiChu ?? string.Empty).Trim();
        if (noiDung.Length < 10) throw new NghiepVuException("Vui lòng mô tả mục tiêu của bạn (ít nhất 10 ký tự).");
        if (noiDung.Length > 500) throw new NghiepVuException("Mô tả tối đa 500 ký tự.");

        using var c = await db.OpenAsync();
        var dangCho = await c.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) FROM tuvan.yeu_cau_tu_van
             WHERE ma_nguoi_dung = @nguoi AND trang_thai = N'ChoChuyenGia';", new { nguoi });
        if (dangCho > 0) throw new NghiepVuException("Bạn đã có yêu cầu đang chờ chuyên gia.");

        var id = Guid.NewGuid();
        await c.ExecuteAsync(@"
            INSERT INTO tuvan.yeu_cau_tu_van (ma_yeu_cau, ma_nguoi_dung, loai, ghi_chu)
            VALUES (@id, @nguoi, N'KeHoachRieng', @noiDung);", new { id, nguoi, noiDung });
        return id;
    }

    public async Task<IEnumerable<YeuCauCuaToi>> YeuCauCuaToiAsync(Guid nguoi)
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<YeuCauCuaToi>(@"
            SELECT ma_yeu_cau AS MaYeuCau, loai AS Loai, trang_thai AS TrangThai,
                   ghi_chu AS GhiChu, thoi_diem AS ThoiDiem
              FROM tuvan.yeu_cau_tu_van
             WHERE ma_nguoi_dung = @nguoi
             ORDER BY thoi_diem DESC;", new { nguoi });
    }

    public async Task<IEnumerable<YeuCauItem>> ChoChuyenGiaAsync()
    {
        using var c = await db.OpenAsync();
        return await c.QueryAsync<YeuCauItem>(@"
            SELECT y.ma_yeu_cau AS MaYeuCau, y.ma_nguoi_dung AS MaNguoiDung, n.ho_ten AS HoTen,
                   y.loai AS Loai, y.ghi_chu AS GhiChu, y.thoi_diem AS ThoiDiem
              FROM tuvan.yeu_cau_tu_van y
              JOIN tuvan.nguoi_dung n ON n.ma_nguoi_dung = y.ma_nguoi_dung
             WHERE y.trang_thai = N'ChoChuyenGia'
             ORDER BY y.thoi_diem;");
    }

    public async Task DaXuLyAsync(Guid maYeuCau)
    {
        using var c = await db.OpenAsync();
        var n = await c.ExecuteAsync(@"
            UPDATE tuvan.yeu_cau_tu_van SET trang_thai = N'DaXuLy'
             WHERE ma_yeu_cau = @maYeuCau AND trang_thai = N'ChoChuyenGia';", new { maYeuCau });
        if (n == 0) throw new NghiepVuException("Yêu cầu không tồn tại hoặc đã được xử lý.");
    }
}