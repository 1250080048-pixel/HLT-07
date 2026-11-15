import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";

type CaloNgay = { ngay: string; tongKcal: number; trangThai: string | null; thongBao: string | null };
type KetQua = {
  anSo: boolean;
  lyDoAnSo: string | null;
  thamChieu: { kcalMin: number; kcalMax: number; nguon: string | null } | null;
  ngay: CaloNgay[];
};

const TEN_TRANG_THAI: Record<string, string> = {
  thap: "Thấp hơn tham khảo",
  trong: "Trong khoảng tham khảo",
  cao: "Cao hơn tham khảo",
};

export default function CaloCard() {
  const [kq, setKq] = useState<KetQua | null>(null);
  const [loi, setLoi] = useState("");

  useEffect(() => {
    goi<KetQua>("/api/calo")
      .then(setKq)
      .catch((e) => setLoi(e instanceof ApiError ? e.message : String(e)));
  }, []);

  if (loi) return <div className="card"><div className="msg-err">{loi}</div></div>;
  if (!kq) return null;

  if (kq.anSo) {
    return (
      <div className="card">
        <h2>Calo nạp vào</h2>
        <p className="muted">{kq.lyDoAnSo}</p>
      </div>
    );
  }

  return (
    <div className="card">
      <h2>Calo nạp vào (7 ngày gần nhất)</h2>
      {kq.thamChieu ? (
        <p className="muted">
          Khoảng tham khảo của mục tiêu: {kq.thamChieu.kcalMin} – {kq.thamChieu.kcalMax} kcal/ngày
          {kq.thamChieu.nguon ? ` · Nguồn: ${kq.thamChieu.nguon}` : ""}.
          Số liệu chỉ để theo dõi, không phải khuyến nghị.
        </p>
      ) : (
        <p className="muted">Chưa có kế hoạch đang áp dụng nên chưa có khoảng tham khảo.</p>
      )}

      {kq.ngay.length === 0 ? (
        <p className="muted">Chưa có ghi nhật ký trong khoảng này.</p>
      ) : (
        <table>
          <thead><tr><th>Ngày</th><th>Tổng kcal</th><th>So với tham khảo</th></tr></thead>
          <tbody>
            {kq.ngay.map((n) => (
              <tr key={n.ngay}>
                <td>{n.ngay.slice(0, 10)}</td>
                <td>{Math.round(n.tongKcal)}</td>
                <td>
                  {n.trangThai ? (
                    <span className={`badge ${n.trangThai === "trong" ? "ok" : "warn"}`}>
                      {TEN_TRANG_THAI[n.trangThai]}
                    </span>
                  ) : "—"}
                  {n.thongBao && <div className="muted">{n.thongBao}</div>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}