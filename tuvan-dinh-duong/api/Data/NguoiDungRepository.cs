using Dapper;

namespace TuVan.Api.Data;

public sealed record HoSoItem(decimal? ChieuCao, decimal? CanNang, string? DiUng, bool ThuocNhomRuiRoCao, string? DiUngNhom);
public sealed record HoSoReq(string HoTen, decimal? ChieuCao, decimal? CanNang, string? DiUng,
    bool ThuocNhomRuiRoCao, List<string>? DiUngNhom = null);
public sealed record DongBoResult(Guid MaNguoiDung, bool CoHoSo);

public sealed class NguoiDungRepository(Db db)
{
    /// <summary>Mã nhóm dị ứng hợp lệ. Mỗi món có thể mang nhiều nhóm, lưu dạng "HaiSan,Trung".</summary>
    public static readonly string[] NhomDiUngHopLe = { "HaiSan", "Trung", "Sua", "DauNanh", "Gluten", "DauPhong" };

    public async Task<DongBoResult> DongBoAsync(string sub, string hoTen)
    {
        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        await c.ExecuteAsync(@"
            MERGE tuvan.nguoi_dung AS t
            USING (SELECT @sub AS subject_oidc) AS s
               ON t.subject_oidc = s.subject_oidc
            WHEN NOT MATCHED THEN
                INSERT (ma_nguoi_dung, subject_oidc, ho_ten, vai_tro)
                VALUES (NEWID(), @sub, @hoTen, N'NguoiDung');", new { sub, hoTen }, tx);

        var id = await c.ExecuteScalarAsync<Guid>(
            "SELECT ma_nguoi_dung FROM tuvan.nguoi_dung WHERE subject_oidc = @sub", new { sub }, tx);
        var coHoSo = await c.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tuvan.ho_so_dinh_duong WHERE ma_nguoi_dung = @id", new { id }, tx);

        tx.Commit();
        return new DongBoResult(id, coHoSo > 0);
    }

    public async Task<HoSoItem?> LayHoSoAsync(Guid nguoiDung)
    {
        using var c = await db.OpenAsync();
        return await c.QuerySingleOrDefaultAsync<HoSoItem>(@"
            SELECT chieu_cao AS ChieuCao, can_nang AS CanNang, di_ung AS DiUng,
                   thuoc_nhom_rui_ro_cao AS ThuocNhomRuiRoCao, di_ung_nhom AS DiUngNhom
              FROM tuvan.ho_so_dinh_duong WHERE ma_nguoi_dung = @nguoiDung;", new { nguoiDung });
    }

    public async Task LuuHoSoAsync(Guid nguoiDung, HoSoReq r)
    {
        if (string.IsNullOrWhiteSpace(r.HoTen)) throw new NghiepVuException("Vui lòng nhập họ tên.");
        if (r.ChieuCao is not null && (r.ChieuCao < 50 || r.ChieuCao > 250))
            throw new NghiepVuException("Chiều cao không hợp lệ (50 - 250 cm).");
        if (r.CanNang is not null && (r.CanNang < 10 || r.CanNang > 300))
            throw new NghiepVuException("Cân nặng không hợp lệ (10 - 300 kg).");

        var nhom = (r.DiUngNhom ?? new List<string>())
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct()
            .ToList();
        var sai = nhom.Where(x => !NhomDiUngHopLe.Contains(x)).ToList();
        if (sai.Count > 0) throw new NghiepVuException("Nhóm dị ứng không hợp lệ: " + string.Join(", ", sai));
        var diUngNhom = nhom.Count == 0 ? null : string.Join(",", nhom);

        using var c = await db.OpenAsync();
        using var tx = c.BeginTransaction();

        await c.ExecuteAsync(@"
            MERGE tuvan.ho_so_dinh_duong AS t
            USING (SELECT @nguoiDung AS ma_nguoi_dung) AS s
               ON t.ma_nguoi_dung = s.ma_nguoi_dung
            WHEN MATCHED THEN
                UPDATE SET chieu_cao = @chieuCao, can_nang = @canNang,
                           di_ung = @diUng, thuoc_nhom_rui_ro_cao = @ruiRo, di_ung_nhom = @diUngNhom
            WHEN NOT MATCHED THEN
                INSERT (ma_ho_so, ma_nguoi_dung, chieu_cao, can_nang, di_ung, thuoc_nhom_rui_ro_cao, di_ung_nhom)
                VALUES (NEWID(), @nguoiDung, @chieuCao, @canNang, @diUng, @ruiRo, @diUngNhom);",
            new
            {
                nguoiDung,
                chieuCao = r.ChieuCao,
                canNang = r.CanNang,
                diUng = r.DiUng,
                ruiRo = r.ThuocNhomRuiRoCao,
                diUngNhom
            }, tx);

        await c.ExecuteAsync(
            "UPDATE tuvan.nguoi_dung SET ho_ten = @hoTen WHERE ma_nguoi_dung = @nguoiDung;",
            new { hoTen = r.HoTen.Trim(), nguoiDung }, tx);

        tx.Commit();
    }
}