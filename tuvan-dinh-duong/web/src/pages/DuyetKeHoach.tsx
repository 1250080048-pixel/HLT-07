import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";
import ThucDonView, { MonTD } from "../components/ThucDonView";

type KeHoach = { maKeHoach: string; maNguoiDung: string; phienBan: number; tuNgay: string; denNgay: string };
type ChiTiet = {
  plan: { maKeHoach: string; maNguoiDung: string; maNguoiSoan: string; phienBan: number; trangThai: string; tuNgay: string; denNgay: string; maMucTieu: string | null };
  hoSo: { hoTen: string; chieuCao: number | null; canNang: number | null; diUng: string | null; thuocNhomRuiRoCao: boolean } | null;
  thucDon: MonTD[];
  khuyenNghi: { noiDung: string; thuTu: number; nguons: { tenNguon: string; donViCongBo: string; namCongBo: number }[] }[];
};

const TEN_MUC_TIEU: Record<string, string> = {
  GiamCan: "Giảm cân",
  TangCan: "Tăng cân lành mạnh",
  TangCo: "Tăng cơ",
  DuyTri: "Duy trì cân nặng",
};

export default function DuyetKeHoach() {
  const [ds, setDs] = useState<KeHoach[]>([]);
  const [chiTiet, setChiTiet] = useState<ChiTiet | null>(null);
  const [lyDo, setLyDo] = useState("");
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  const taiDanhSach = async () => setDs(await goi<KeHoach[]>("/api/duyet/cho-duyet"));
  useEffect(() => { taiDanhSach().catch((e) => setLoi(e.message)); }, []);

  const mo = async (id: string) => {
    setLoi(""); setOk(""); setLyDo("");
    try {
      setChiTiet(await goi<ChiTiet>(`/api/duyet/${id}/chi-tiet`));
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  const quyetDinh = async (duyet: boolean) => {
    if (!chiTiet) return;
    setLoi(""); setOk("");
    try {
      await goi(`/api/duyet/${chiTiet.plan.maKeHoach}/${duyet ? "duyet" : "tra-lai"}`, {
        method: "POST",
        body: duyet ? undefined : JSON.stringify({ lyDo }),
      });
      setOk(duyet ? "Đã duyệt kế hoạch." : "Đã trả lại kế hoạch kèm lý do.");
      setChiTiet(null);
      setLyDo("");
      await taiDanhSach();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <ThongBao loi={loi} ok={ok} />

      <div className="card">
        <h2>Hàng đợi duyệt</h2>
        <p className="muted">Bạn chỉ thấy kế hoạch do người khác soạn (BR02).</p>
        {ds.length === 0 ? <p className="muted">Không có kế hoạch nào chờ duyệt.</p> : (
          <table>
            <thead><tr><th>Mã</th><th>Từ - đến</th><th>Phiên bản</th><th></th></tr></thead>
            <tbody>
              {ds.map((k) => (
                <tr key={k.maKeHoach}>
                  <td className="muted">{k.maKeHoach.slice(0, 8)}</td>
                  <td>{k.tuNgay.slice(0, 10)} → {k.denNgay.slice(0, 10)}</td>
                  <td>{k.phienBan}</td>
                  <td><button className="btn secondary" onClick={() => mo(k.maKeHoach)}>Xem chi tiết</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {chiTiet && (
        <>
          <div className="card">
            <h2>Kế hoạch {chiTiet.plan.maKeHoach.slice(0, 8)}</h2>
            <p className="muted">
              Từ {chiTiet.plan.tuNgay.slice(0, 10)} đến {chiTiet.plan.denNgay.slice(0, 10)}
              {chiTiet.plan.maMucTieu && <> · Theo mục tiêu: <strong>{TEN_MUC_TIEU[chiTiet.plan.maMucTieu] ?? chiTiet.plan.maMucTieu}</strong></>}
            </p>
          </div>

          <div className="card">
            <h2>Hồ sơ khách</h2>
            {chiTiet.hoSo ? (
              <table>
                <tbody>
                  <tr><th>Họ tên</th><td>{chiTiet.hoSo.hoTen}</td></tr>
                  <tr><th>Chiều cao</th><td>{chiTiet.hoSo.chieuCao ?? "—"} cm</td></tr>
                  <tr><th>Cân nặng</th><td>{chiTiet.hoSo.canNang ?? "—"} kg</td></tr>
                  <tr><th>Dị ứng / ghi chú</th><td>{chiTiet.hoSo.diUng || "—"}</td></tr>
                  <tr>
                    <th>Nhóm rủi ro cao (tự khai)</th>
                    <td>
                      {chiTiet.hoSo.thuocNhomRuiRoCao
                        ? <span className="badge warn">Có: cần tư vấn trực tiếp</span>
                        : "Không"}
                    </td>
                  </tr>
                </tbody>
              </table>
            ) : <p className="muted">Khách chưa khai báo hồ sơ.</p>}
            <p className="muted">Hồ sơ chỉ dùng để thẩm định kế hoạch. Mỗi lần xem được ghi vào nhật ký.</p>
          </div>

          <div className="card">
            <h2>Thực đơn khách đã chọn</h2>
            <ThucDonView mon={chiTiet.thucDon} />
          </div>

          <div className="card">
            <h2>Khuyến nghị và nguồn</h2>
            {chiTiet.khuyenNghi.length === 0 ? <p className="muted">Chưa có khuyến nghị.</p> : (
              <ol>
                {chiTiet.khuyenNghi.map((k) => (
                  <li key={k.thuTu} style={{ marginBottom: 10 }}>
                    {k.noiDung}
                    {k.nguons.length === 0
                      ? <div className="msg-err" style={{ marginTop: 4 }}>Thiếu nguồn (BR04)</div>
                      : k.nguons.map((n, i) => (
                          <div key={i} className="muted">Nguồn: {n.tenNguon} ({n.donViCongBo}, {n.namCongBo})</div>
                        ))}
                  </li>
                ))}
              </ol>
            )}
          </div>

          <div className="card">
            <h2>Quyết định</h2>
            <label>Lý do trả lại (bắt buộc khi trả lại)</label>
            <textarea value={lyDo} onChange={(e) => setLyDo(e.target.value)} />
            <div className="row">
              <button className="btn" onClick={() => quyetDinh(true)}>Duyệt</button>
              <button className="btn danger" disabled={!lyDo.trim()} onClick={() => quyetDinh(false)}>Trả lại</button>
            </div>
          </div>
        </>
      )}
    </>
  );
}