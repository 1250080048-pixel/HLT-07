import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";

type YeuCau = { maYeuCau: string; maNguoiDung: string; hoTen: string; loai: string; ghiChu: string | null; thoiDiem: string };

const TEN_LOAI: Record<string, string> = {
  KeHoachRieng: "Kế hoạch riêng",
  MucTieu: "Theo mục tiêu",
  GiamCan: "Giảm cân",
};

export default function YeuCauCho() {
  const [ds, setDs] = useState<YeuCau[]>([]);
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  const tai = async () => setDs(await goi<YeuCau[]>("/api/yeu-cau/cho"));
  useEffect(() => { tai().catch((e) => setLoi(e.message)); }, []);

  const daXuLy = async (id: string) => {
    setLoi(""); setOk("");
    try {
      await goi(`/api/yeu-cau/${id}/da-xu-ly`, { method: "POST" });
      setOk("Đã đánh dấu xử lý.");
      await tai();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <ThongBao loi={loi} ok={ok} />
      <div className="card">
        <h2>Yêu cầu chờ chuyên gia soạn</h2>
        <p className="muted">
          Với mỗi yêu cầu, hãy vào <strong>Soạn kế hoạch</strong> để tạo bản nháp cho khách, thêm món ở{" "}
          <strong>Thực đơn kế hoạch</strong>, rồi gửi duyệt. Sau khi gửi duyệt, bấm <strong>Đã xử lý</strong> ở đây.
        </p>
        {ds.length === 0 ? <p className="muted">Không có yêu cầu nào.</p> : (
          <table>
            <thead><tr><th>Khách</th><th>Loại</th><th>Mô tả của khách</th><th>Thời điểm</th><th></th></tr></thead>
            <tbody>
              {ds.map((y) => (
                <tr key={y.maYeuCau}>
                  <td>
                    {y.hoTen}
                    <div className="muted">{y.maNguoiDung.slice(0, 8)}</div>
                  </td>
                  <td>{TEN_LOAI[y.loai] ?? y.loai}</td>
                  <td style={{ maxWidth: 360 }}>{y.ghiChu ?? "—"}</td>
                  <td>{new Date(y.thoiDiem).toLocaleString("vi-VN")}</td>
                  <td><button className="btn secondary" onClick={() => daXuLy(y.maYeuCau)}>Đã xử lý</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </>
  );
}