import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { goi, ApiError } from "../api";
import { ThongBao } from "../components/Feedback";
import PageHead from "../components/PageHead";
import { IllusLa } from "../components/Illustrations";
import ThucDonView, { MonTD } from "../components/ThucDonView";

type MucTieu = { maMucTieu: string; tenHienThi: string; moTa: string | null };
type KeHoachMT = { maKeHoach: string; tuNgay: string; denNgay: string; trangThai: string; maMucTieu: string };
type CuaToi = { plan: KeHoachMT | null; mon: MonTD[]; coKeHoachDangXuLy: boolean };
type HoSo = { thuocNhomRuiRoCao: boolean } | null;
type KetQua = { loai: "ApDung" | "YeuCauRieng"; maKeHoach: string | null; maYeuCau: string | null; thongBao: string };

const TEN_TRANG_THAI: Record<string, string> = {
  Nhap: "Đang soạn",
  ChoDuyet: "Đang chờ chuyên gia xác nhận",
  DaDuyet: "Đã duyệt",
  DangApDung: "Đang áp dụng",
  BiTraLai: "Bị trả lại",
  KetThuc: "Đã kết thúc",
};

export default function MucTieuPage() {
  const [ds, setDs] = useState<MucTieu[]>([]);
  const [chon, setChon] = useState<MucTieu | null>(null);
  const [mau, setMau] = useState<MonTD[]>([]);
  const [cuaToi, setCuaToi] = useState<CuaToi | null>(null);
  const [hoSo, setHoSo] = useState<HoSo>(null);
  const [daTaiHoSo, setDaTaiHoSo] = useState(false);
  const [loi, setLoi] = useState("");
  const [ok, setOk] = useState("");

  const taiCuaToi = async () => setCuaToi(await goi<CuaToi>("/api/muc-tieu/cua-toi"));

  useEffect(() => {
    goi<MucTieu[]>("/api/muc-tieu").then(setDs).catch((e) => setLoi(e.message));
    taiCuaToi().catch((e) => setLoi(e.message));
    goi<HoSo>("/api/ho-so")
      .then((h) => setHoSo(h && "thuocNhomRuiRoCao" in h ? h : null))
      .catch(() => setHoSo(null))
      .finally(() => setDaTaiHoSo(true));
  }, []);

  const xem = async (m: MucTieu) => {
    setLoi(""); setOk(""); setChon(m);
    try {
      setMau(await goi<MonTD[]>(`/api/muc-tieu/${m.maMucTieu}/thuc-don`));
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  const batDau = async () => {
    if (!chon) return;
    setLoi(""); setOk("");
    try {
      const kq = await goi<KetQua>(`/api/muc-tieu/${chon.maMucTieu}/chon`, { method: "POST" });
      setOk(kq.thongBao);
      await taiCuaToi();
    } catch (e) {
      setLoi(e instanceof ApiError ? e.message : String(e));
    }
  };

  const nhomRuiRo = hoSo?.thuocNhomRuiRoCao === true;

  return (
    <>
      <PageHead
        nhan="Định hướng của bạn"
        tieuDe="Mục tiêu dinh dưỡng"
        moTa="Mục tiêu do bạn tự đặt và theo dõi. Không có gợi ý điều trị tự động."
        minhHoa={<IllusLa size={110} />}
      />

      <ThongBao loi={loi} ok={ok} />

      {daTaiHoSo && !hoSo && (
        <div className="card">
          <h2>Cần khai báo hồ sơ</h2>
          <p className="muted">
            Bạn cần khai báo hồ sơ trước khi chọn mục tiêu, để hệ thống xác định bạn có cần tư vấn riêng hay không.
          </p>
          <Link to="/ho-so" className="btn">Khai báo hồ sơ</Link>
        </div>
      )}

      {cuaToi?.plan && (
        <div className="card">
          <h2>Kế hoạch của bạn</h2>
          <p className="muted">
            Từ {cuaToi.plan.tuNgay.slice(0, 10)} đến {cuaToi.plan.denNgay.slice(0, 10)} ·{" "}
            <span className={`badge ${cuaToi.plan.trangThai === "DangApDung" ? "ok" : "warn"}`}>
              {TEN_TRANG_THAI[cuaToi.plan.trangThai] ?? cuaToi.plan.trangThai}
            </span>
          </p>
          <ThucDonView mon={cuaToi.mon} />
        </div>
      )}

      <div className="card">
        <h2>Chọn mục tiêu</h2>
        <div className="row">
          {ds.map((m) => (
            <button
              key={m.maMucTieu}
              className={chon?.maMucTieu === m.maMucTieu ? "btn" : "btn secondary"}
              onClick={() => xem(m)}
            >
              {m.tenHienThi}
            </button>
          ))}
        </div>
      </div>

      {chon && (
        <div className="card">
          <h2>{chon.tenHienThi}</h2>
          <p className="muted">{chon.moTa}</p>

          {nhomRuiRo ? (
            <p className="msg-err">
              Hồ sơ của bạn thuộc nhóm cần tư vấn riêng. Hệ thống không áp dụng thực đơn chung cho bạn;
              bấm nút bên dưới để gửi yêu cầu kế hoạch riêng tới chuyên gia.
            </p>
          ) : (
            <p className="muted">
              Bấm áp dụng để dùng ngay thực đơn chung đã được chuyên gia duyệt. Thực đơn không được chỉnh
              theo từng người. Nếu bạn có bệnh nền, đang mang thai, hoặc từng có rối loạn ăn uống, hãy khai báo
              trong hồ sơ để chuyển sang kế hoạch riêng.
            </p>
          )}

          <ThucDonView mon={mau} />

          <button
            className="btn"
            disabled={!hoSo || !!cuaToi?.coKeHoachDangXuLy}
            onClick={batDau}
          >
            {nhomRuiRo ? "Gửi yêu cầu kế hoạch riêng" : "Áp dụng kế hoạch theo mục tiêu này"}
          </button>

          {cuaToi?.coKeHoachDangXuLy && (
            <p className="muted">Bạn đang có kế hoạch chưa kết thúc nên chưa thể bắt đầu mục tiêu mới.</p>
          )}
        </div>
      )}
    </>
  );
}