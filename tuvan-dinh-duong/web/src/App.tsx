import { useEffect, useState } from "react";
import { Link, Navigate, Route, Routes, useLocation, useNavigate } from "react-router-dom";
import type { User } from "oidc-client-ts";
import { userManager, rolesCua } from "./auth";
import { goi } from "./api";
import AppShell, { LaIcon, NavItem } from "./components/AppShell";
import SoanKeHoach from "./pages/SoanKeHoach";
import DuyetKeHoach from "./pages/DuyetKeHoach";
import NguoiDungPage from "./pages/NguoiDung";
import NhatKyPage from "./pages/NhatKy";
import MucTieuPage from "./pages/MucTieu";
import ThucDon from "./pages/ThucDon";
import YeuCauCho from "./pages/YeuCauCho";
import HoSoPage from "./pages/HoSo";
import KeHoachRieng from "./pages/KeHoachRieng";

const NAV_CA_NHAN: NavItem[] = [
  { to: "/", label: "Tổng quan" },
  { to: "/muc-tieu", label: "Mục tiêu" },
  { to: "/nhat-ky", label: "Nhật ký ăn" },
  { to: "/ke-hoach-rieng", label: "Kế hoạch riêng" },
  { to: "/ho-so", label: "Hồ sơ" },
];

const NAV_CHUYEN_GIA: NavItem[] = [
  { to: "/yeu-cau", label: "Yêu cầu chờ" },
  { to: "/soan", label: "Soạn kế hoạch" },
  { to: "/thuc-don", label: "Thực đơn kế hoạch" },
];

const NAV_DUYET: NavItem[] = [{ to: "/duyet", label: "Hàng đợi duyệt" }];

const TIEU_DE: Record<string, string> = {
  "/": "Tổng quan",
  "/muc-tieu": "Mục tiêu",
  "/nhat-ky": "Nhật ký ăn",
  "/ke-hoach-rieng": "Kế hoạch riêng",
  "/ho-so": "Hồ sơ",
  "/yeu-cau": "Yêu cầu chờ",
  "/soan": "Soạn kế hoạch",
  "/thuc-don": "Thực đơn kế hoạch",
  "/duyet": "Hàng đợi duyệt",
};

export default function App() {
  const [user, setUser] = useState<User | null | undefined>(undefined);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    userManager.getUser().then(setUser);
  }, []);

  // Sau mỗi lần đăng nhập: đồng bộ tài khoản khách; khách chưa có hồ sơ thì chuyển tới trang hồ sơ
  useEffect(() => {
    if (!user || user.expired) return;
    const roles = rolesCua(user.access_token);
    const laNhanVien = roles.includes("ChuyenGiaSoan") || roles.includes("ChuyenGiaDuyet");
    goi<{ coHoSo: boolean }>("/api/dang-ky/dong-bo", { method: "POST" })
      .then((r) => {
        if (!laNhanVien && !r.coHoSo && window.location.pathname === "/") {
          navigate("/ho-so", { replace: true });
        }
      })
      .catch(() => {});
  }, [user, navigate]);

  // Trang nhận kết quả đăng nhập từ Keycloak
  if (location.pathname === "/callback") {
    return (
      <Callback
        onDone={(u) => {
          setUser(u);
          navigate("/", { replace: true });
        }}
      />
    );
  }

  if (user === undefined) {
    return (
      <div className="login">
        <p className="muted">Đang tải...</p>
      </div>
    );
  }

  if (!user || user.expired) {
    return (
      <div className="login">
        <div className="login-card">
          <div className="brand brand-center">
            <div className="brand-mark">
              <LaIcon size={24} />
            </div>
            <div style={{ textAlign: "left" }}>
              <div className="brand-name">Dinh dưỡng</div>
              <div className="brand-sub">đồng hành</div>
            </div>
          </div>
          <h1>Đăng nhập</h1>
          <p className="muted">
            Tư vấn dinh dưỡng có chuyên gia duyệt. Thực đơn cá nhân luôn do chuyên gia xác nhận.
          </p>
          <button className="btn btn-block" onClick={() => userManager.signinRedirect()}>
            Đăng nhập
          </button>
          <p className="muted small" style={{ marginTop: 16 }}>
            Chưa có tài khoản? Bấm Đăng nhập, rồi chọn Register.
          </p>
        </div>
      </div>
    );
  }

  const roles = rolesCua(user.access_token);
  const laChuyenGiaSoan = roles.includes("ChuyenGiaSoan");
  const laChuyenGiaDuyet = roles.includes("ChuyenGiaDuyet");
  const laNhanVien = laChuyenGiaSoan || laChuyenGiaDuyet;
  const ten = user.profile.name || user.profile.preferred_username || "bạn";

  const nav: NavItem[] = [
    ...(laNhanVien ? [] : NAV_CA_NHAN),
    ...(laChuyenGiaSoan ? NAV_CHUYEN_GIA : []),
    ...(laChuyenGiaDuyet ? NAV_DUYET : []),
  ];

  return (
    <AppShell
      khongGian={laNhanVien ? "Không gian chuyên gia" : "Không gian cá nhân"}
      ten={ten}
      nav={nav}
      tieuDe={TIEU_DE[location.pathname] ?? "Tổng quan"}
      onDangXuat={() => userManager.signoutRedirect()}
    >
      <Routes>
        <Route
          path="/"
          element={
            laNhanVien ? (
              <Navigate to={laChuyenGiaSoan ? "/soan" : "/duyet"} replace />
            ) : (
              <NguoiDungPage ten={ten} />
            )
          }
        />
        <Route path="/nhat-ky" element={laNhanVien ? <Navigate to="/" replace /> : <NhatKyPage />} />
        <Route path="/ho-so" element={laNhanVien ? <Navigate to="/" replace /> : <HoSoPage />} />
        <Route path="/muc-tieu" element={laNhanVien ? <Navigate to="/" replace /> : <MucTieuPage />} />
        <Route path="/ke-hoach-rieng" element={laNhanVien ? <Navigate to="/" replace /> : <KeHoachRieng />} />
        <Route path="/yeu-cau" element={laChuyenGiaSoan ? <YeuCauCho /> : <Navigate to="/" replace />} />
        <Route path="/soan" element={laChuyenGiaSoan ? <SoanKeHoach /> : <Navigate to="/" replace />} />
        <Route path="/thuc-don" element={laChuyenGiaSoan ? <ThucDon /> : <Navigate to="/" replace />} />
        <Route path="/duyet" element={laChuyenGiaDuyet ? <DuyetKeHoach /> : <Navigate to="/" replace />} />
      </Routes>
    </AppShell>
  );
}

function Callback({ onDone }: { onDone: (u: User) => void }) {
  const [loi, setLoi] = useState("");

  useEffect(() => {
    userManager
      .signinRedirectCallback()
      .then((u) => onDone(u))
      .catch((e) => setLoi(String(e)));
    // Chỉ chạy một lần khi trang callback mở
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="login">
      <div className="login-card">
        {loi ? (
          <>
            <div className="msg-err">{loi}</div>
            <Link to="/">Quay lại</Link>
          </>
        ) : (
          <p className="muted">Đang xác thực...</p>
        )}
      </div>
    </div>
  );
}