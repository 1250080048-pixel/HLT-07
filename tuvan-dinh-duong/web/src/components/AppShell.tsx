import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";

export type NavItem = { to: string; label: string };

export function LaIcon({ size = 22 }: { size?: number }) {
  return (
    <svg
      viewBox="0 0 24 24"
      width={size}
      height={size}
      fill="none"
      stroke="currentColor"
      strokeWidth={1.8}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M5 19c0-8 5-13 14-14-1 9-6 14-14 14z" />
      <path d="M5 19l7-7" />
    </svg>
  );
}

export default function AppShell({
  khongGian,
  ten,
  nav,
  tieuDe,
  onDangXuat,
  children,
}: {
  khongGian: string;
  ten: string;
  nav: NavItem[];
  tieuDe: string;
  onDangXuat: () => void;
  children: ReactNode;
}) {
  const chu =
    ten
      .trim()
      .split(/\s+/)
      .slice(-2)
      .map((s) => s[0]?.toUpperCase() ?? "")
      .join("") || "KH";

  return (
    <div className="app">
      <aside className="side">
        <div className="brand">
          <div className="brand-mark">
            <LaIcon />
          </div>
          <div>
            <div className="brand-name">Dinh dưỡng</div>
            <div className="brand-sub">đồng hành</div>
          </div>
        </div>

        <div className="side-label">{khongGian}</div>
        <nav className="nav">
          {nav.map((n) => (
            <NavLink key={n.to} to={n.to} end={n.to === "/"}>
              {n.label}
            </NavLink>
          ))}
        </nav>

        <div className="side-foot">
          <div className="avatar">{chu}</div>
          <div className="who">
            <b>{ten}</b>
            <span>Tài khoản của bạn</span>
          </div>
          <button className="linkbtn" onClick={onDangXuat}>
            Đăng xuất
          </button>
        </div>
      </aside>

      <div className="main">
        <header className="topbar">
          <div className="crumb">
            {khongGian}
            <span>›</span>
            <b>{tieuDe}</b>
          </div>
        </header>

        <main className="content">{children}</main>
      </div>
    </div>
  );
}