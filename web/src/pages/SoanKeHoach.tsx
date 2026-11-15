import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";

type Nguon = { maNguon: string; tenNguon: string; donViCongBo: string; namCongBo: number };
type KeHoach = { maKeHoach: string; trangThai: string; phienBan: number; tuNgay: string; denNgay: string };
type KhuyenNghi = { maKhuyenNghi: string; noiDung: string; thuTu: number };

export default function SoanKeHoach() {
  const [nguons, setNguons] = useState<Nguon[]>([]);
  const [dsKeHoach, setDsKeHoach] = useState<KeHoach[]>([]);
  const [dangSua, setDangSua] = useState<string | null>(null);
  const [khuyenNghis, setKhuyenNghis] = useState<KhuyenNghi[]>([]);
  const [tao, setTao] = useState({ maNguoiDung: "", tuNgay: today(), denNgay: today() });
  const [kn, setKn] = useState({ noiDung: "", maNguons: [] as string[] });
  const [loi, setLoi] = useState(""); const [ok, setOk] = useState("");

  const taiDanhSach = async () => setDsKeHoach(await goi<KeHoach[]>("/api/ke-hoach/cua-toi-soan"));
  useEffect(() => {
    goi<Nguon[]>("/api/nguon").then(setNguons).catch((e) => setLoi(e.message));
    taiDanhSach().catch((e) => setLoi(e.message));
  }, []);

  const chay = async (fn: () => Promise<void>, thanhCong: string) => {
    setLoi(""); setOk("");
    try { await fn(); setOk(thanhCong); } catch (e) { setLoi(e instanceof ApiError ? e.message : String(e)); }
  };

  const taoKeHoach = () => chay(async () => {
    const res = await goi<{ maKeHoach: string }>("/api/ke-hoach", { method: "POST", body: JSON.stringify(tao) });
    setDangSua(res.maKeHoach);
    await taiDanhSach();
  }, "Đã tạo bản nháp. Thêm khuyến nghị và gắn nguồn bên dưới.");

  const chonKeHoach = async (id: string) => {
    setDangSua(id);
    setKhuyenNghis(await goi<KhuyenNghi[]>(`/api/ke-hoach/${id}/khuyen-nghi`));
  };

  const themKn = () => chay(async () => {
    if (!dangSua) return;
    await goi(`/api/ke-hoach/${dangSua}/khuyen-nghi`, {
      method: "POST",
      body: JSON.stringify({ noiDung: kn.noiDung, thuTu: khuyenNghis.length + 1, maNguons: kn.maNguons }),
    });
    setKn({ noiDung: "", maNguons: [] });
    setKhuyenNghis(await goi<KhuyenNghi[]>(`/api/ke-hoach/${dangSua}/khuyen-nghi`));
  }, "Đã thêm khuyến nghị.");

  const guiDuyet = () => chay(async () => {
    if (!dangSua) return;
    await goi(`/api/ke-hoach/${dangSua}/gui-duyet`, { method: "POST" });
    await taiDanhSach();
  }, "Đã gửi duyệt. Nội dung không sửa được cho đến khi có kết quả.");

  const apDung = (id: string) => chay(async () => {
    await goi(`/api/ke-hoach/${id}/ap-dung`, { method: "POST" });
    await taiDanhSach();
  }, "Đã áp dụng kế hoạch cho khách.");

  const toggleNguon = (id: string) =>
    setKn((s) => ({ ...s, maNguons: s.maNguons.includes(id) ? s.maNguons.filter((x) => x !== id) : [...s.maNguons, id] }));

  return (
    <>
      <ThongBao loi={loi} ok={ok} />

      <div className="card">
        <h2>Tạo bản nháp kế hoạch</h2>
        <div className="grid">
          <div><label>Mã người dùng (khách)</label><input value={tao.maNguoiDung} onChange={(e) => setTao({ ...tao, maNguoiDung: e.target.value })} /></div>
          <div><label>Từ ngày</label><input type="date" value={tao.tuNgay} onChange={(e) => setTao({ ...tao, tuNgay: e.target.value })} /></div>
          <div><label>Đến ngày</label><input type="date" value={tao.denNgay} onChange={(e) => setTao({ ...tao, denNgay: e.target.value })} /></div>
        </div>
        <button className="btn" onClick={taoKeHoach} disabled={!tao.maNguoiDung}>Tạo bản nháp</button>
      </div>

      <div className="card">
        <h2>Kế hoạch của tôi</h2>
        <table>
          <thead><tr><th>Mã</th><th>Từ - đến</th><th>Trạng thái</th><th></th></tr></thead>
          <tbody>
            {dsKeHoach.map((k) => (
              <tr key={k.maKeHoach}>
                <td className="muted">{k.maKeHoach.slice(0, 8)}</td>
                <td>{k.tuNgay} → {k.denNgay}</td>
                <td><span className={`badge ${k.trangThai === "DangApDung" ? "ok" : k.trangThai === "ChoDuyet" ? "warn" : "info"}`}>{k.trangThai}</span></td>
                <td className="row">
                  <button className="btn secondary" onClick={() => chonKeHoach(k.maKeHoach)}>Mở</button>
                  {k.trangThai === "DaDuyet" && <button className="btn" onClick={() => apDung(k.maKeHoach)}>Áp dụng</button>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {dangSua && (
        <div className="card">
          <h2>Khuyến nghị của kế hoạch {dangSua.slice(0, 8)}</h2>
          <table>
            <thead><tr><th>#</th><th>Nội dung</th></tr></thead>
            <tbody>
              {khuyenNghis.map((x) => <tr key={x.maKhuyenNghi}><td>{x.thuTu}</td><td>{x.noiDung}</td></tr>)}
            </tbody>
          </table>

          <label style={{ marginTop: 16 }}>Nội dung khuyến nghị</label>
          <textarea value={kn.noiDung} onChange={(e) => setKn({ ...kn, noiDung: e.target.value })} />
          <label>Nguồn tham khảo (bắt buộc, BR04)</label>
          <div className="row" style={{ marginBottom: 12 }}>
            {nguons.map((n) => (
              <label key={n.maNguon} className="row" style={{ margin: 0, color: "var(--text)" }}>
                <input type="checkbox" style={{ width: "auto", margin: 0 }}
                  checked={kn.maNguons.includes(n.maNguon)} onChange={() => toggleNguon(n.maNguon)} />
                {n.tenNguon} ({n.donViCongBo}, {n.namCongBo})
              </label>
            ))}
          </div>
          <div className="row">
            <button className="btn" onClick={themKn} disabled={!kn.noiDung || kn.maNguons.length === 0}>Thêm khuyến nghị</button>
            <button className="btn secondary" onClick={guiDuyet}>Gửi duyệt</button>
          </div>
        </div>
      )}
    </>
  );
}

function today() { return new Date().toISOString().slice(0, 10); }
