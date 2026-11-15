using System.Security.Claims;
using Dapper;
using TuVan.Api.Data;
using TuVan.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// Dapper: ánh xạ cột snake_case sang thuộc tính PascalCase nếu cần
DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddSingleton<Db>();
builder.Services.AddScoped<KeHoachRepository>();
builder.Services.AddScoped<HuongDanRepository>();
builder.Services.AddScoped<MucTieuRepository>();
builder.Services.AddScoped<NguoiDungRepository>();
builder.Services.AddScoped<DuyetRepository>();
builder.Services.AddScoped<CaloRepository>();
builder.Services.AddScoped<TichMonRepository>();
builder.Services.AddHostedService<HangDoiWorker>();

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", o =>
{
    o.Authority = builder.Configuration["Oidc:Authority"];
    o.Audience = builder.Configuration["Oidc:Audience"];
    o.RequireHttpsMetadata = false;               // chỉ dùng khi chạy thử trên localhost
    o.MapInboundClaims = false;                   // giữ nguyên tên claim của Keycloak (sub, roles, name)
    o.TokenValidationParameters.RoleClaimType = "roles";
});
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ChuyenGiaSoan", p => p.RequireRole("ChuyenGiaSoan"));
    o.AddPolicy("ChuyenGiaDuyet", p => p.RequireRole("ChuyenGiaDuyet"));
    o.AddPolicy("NguoiDung", p => p.RequireAuthenticatedUser());
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["Cors:Origin"]!).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Bắt lỗi nghiệp vụ và trả về HTTP 422 kèm thông điệp rõ ràng
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (NghiepVuException ex)
    {
        ctx.Response.StatusCode = 422;
        await ctx.Response.WriteAsJsonAsync(new { loi = ex.Message });
    }
});

var buaHopLe = new HashSet<string> { "Sang", "Trua", "Toi", "Phu" };

async Task<Guid> NguoiHienTai(ClaimsPrincipal u, KeHoachRepository repo)
{
    var sub = u.FindFirst("sub")?.Value
       ?? u.FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? throw new UnauthorizedAccessException();
    return await repo.MaNguoiDungTuSubAsync(sub)
        ?? throw new NghiepVuException("Tài khoản chưa được đăng ký trong hệ thống.");
}

// ---------- Đăng ký và hồ sơ khách ----------
app.MapPost("/api/dang-ky/dong-bo", async (ClaimsPrincipal u, NguoiDungRepository nr) =>
{
    var sub = u.FindFirst("sub")?.Value
       ?? u.FindFirst(ClaimTypes.NameIdentifier)?.Value
       ?? throw new UnauthorizedAccessException();
    var ten = u.FindFirst("name")?.Value
       ?? u.FindFirst("preferred_username")?.Value
       ?? "Khách";
    var kq = await nr.DongBoAsync(sub, ten);
    return Results.Ok(new { maNguoiDung = kq.MaNguoiDung, coHoSo = kq.CoHoSo });
}).RequireAuthorization("NguoiDung");

app.MapGet("/api/ho-so", async (ClaimsPrincipal u, KeHoachRepository kr, NguoiDungRepository nr) =>
    Results.Ok(await nr.LayHoSoAsync(await NguoiHienTai(u, kr))))
   .RequireAuthorization("NguoiDung");

app.MapPut("/api/ho-so", async (HoSoReq req, ClaimsPrincipal u, KeHoachRepository kr, NguoiDungRepository nr) =>
{
    await nr.LuuHoSoAsync(await NguoiHienTai(u, kr), req);
    return Results.NoContent();
}).RequireAuthorization("NguoiDung");

// ---------- Danh mục ----------
app.MapGet("/api/nguon", async (KeHoachRepository r) => Results.Ok(await r.NguonDangHoatDongAsync()))
   .RequireAuthorization("NguoiDung");
app.MapGet("/api/mon-an", async (MucTieuRepository r) => Results.Ok(await r.DanhSachMonAsync()))
   .RequireAuthorization("NguoiDung");

// ---------- Mục tiêu (khách) ----------
app.MapGet("/api/muc-tieu", async (MucTieuRepository r) => Results.Ok(await r.DanhSachMucTieuAsync()))
   .RequireAuthorization("NguoiDung");

