using Dapper;
using TuVan.Api.Data;

namespace TuVan.Api.Workers;

/// <summary>
/// Tiến trình nền xử lý hàng đợi tự động (mục 5.2.13 của báo cáo).
/// Worker chỉ đọc, ghi trạng thái việc và nhật ký. Không duyệt kế hoạch.
/// </summary>
public sealed class HangDoiWorker(Db db, ILogger<HangDoiWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var c = await db.OpenAsync(ct);
                var viecs = (await c.QueryAsync<ViecItem>(
                    "tuvan.sp_lay_viec_tu_dong",
                    commandType: System.Data.CommandType.StoredProcedure)).ToList();

                foreach (var v in viecs)
                {
                    // Ở đây chỉ ghi nhận việc đã xử lý. Gửi thông báo thật sẽ thêm sau.
                    log.LogInformation("Xử lý việc {Loai} cho {Doi}", v.LoaiViec, v.MaDoiTuong);
                    await c.ExecuteAsync(
                        "UPDATE tuvan.hang_doi_tu_dong SET trang_thai = N'Xong' WHERE ma_viec = @Ma",
                        new { Ma = v.MaViec });
                    await c.ExecuteAsync(@"
                        INSERT INTO tuvan.nhat_ky_hieu_chinh (ten_bang, ma_ban_ghi, hanh_dong, gia_tri_moi, nguoi_thuc_hien)
                        VALUES (N'hang_doi_tu_dong', @Ma, N'HoanThanhViec', @Json, NULL);",
                        new { Ma = v.MaViec, Json = $"{{\"loai_viec\":\"{v.LoaiViec}\"}}" });
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                log.LogError(ex, "Lỗi khi xử lý hàng đợi");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
        }
    }
}
