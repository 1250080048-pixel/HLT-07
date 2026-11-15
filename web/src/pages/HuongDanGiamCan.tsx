import { useEffect, useState } from "react";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";

type HuongDan = { thuTu: number; noiDung: string; tenNguon: string; donViCongBo: string; namCongBo: number };
type Resp = { nhomRuiRoCao: boolean; thongBao: string; huongDan: HuongDan[] };

export default function HuongDanGiamCan() {
  const [data, setData] = useState<Resp | null>(null);
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  useEffect(() => {
    goi<Resp>("/api/huong-dan/giam-can").then(setData).catch((e) => setLoi(e.message));
  }, []);

  const guiYeuCau = async () => {
    setLoi(""); setOk("");
    try {
      await goi("/api/yeu-cau/giam-can", { method: "POST" });
      setOk("Đã gửi yêu cầu. Chuyên gia sẽ liên hệ và xác nhận kế hoạch cá nhân.");
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <ThongBao loi={loi} ok={ok} />
      {data && (
        <>
          <div className="card">
            <h2>Hướng dẫn giảm cân chung</h2>
            <p className="muted">{data.thongBao}</p>
            <ol>
              {data.huongDan.map((h) => (
                <li key={h.thuTu} style={{ marginBottom: 10 }}>
                  {h.noiDung}
                  <div className="muted">Nguồn: {h.tenNguon} ({h.donViCongBo}, {h.namCongBo})</div>
                </li>
              ))}
            </ol>
          </div>
          <div className="card">
            <h2>Kế hoạch cá nhân</h2>
            <p className="muted">
              Kế hoạch riêng cho bạn do chuyên gia soạn và duyệt. Bấm nút dưới đây để gửi yêu cầu.
            </p>
            <button className="btn" onClick={guiYeuCau}>Gửi yêu cầu cho chuyên gia</button>
          </div>
        </>
      )}
    </>
  );
}