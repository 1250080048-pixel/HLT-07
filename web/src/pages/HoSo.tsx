import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";
import PageHead from "../components/PageHead";
import { IllusHoSo } from "../components/Illustrations";

type HoSo = {
  chieuCao: number | null;
  canNang: number | null;
  diUng: string | null;
  thuocNhomRuiRoCao: boolean;
  diUngNhom: string | null;
} | null;

const NHOM_DI_UNG = [
  { ma: "HaiSan", ten: "Hải sản / cá" },
  { ma: "Trung", ten: "Trứng" },
  { ma: "Sua", ten: "Sữa" },
  { ma: "DauNanh", ten: "Đậu nành (đậu hũ, sữa đậu nành)" },
  { ma: "Gluten", ten: "Lúa mì / gluten (bột mì, yến mạch, phở)" },
  { ma: "DauPhong", ten: "Đậu phộng" },
];

export default function HoSoPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    hoTen: "",
    chieuCao: "",
    canNang: "",
    diUng: "",
    thuocNhomRuiRoCao: false,
    diUngNhom: [] as string[],
  });
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  useEffect(() => {
    goi<HoSo>("/api/ho-so")
      .then((h) => {
        if (!h) return;
        setForm((f) => ({
          ...f,
          chieuCao: h.chieuCao?.toString() ?? "",
          canNang: h.canNang?.toString() ?? "",
          diUng: h.diUng ?? "",
          thuocNhomRuiRoCao: h.thuocNhomRuiRoCao,
          diUngNhom: h.diUngNhom ? h.diUngNhom.split(",") : [],
        }));
      })
      .catch((e) => setLoi(e.message));
  }, []);

  const bat = (ma: string) =>
    setForm((f) => ({
      ...f,
      diUngNhom: f.diUngNhom.includes(ma)
        ? f.diUngNhom.filter((x) => x !== ma)
        : [...f.diUngNhom, ma],
    }));

  const luu = async () => {
    setLoi(""); setOk("");
    try {
      await goi("/api/ho-so", {
        method: "PUT",
        body: JSON.stringify({
          hoTen: form.hoTen,
          chieuCao: form.chieuCao ? Number(form.chieuCao) : null,
          canNang: form.canNang ? Number(form.canNang) : null,
          diUng: form.diUng || null,
          thuocNhomRuiRoCao: form.thuocNhomRuiRoCao,
          diUngNhom: form.diUngNhom,
        }),
      });
      setOk("Đã lưu hồ sơ.");
      setTimeout(() => navigate("/", { replace: true }), 600);
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  return (
    <>
      <PageHead
        nhan="Hồ sơ"
        tieuDe="Hồ sơ của tôi"
        moTa="Thông tin này giúp chuyên gia và hệ thống chọn thực đơn phù hợp với bạn. Chỉ chuyên gia phụ trách mới xem được chi tiết."
        minhHoa={<IllusHoSo size={110} />}
      />

      <ThongBao loi={loi} ok={ok} />

      <div className="card">
        <h2>Thông tin cơ bản</h2>
        <div className="grid">
          <div>
            <label>Họ tên</label>
            <input value={form.hoTen} onChange={(e) => setForm({ ...form, hoTen: e.target.value })} />
          </div>
          <div>
            <label>Chiều cao (cm)</label>
            <input type="number" value={form.chieuCao} onChange={(e) => setForm({ ...form, chieuCao: e.target.value })} />
          </div>
          <div>
            <label>Cân nặng (kg)</label>
            <input type="number" value={form.canNang} onChange={(e) => setForm({ ...form, canNang: e.target.value })} />
          </div>
        </div>
      </div>

      <div className="card">
        <h2>Dị ứng thực phẩm</h2>
        <label>Chọn các nhóm bạn bị dị ứng</label>
        <div className="row" style={{ marginBottom: 12 }}>
          {NHOM_DI_UNG.map((n) => (
            <label key={n.ma} className="row" style={{ margin: 0, color: "var(--text)" }}>
              <input
                type="checkbox"
                style={{ width: "auto", margin: 0 }}
                checked={form.diUngNhom.includes(n.ma)}
                onChange={() => bat(n.ma)}
              />
              {n.ten}
            </label>
          ))}
        </div>
        <p className="muted">
          Các món chứa nhóm bạn chọn sẽ được thay bằng món an toàn đã được chuyên gia duyệt. Nếu không có món thay, hệ thống sẽ chuyển sang kế hoạch riêng.
        </p>

        <label>Ghi chú thêm về dị ứng hoặc sức khỏe (nếu có)</label>
        <textarea value={form.diUng} onChange={(e) => setForm({ ...form, diUng: e.target.value })} />
      </div>

      <div className="card">
        <h2>Cam kết</h2>
        <label className="row" style={{ color: "var(--text)", margin: 0 }}>
          <input
            type="checkbox"
            style={{ width: "auto", margin: 0 }}
            checked={form.thuocNhomRuiRoCao}
            onChange={(e) => setForm({ ...form, thuocNhomRuiRoCao: e.target.checked })}
          />
          Tôi đang có bệnh nền, đang mang thai, hoặc từng có rối loạn ăn uống. Tôi hiểu rằng mình cần chuyên gia tư vấn trực tiếp và không tự nhận thực đơn mẫu.
        </label>

        <div style={{ marginTop: 20 }}>
          <button className="btn" disabled={!form.hoTen.trim()} onClick={luu}>Lưu hồ sơ</button>
        </div>
      </div>
    </>
  );
}