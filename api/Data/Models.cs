namespace TuVan.Api.Data;

public sealed record KeHoachItem(Guid MaKeHoach, Guid MaNguoiDung, Guid MaNguoiSoan,
    int PhienBan, string TrangThai, DateTime TuNgay, DateTime DenNgay);

public sealed record KhuyenNghiItem(Guid MaKhuyenNghi, string NoiDung, int ThuTu);

public sealed record NguonItem(Guid MaNguon, string TenNguon, string DonViCongBo, int NamCongBo);

public sealed record MonAnItem(Guid MaMonAn, string TenMon, decimal? NangLuong);

public sealed record TienDoItem(Guid MaKeHoach, int TongNgay, int SoNgayDaGhi, decimal TyLePhanTram);

public sealed record ViecItem(Guid MaViec, string LoaiViec, Guid MaDoiTuong);
