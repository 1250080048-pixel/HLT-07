import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";

type YeuCau = { maYeuCau: string; loai: string; trangThai: string; ghiChu: string | null; thoiDiem: string };

const TEN_TRANG_THAI: Record<string, string> = {
  ChoChuyenGia: "Đang chờ chuyên gia",
  DaXuLy: "Chuyên gia đã tiếp nhận",
};

export default function KeHoachRieng() {
  const [ds, setDs] = useState<YeuCau[]>([]);
  const [ghiChu, setGhiChu] = useState("");
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  const tai = async () => setDs(await goi<YeuCau[]>("/api/yeu-cau/cua-toi"));
  useEffect(() => { tai().catch((e) => setLoi(e.message)); }, []);

  const gui = async () => {
    setLoi(""); setOk("");
    try {
      await goi("/api/yeu-cau/ke-hoach-rieng", { method: "POST", body: JSON.stringify({ ghiChu }) });
      setOk("Đã gửi yêu cầu. Chuyên gia sẽ soạn kế hoạch riêng cho bạn.");
      setGhiChu("");
      await tai();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <ThongBao loi={loi} ok={ok} />

      <div className="card">
        <h2>Yêu cầu kế hoạch riêng</h2>
        <p className="muted">
          Chuyên gia dinh dưỡng sẽ soạn một kế hoạch riêng cho bạn, dựa trên hồ sơ và mục tiêu bạn mô tả.
          Kế hoạch chỉ được áp dụng sau khi chuyên gia khác xác nhận.
        </p>
        <label>Mô tả mục tiêu và tình trạng của bạn (10 - 500 ký tự)</label>
        <textarea
          value={ghiChu}
          onChange={(e) => setGhiChu(e.target.value)}
          placeholder="Ví dụ: Tôi muốn giảm 5 kg trong 2 tháng, làm việc văn phòng, ít vận động..."
        />
        <p className="muted">
          Mô tả này cùng hồ sơ của bạn chỉ được chuyên gia phụ trách xem. Không đưa thông tin nhận dạng cá nhân vào đây.
        </p>
        <button className="btn" disabled={ghiChu.trim().length < 10} onClick={gui}>Gửi yêu cầu</button>
      </div>

      <div className="card">
        <h2>Yêu cầu của tôi</h2>
        {ds.length === 0 ? <p className="muted">Bạn chưa gửi yêu cầu nào.</p> : (
          <table>
            <thead><tr><th>Thời điểm</th><th>Mô tả</th><th>Trạng thái</th></tr></thead>
            <tbody>
              {ds.map((y) => (
                <tr key={y.maYeuCau}>
                  <td>{new Date(y.thoiDiem).toLocaleString("vi-VN")}</td>
                  <td>{y.ghiChu ?? "—"}</td>
                  <td>
                    <span className={`badge ${y.trangThai === "DaXuLy" ? "ok" : "warn"}`}>
                      {TEN_TRANG_THAI[y.trangThai] ?? y.trangThai}
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