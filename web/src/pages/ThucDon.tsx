import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";
import ThucDonView, { MonTD } from "../components/ThucDonView";

type KeHoach = { maKeHoach: string; trangThai: string; tuNgay: string; denNgay: string };
type MonAn = { maMonAn: string; tenMon: string; nhomThucPham: string; nangLuong: number | null };

export default function ThucDon() {
  const [ds, setDs] = useState<KeHoach[]>([]);
  const [monAns, setMonAns] = useState<MonAn[]>([]);
  const [chon, setChon] = useState<string | null>(null);
  const [mon, setMon] = useState<MonTD[]>([]);
  const [ngay, setNgay] = useState(1);
  const [form, setForm] = useState({ buaAn: "Sang", maMonAn: "", khauPhan: "" });
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  useEffect(() => {
    goi<KeHoach[]>("/api/ke-hoach/cua-toi-soan")
      .then((r) => setDs(r.filter((k) => k.trangThai === "Nhap")))
      .catch((e) => setLoi(e.message));
    goi<MonAn[]>("/api/mon-an").then(setMonAns).catch((e) => setLoi(e.message));
  }, []);

  const mo = async (id: string) => {
    setLoi(""); setOk(""); setChon(id);
    setMon(await goi<MonTD[]>(`/api/ke-hoach/${id}/thuc-don`));
  };

  const them = async () => {
    if (!chon) return;
    setLoi(""); setOk("");
    try {
      await goi(`/api/ke-hoach/${chon}/thuc-don`, {
        method: "POST",
        body: JSON.stringify({ ...form, ngayThu: ngay }),
      });
      setOk(`Đã thêm món vào bữa của ngày ${ngay}.`);
      setForm({ ...form, maMonAn: "", khauPhan: "" });
      setMon(await goi<MonTD[]>(`/api/ke-hoach/${chon}/thuc-don`));
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  const xoa = async (ngayXoa: number, bua: string, maMon: string) => {
    if (!chon) return;
    setLoi(""); setOk("");
    try {
      await goi(`/api/ke-hoach/${chon}/thuc-don/xoa`, {
        method: "POST",
        body: JSON.stringify({ buaAn: bua, maMonAn: maMon, ngayThu: ngayXoa }),
      });
      setMon(await goi<MonTD[]>(`/api/ke-hoach/${chon}/thuc-don`));
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <ThongBao loi={loi} ok={ok} />
      <div className="card">
        <h2>Kế hoạch đang soạn (Nháp)</h2>
        {ds.length === 0 ? <p className="muted">Không có kế hoạch nháp nào.</p> : (
          <table>
            <thead><tr><th>Mã</th><th>Từ - đến</th><th></th></tr></thead>
            <tbody>
              {ds.map((k) => (
                <tr key={k.maKeHoach}>
                  <td className="muted">{k.maKeHoach.slice(0, 8)}</td>
                  <td>{k.tuNgay.slice(0, 10)} → {k.denNgay.slice(0, 10)}</td>
                  <td><button className="btn secondary" onClick={() => mo(k.maKeHoach)}>Soạn thực đơn</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {chon && (
        <>
          <div className="card">
            <h2>Thêm món vào thực đơn</h2>
            <div className="grid">
              <div>
                <label>Ngày thứ</label>
                <select value={ngay} onChange={(e) => setNgay(Number(e.target.value))}>
                  {[1, 2, 3, 4, 5, 6, 7].map((n) => <option key={n} value={n}>Ngày {n}</option>)}
                </select>
              </div>
              <div>
                <label>Bữa ăn</label>
                <select value={form.buaAn} onChange={(e) => setForm({ ...form, buaAn: e.target.value })}>
                  <option value="Sang">Sáng</option>
                  <option value="Trua">Trưa</option>
                  <option value="Toi">Tối</option>
                  <option value="Phu">Phụ</option>
                </select>
              </div>
              <div>
                <label>Món ăn</label>
                <select value={form.maMonAn} onChange={(e) => setForm({ ...form, maMonAn: e.target.value })}>
                  <option value="">-- Chọn món --</option>
                  {monAns.map((m) => <option key={m.maMonAn} value={m.maMonAn}>{m.tenMon}</option>)}
                </select>
              </div>
              <div>
                <label>Khẩu phần</label>
                <input value={form.khauPhan} onChange={(e) => setForm({ ...form, khauPhan: e.target.value })} placeholder="ví dụ: 1 chén" />
              </div>
            </div>
            <button className="btn" disabled={!form.maMonAn || !form.khauPhan} onClick={them}>Thêm món</button>
          </div>

          <div className="card">
            <h2>Thực đơn của kế hoạch {chon.slice(0, 8)}</h2>
            <ThucDonView mon={mon} onXoa={xoa} ngayChon={ngay} onDoiNgay={setNgay} />
          </div>
        </>
      )}
    </>
  );
}