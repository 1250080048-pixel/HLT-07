import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { IllusBat } from "../components/Illustrations";
import { goi } from "../api";
import { ThongBao } from "../components/Feedback";
import PageHead from "../components/PageHead";
import ThucDonView, { MonTD } from "../components/ThucDonView";
import CaloCard from "../components/CaloCard";
import { ngayCuaKeHoach, ngayHomNay } from "../lib/ngay";

type KeHoachDang = {
  plan: { maKeHoach: string; tuNgay: string; denNgay: string; trangThai: string } | null;
  mon: MonTD[];
};
type TienDo = { maKeHoach: string; tongNgay: number; soNgayDaGhi: number; tyLePhanTram: number };
type CaloRes = { anSo: boolean; ngay: { ngay: string; tongKcal: number }[] };

function loiChao(): string {
  const h = new Date().getHours();
  if (h < 11) return "buổi sáng";
  if (h < 14) return "buổi trưa";
  if (h < 18) return "buổi chiều";
  return "buổi tối";
}

export default function NguoiDungPage({ ten }: { ten: string }) {
  const [kh, setKh] = useState<KeHoachDang | null>(null);
  const [tienDo, setTienDo] = useState<TienDo[]>([]);
  const [calo, setCalo] = useState<CaloRes | null>(null);
  const [loi, setLoi] = useState("");

  useEffect(() => {
    Promise.all([
      goi<KeHoachDang>("/api/ke-hoach/dang-ap-dung"),
      goi<TienDo[]>("/api/tien-do"),
      goi<CaloRes>("/api/calo"),
    ])
      .then(([a, b, c]) => {
        setKh(a);
        setTienDo(b);
        setCalo(c);
      })
      .catch((e) => setLoi(e.message));
  }, []);

  const homNay = ngayHomNay();
  const ngayHienTai = kh?.plan ? ngayCuaKeHoach(kh.plan.tuNgay) : 1;
  const caloHomNay =
    calo && !calo.anSo
      ? calo.ngay.find((n) => n.ngay.slice(0, 10) === homNay)?.tongKcal ?? 0
      : null;
  const tuan = tienDo[0];

  return (
    <>
      <PageHead
        nhan="Hôm nay · Nhịp sống của bạn"
        tieuDe={`Chào ${loiChao()}, ${ten}`}
        moTa="Một ngày mới để ghi lại những lựa chọn của bạn."
        hanhDong={
          <Link to="/nhat-ky" className="btn">
            + Ghi bữa ăn
          </Link>
        }
        minhHoa={<IllusBat size={110} />}
      />

      <ThongBao loi={loi} />

      <div className="stats">
        <div className="stat">
          <div className="stat-label">Kế hoạch đang áp dụng</div>
          <div className="stat-val">{kh?.plan ? "Đang áp dụng" : "Chưa có"}</div>
          <div className="stat-note">
            {kh?.plan ? `Hôm nay là ngày ${ngayHienTai} / 7` : "Chọn mục tiêu để bắt đầu"}
          </div>
        </div>

        <div className="stat">
          <div className="stat-label">Năng lượng hôm nay</div>
          <div className="stat-val">
            {caloHomNay === null ? "—" : Math.round(caloHomNay)}
            <small>kcal</small>
          </div>
          <div className="stat-note">
            {caloHomNay === null ? "Số liệu được ẩn theo hồ sơ" : "Ước tính từ các món đã ghi"}
          </div>
        </div>

        <div className="stat">
          <div className="stat-label">Ngày đã ghi</div>
          <div className="stat-val">
            {tuan?.soNgayDaGhi ?? 0}
            <small>/ {tuan?.tongNgay ?? 7} ngày</small>
          </div>
          <div className="stat-note">Theo kế hoạch đang áp dụng</div>
        </div>
      </div>

      <div className="card">
        <h2>Kế hoạch đang áp dụng</h2>
        {kh?.plan ? (
          <>
            <p className="muted">
              Từ {kh.plan.tuNgay.slice(0, 10)} đến {kh.plan.denNgay.slice(0, 10)}
            </p>
            <ThucDonView mon={kh.mon} ngayMacDinh={ngayHienTai} />
          </>
        ) : (
          <p className="muted">
            Chưa có kế hoạch cá nhân. Xem <Link to="/muc-tieu">các mục tiêu</Link> để bắt đầu.
          </p>
        )}
      </div>

      <CaloCard />
    </>
  );
}