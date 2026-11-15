import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";
import PageHead from "../components/PageHead";
import { IllusBat } from "../components/Illustrations";
import { ngayHomNay } from "../lib/ngay";

type MonAn = { maMonAn: string; tenMon: string; nhomThucPham: string; nangLuong: number | null };
type TienDo = { maKeHoach: string; tongNgay: number; soNgayDaGhi: number; tyLePhanTram: number };
type MonTich = { maMonAn: string; tenMon: string; buaAn: string; khauPhan: string; daAn: boolean };
type ThucDonNgay = { ngay: string; coKeHoach: boolean; trongKhoang: boolean; ngayThu: number; mon: MonTich[] };

const TEN_BUA: Record<string, string> = {
  Sang: "Bữa sáng",
  Trua: "Bữa trưa",
  Toi: "Bữa tối",
  Phu: "Bữa phụ",
};

export default function NhatKyPage() {
  const [ngayAn, setNgayAn] = useState(ngayHomNay());
  const [buaAn, setBuaAn] = useState("Sang");
  const [thucDon, setThucDon] = useState<ThucDonNgay | null>(null);
  const [chon, setChon] = useState<string[]>([]);
  const [monAns, setMonAns] = useState<MonAn[]>([]);
  const [tienDo, setTienDo] = useState<TienDo[]>([]);
  const [ngoai, setNgoai] = useState({ maMonAn: "", khoiLuong: 100 });
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  const taiTienDo = async () => setTienDo(await goi<TienDo[]>("/api/tien-do"));

  const taiThucDon = async (ngay: string, bua: string) => {
    const r = await goi<ThucDonNgay>(`/api/nhat-ky/thuc-don?ngay=${ngay}`);
    setThucDon(r);
    setChon(r.mon.filter((m) => m.buaAn === bua && m.daAn).map((m) => m.maMonAn));
  };

  useEffect(() => {
    goi<MonAn[]>("/api/mon-an").then(setMonAns).catch((e) => setLoi(e.message));
    taiTienDo().catch((e) => setLoi(e.message));
  }, []);

  useEffect(() => {
    setLoi("");
    setOk("");
    taiThucDon(ngayAn, buaAn).catch((e) => setLoi(e instanceof ApiError ? e.message : String(e)));
  }, [ngayAn, buaAn]);

  const monBua = thucDon?.mon.filter((m) => m.buaAn === buaAn) ?? [];

  const bat = (ma: string) =>
    setChon((c) => (c.includes(ma) ? c.filter((x) => x !== ma) : [...c, ma]));

  const luuBua = async () => {
    setLoi("");
    setOk("");
    try {
      await goi("/api/nhat-ky/tick", {
        method: "POST",
        body: JSON.stringify({ ngayAn, buaAn, maMonAns: chon }),
      });
      setOk(`Đã lưu ${TEN_BUA[buaAn].toLowerCase()} theo lựa chọn của bạn.`);
      await taiThucDon(ngayAn, buaAn);
      await taiTienDo();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  const ghiNgoai = async () => {
    setLoi("");
    setOk("");
    try {
      await goi("/api/nhat-ky", {
        method: "POST",
        body: JSON.stringify({
          maMonAn: ngoai.maMonAn,
          ngayAn,
          buaAn,
          khoiLuong: Number(ngoai.khoiLuong),
        }),
      });
      setOk("Đã ghi món ngoài thực đơn.");
      setNgoai({ maMonAn: "", khoiLuong: 100 });
      await taiTienDo();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <PageHead
        nhan="Ghi nhận của bạn"
        tieuDe="Nhật ký ăn uống"
        moTa="Tích những món bạn đã ăn trong thực đơn của ngày. Số liệu dinh dưỡng chỉ là ước tính."
        minhHoa={<IllusBat size={110} />}
      />

      <ThongBao loi={loi} ok={ok} />

      <div className="card">
        <h2>Ngày và bữa ăn</h2>
        <div className="grid">
          <div>
            <label>Ngày ăn</label>
            <input type="date" value={ngayAn} onChange={(e) => setNgayAn(e.target.value)} />
          </div>
          <div>
            <label>Bữa ăn</label>
            <select value={buaAn} onChange={(e) => setBuaAn(e.target.value)}>
              {Object.keys(TEN_BUA).map((b) => (
                <option key={b} value={b}>{TEN_BUA[b]}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      <div className="card">
        {!thucDon?.coKeHoach ? (
          <>
            <h2>Thực đơn của ngày</h2>
            <p className="muted">
              Bạn chưa có kế hoạch đang áp dụng. Vào <strong>Mục tiêu</strong> để bắt đầu.
            </p>
          </>
        ) : !thucDon.trongKhoang ? (
          <>
            <h2>Thực đơn của ngày</h2>
            <p className="muted">Ngày này nằm ngoài thời gian kế hoạch đang áp dụng.</p>
          </>
        ) : monBua.length === 0 ? (
          <>
            <h2>{TEN_BUA[buaAn]}</h2>
            <p className="muted">Bữa này chưa có món trong thực đơn ngày {thucDon.ngayThu}.</p>
          </>
        ) : (
          <>
            <h2>{TEN_BUA[buaAn]} · thực đơn ngày {thucDon.ngayThu}</h2>
            <p className="muted">Tích vào các món bạn đã ăn, rồi bấm lưu.</p>
            <table>
              <tbody>
                {monBua.map((m) => (
                  <tr key={m.maMonAn}>
                    <td style={{ width: 40 }}>
                      <input
                        type="checkbox"
                        style={{ width: "auto", margin: 0 }}
                        checked={chon.includes(m.maMonAn)}
                        onChange={() => bat(m.maMonAn)}
                      />
                    </td>
                    <td>{m.tenMon}</td>
                    <td className="muted">{m.khauPhan}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <button className="btn" style={{ marginTop: 16 }} onClick={luuBua}>
              Lưu {TEN_BUA[buaAn].toLowerCase()}
            </button>
          </>
        )}
      </div>

      <div className="card">
        <h2>Món ngoài thực đơn</h2>
        <p className="muted">Nếu bạn ăn món không có trong thực đơn, ghi ở đây.</p>
        <div className="grid">
          <div>
            <label>Món ăn</label>
            <select value={ngoai.maMonAn} onChange={(e) => setNgoai({ ...ngoai, maMonAn: e.target.value })}>
              <option value="">-- Chọn món --</option>
              {monAns.map((m) => (
                <option key={m.maMonAn} value={m.maMonAn}>
                  {m.tenMon}
                  {m.nhomThucPham ? ` (${m.nhomThucPham})` : ""}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label>Khối lượng (gam)</label>
            <input
              type="number"
              min={1}
              value={ngoai.khoiLuong}
              onChange={(e) => setNgoai({ ...ngoai, khoiLuong: Number(e.target.value) })}
            />
          </div>
        </div>
        <button className="btn" disabled={!ngoai.maMonAn} onClick={ghiNgoai}>
          Ghi món ngoài thực đơn
        </button>
      </div>

      <div className="card">
        <h2>Tiến độ theo kế hoạch đang áp dụng</h2>
        {tienDo.length === 0 ? (
          <p className="muted">Chưa có kế hoạch nào đang áp dụng.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Kế hoạch</th>
                <th>Số ngày</th>
                <th>Đã ghi</th>
                <th>Tỷ lệ</th>
              </tr>
            </thead>
            <tbody>
              {tienDo.map((t) => (
                <tr key={t.maKeHoach}>
                  <td className="muted">{t.maKeHoach.slice(0, 8)}</td>
                  <td>{t.tongNgay}</td>
                  <td>{t.soNgayDaGhi}</td>
                  <td>
                    <span className={`badge ${t.tyLePhanTram >= 80 ? "ok" : "warn"}`}>
                      {t.tyLePhanTram}%
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}