app.MapGet("/api/muc-tieu/{ma}/thuc-don", async (string ma, MucTieuRepository r) =>
    Results.Ok(await r.ThucDonMauAsync(ma)))
   .RequireAuthorization("NguoiDung");

// Khách chọn mục tiêu: không rủi ro thì áp dụng ngay; có rủi ro thì gửi yêu cầu kế hoạch riêng
app.MapPost("/api/muc-tieu/{ma}/chon", async (string ma, ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
{
    var nd = await NguoiHienTai(u, kr);
    var kq = await mr.ChonMucTieuAsync(nd, ma);
    return Results.Ok(kq);
}).RequireAuthorization("NguoiDung");

app.MapGet("/api/muc-tieu/cua-toi", async (ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
{
    var nd = await NguoiHienTai(u, kr);
    var kh = await mr.KeHoachMucTieuGanNhatAsync(nd);
    if (kh is null)
        return Results.Ok(new { plan = (object?)null, mon = Array.Empty<object>(), coKeHoachDangXuLy = false });
    var mon = await mr.ThucDonCuaKeHoachAsync(kh.MaKeHoach, nd);
    var dangXuLy = kh.TrangThai is "Nhap" or "ChoDuyet" or "DangApDung";
    return Results.Ok(new { plan = kh, mon, coKeHoachDangXuLy = dangXuLy });
}).RequireAuthorization("NguoiDung");

// ---------- Kế hoạch riêng (khách gửi, chuyên gia soạn xử lý) ----------
app.MapPost("/api/yeu-cau/ke-hoach-rieng", async (YeuCauRiengReq req, ClaimsPrincipal u, KeHoachRepository kr, HuongDanRepository hr) =>
{
    var nd = await NguoiHienTai(u, kr);
    var id = await hr.TaoYeuCauKeHoachRiengAsync(nd, req.GhiChu);
    return Results.Created($"/api/yeu-cau/{id}", new { maYeuCau = id });
}).RequireAuthorization("NguoiDung");

app.MapGet("/api/yeu-cau/cua-toi", async (ClaimsPrincipal u, KeHoachRepository kr, HuongDanRepository hr) =>
    Results.Ok(await hr.YeuCauCuaToiAsync(await NguoiHienTai(u, kr))))
   .RequireAuthorization("NguoiDung");

app.MapGet("/api/yeu-cau/cho", async (HuongDanRepository hr) =>
    Results.Ok(await hr.ChoChuyenGiaAsync()))
   .RequireAuthorization("ChuyenGiaSoan");

app.MapPost("/api/yeu-cau/{id:guid}/da-xu-ly", async (Guid id, HuongDanRepository hr) =>
{
    await hr.DaXuLyAsync(id);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

// ---------- Soạn kế hoạch (chuyên gia soạn) ----------
app.MapPost("/api/ke-hoach", async (CreateKeHoachReq req, ClaimsPrincipal u, KeHoachRepository r) =>
{
    var soan = await NguoiHienTai(u, r);
    var id = await r.TaoNhapAsync(soan, req.MaNguoiDung, req.TuNgay, req.DenNgay);
    return Results.Created($"/api/ke-hoach/{id}", new { maKeHoach = id });
}).RequireAuthorization("ChuyenGiaSoan");

app.MapPost("/api/ke-hoach/{id:guid}/khuyen-nghi", async (Guid id, AddKhuyenNghiReq req, ClaimsPrincipal u, KeHoachRepository r) =>
{
    var soan = await NguoiHienTai(u, r);
    await r.ThemKhuyenNghiAsync(id, soan, req.NoiDung, req.ThuTu, req.MaNguons);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

app.MapPost("/api/ke-hoach/{id:guid}/gui-duyet", async (Guid id, ClaimsPrincipal u, KeHoachRepository r) =>
{
    var soan = await NguoiHienTai(u, r);
    await r.GuiDuyetAsync(id, soan);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

app.MapGet("/api/ke-hoach/cua-toi-soan", async (ClaimsPrincipal u, KeHoachRepository r) =>
    Results.Ok(await r.KeHoachCuaNguoiSoanAsync(await NguoiHienTai(u, r))))
   .RequireAuthorization("ChuyenGiaSoan");

app.MapGet("/api/ke-hoach/{id:guid}/khuyen-nghi", async (Guid id, KeHoachRepository r) =>
    Results.Ok(await r.KhuyenNghiCuaAsync(id)))
   .RequireAuthorization("NguoiDung");

app.MapPost("/api/ke-hoach/{id:guid}/ap-dung", async (Guid id, ClaimsPrincipal u, KeHoachRepository r) =>
{
    await r.ApDungAsync(id, await NguoiHienTai(u, r));
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

// Thực đơn 7 ngày của kế hoạch
app.MapGet("/api/ke-hoach/{id:guid}/thuc-don", async (Guid id, ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
    Results.Ok(await mr.ThucDonCuaKeHoachAsync(id, await NguoiHienTai(u, kr))))
   .RequireAuthorization("NguoiDung");

app.MapPost("/api/ke-hoach/{id:guid}/thuc-don", async (Guid id, ThemMonReq req, ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
{
    if (!buaHopLe.Contains(req.BuaAn)) throw new NghiepVuException("Bữa ăn không hợp lệ.");
    var soan = await NguoiHienTai(u, kr);
    await mr.ThemMonAsync(id, soan, req.NgayThu, req.BuaAn, req.MaMonAn, req.KhauPhan);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

app.MapPost("/api/ke-hoach/{id:guid}/thuc-don/xoa", async (Guid id, XoaMonReq req, ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
{
    if (!buaHopLe.Contains(req.BuaAn)) throw new NghiepVuException("Bữa ăn không hợp lệ.");
    var soan = await NguoiHienTai(u, kr);
    await mr.XoaMonAsync(id, soan, req.NgayThu, req.BuaAn, req.MaMonAn);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaSoan");

app.MapGet("/api/ke-hoach/dang-ap-dung", async (ClaimsPrincipal u, KeHoachRepository kr, MucTieuRepository mr) =>
{
    var nd = await NguoiHienTai(u, kr);
    var kh = await mr.KeHoachDangApDungAsync(nd);
    if (kh is null) return Results.Ok(new { plan = (object?)null, mon = Array.Empty<object>() });
    var mon = await mr.ThucDonCuaKeHoachAsync(kh.MaKeHoach, nd);
    return Results.Ok(new { plan = kh, mon });
}).RequireAuthorization("NguoiDung");

// ---------- Chuyên gia duyệt ----------
app.MapGet("/api/duyet/cho-duyet", async (ClaimsPrincipal u, KeHoachRepository r) =>
    Results.Ok(await r.ChoDuyetAsync(await NguoiHienTai(u, r))))
   .RequireAuthorization("ChuyenGiaDuyet");

app.MapGet("/api/duyet/{id:guid}/chi-tiet", async (Guid id, ClaimsPrincipal u, KeHoachRepository kr, DuyetRepository dr) =>
{
    var nguoiXem = await NguoiHienTai(u, kr);
    var kh = await dr.KeHoachAsync(id) ?? throw new NghiepVuException("Không tìm thấy kế hoạch.");
    if (kh.MaNguoiSoan == nguoiXem)
        throw new NghiepVuException("BR02: bạn là người soạn kế hoạch này nên không thể thẩm định.");

    await dr.GhiNhanXemAsync(id, nguoiXem);

    var hoSo = await dr.HoSoKhachAsync(kh.MaNguoiDung);
    var thucDon = await dr.ThucDonAsync(id);
    var kn = await dr.KhuyenNghiAsync(id);

    var khuyenNghi = kn
        .GroupBy(x => (x.MaKhuyenNghi, x.NoiDung, x.ThuTu))
        .OrderBy(g => g.Key.ThuTu)
        .Select(g => new
        {
            noiDung = g.Key.NoiDung,
            thuTu = g.Key.ThuTu,
            nguons = g.Where(n => n.TenNguon != null)
                      .Select(n => new { tenNguon = n.TenNguon, donViCongBo = n.DonViCongBo, namCongBo = n.NamCongBo })
                      .ToList()
        })
        .ToList();

    return Results.Ok(new { plan = kh, hoSo, thucDon, khuyenNghi });
}).RequireAuthorization("ChuyenGiaDuyet");

app.MapPost("/api/duyet/{id:guid}/duyet", async (Guid id, ClaimsPrincipal u, KeHoachRepository r, MucTieuRepository mr) =>
{
    await r.RaQuyetDinhAsync(id, await NguoiHienTai(u, r), duyet: true, lyDo: null);
    await mr.KichHoatMauDaDuyetAsync(id);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaDuyet");

app.MapPost("/api/duyet/{id:guid}/tra-lai", async (Guid id, TraLaiReq req, ClaimsPrincipal u, KeHoachRepository r) =>
{
    await r.RaQuyetDinhAsync(id, await NguoiHienTai(u, r), duyet: false, lyDo: req.LyDo);
    return Results.NoContent();
}).RequireAuthorization("ChuyenGiaDuyet");

// ---------- Người dùng: nhật ký và tiến độ ----------
app.MapPost("/api/nhat-ky", async (GhiBuaAnReq req, ClaimsPrincipal u, KeHoachRepository r) =>
{
    if (!buaHopLe.Contains(req.BuaAn)) throw new NghiepVuException("Bữa ăn không hợp lệ.");
    await r.GhiNhatKyAsync(await NguoiHienTai(u, r), req.MaMonAn, req.NgayAn, req.BuaAn, req.KhoiLuong);
    return Results.NoContent();
}).RequireAuthorization("NguoiDung");

// Thực đơn của một ngày và trạng thái đã ăn để khách tích
app.MapGet("/api/nhat-ky/thuc-don", async (DateTime? ngay, ClaimsPrincipal u, KeHoachRepository kr, TichMonRepository tr) =>
    Results.Ok(await tr.ThucDonNgayAsync(await NguoiHienTai(u, kr), (ngay ?? DateTime.Today).Date)))
   .RequireAuthorization("NguoiDung");

// Lưu các món được tích trong một bữa
app.MapPost("/api/nhat-ky/tick", async (TickReq req, ClaimsPrincipal u, KeHoachRepository kr, TichMonRepository tr) =>
{
    await tr.LuuTickAsync(await NguoiHienTai(u, kr), req.NgayAn, req.BuaAn, req.MaMonAns);
    return Results.NoContent();
}).RequireAuthorization("NguoiDung");

app.MapGet("/api/tien-do", async (ClaimsPrincipal u, KeHoachRepository r) =>
    Results.Ok(await r.TienDoAsync(await NguoiHienTai(u, r))))
   .RequireAuthorization("NguoiDung");

// Tổng calo nạp vào theo ngày (mặc định 7 ngày gần nhất). Nhóm rủi ro cao bị ẩn số liệu.
app.MapGet("/api/calo", async (DateTime? tuNgay, DateTime? denNgay, ClaimsPrincipal u, KeHoachRepository kr, CaloRepository cr) =>
{
    var nd = await NguoiHienTai(u, kr);
    var den = (denNgay ?? DateTime.Today).Date;
    var tu = (tuNgay ?? den.AddDays(-6)).Date;
    return Results.Ok(await cr.LayAsync(nd, tu, den));
}).RequireAuthorization("NguoiDung");

app.Run();

// ---------- Kiểu dữ liệu nhận từ client ----------
public sealed record CreateKeHoachReq(Guid MaNguoiDung, DateTime TuNgay, DateTime DenNgay);
public sealed record AddKhuyenNghiReq(string NoiDung, int ThuTu, List<Guid> MaNguons);
public sealed record TraLaiReq(string LyDo);
public sealed record GhiBuaAnReq(Guid MaMonAn, DateTime NgayAn, string BuaAn, decimal KhoiLuong);
public sealed record ThemMonReq(string BuaAn, Guid MaMonAn, string KhauPhan, int NgayThu = 1);
public sealed record XoaMonReq(string BuaAn, Guid MaMonAn, int NgayThu = 1);
public sealed record YeuCauRiengReq(string GhiChu);
public sealed record TickReq(DateTime NgayAn, string BuaAn, List<Guid> MaMonAns);