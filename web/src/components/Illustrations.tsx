/** Bát thức ăn và lá rau, dùng cho trang Tổng quan và Nhật ký. */
export function IllusBat({ size = 120 }: { size?: number }) {
  return (
    <svg viewBox="0 0 120 120" width={size} height={size} aria-hidden="true">
      <circle cx="60" cy="60" r="58" fill="#e4f0e7" />
      <ellipse cx="60" cy="78" rx="38" ry="12" fill="#d9e8dc" />
      <path d="M22 70 h76 a38 30 0 0 1 -76 0 z" fill="#ffffff" stroke="#2e6b43" strokeWidth="2" />
      <path d="M40 60 q6 -10 0 -18" stroke="#9bbfa4" strokeWidth="2" fill="none" strokeLinecap="round" />
      <path d="M60 58 q6 -10 0 -18" stroke="#9bbfa4" strokeWidth="2" fill="none" strokeLinecap="round" />
      <path d="M80 60 q6 -10 0 -18" stroke="#9bbfa4" strokeWidth="2" fill="none" strokeLinecap="round" />
      <path d="M48 62 c-2 -10 6 -18 14 -22 c-2 10 -8 18 -14 22 z" fill="#2e6b43" />
      <path d="M62 60 c4 -10 14 -14 22 -12 c-2 9 -10 14 -22 12 z" fill="#4a8a5c" />
      <circle cx="40" cy="56" r="4" fill="#c97a3a" />
      <circle cx="78" cy="52" r="3" fill="#e0b050" />
    </svg>
  );
}

/** Lá rau và trái cây, dùng cho các trang mục tiêu. */
export function IllusLa({ size = 120 }: { size?: number }) {
  return (
    <svg viewBox="0 0 120 120" width={size} height={size} aria-hidden="true">
      <circle cx="60" cy="60" r="58" fill="#f6ecd8" />
      <path d="M30 88 C30 50 60 30 92 32 C90 64 64 90 30 88 Z" fill="#2e6b43" />
      <path d="M32 86 C50 66 66 52 86 38" stroke="#e4f0e7" strokeWidth="2" fill="none" strokeLinecap="round" />
      <path d="M40 92 C52 80 66 70 78 66" stroke="#9bbfa4" strokeWidth="1.5" fill="none" strokeLinecap="round" />
      <circle cx="82" cy="78" r="12" fill="#e07a3f" />
      <path d="M82 66 q2 -4 6 -4" stroke="#4a8a5c" strokeWidth="2" fill="none" strokeLinecap="round" />
    </svg>
  );
}

/** Nhịp tim và thước đo, dùng cho trang Hồ sơ và Calo. */
export function IllusHoSo({ size = 120 }: { size?: number }) {
  return (
    <svg viewBox="0 0 120 120" width={size} height={size} aria-hidden="true">
      <circle cx="60" cy="60" r="58" fill="#e8eef6" />
      <rect x="32" y="26" width="56" height="70" rx="10" fill="#ffffff" stroke="#214e7a" strokeWidth="2" />
      <path d="M42 52 h12 l4 -8 l6 16 l4 -8 h10" stroke="#2e6b43" strokeWidth="2.5" fill="none" strokeLinecap="round" strokeLinejoin="round" />
      <rect x="42" y="72" width="36" height="5" rx="2.5" fill="#d9e3ee" />
      <rect x="42" y="82" width="22" height="5" rx="2.5" fill="#d9e3ee" />
    </svg>
  );
}

/** Mảnh ghép trạng thái rỗng, dùng khi chưa có dữ liệu. */
export function IllusRong({ size = 96 }: { size?: number }) {
  return (
    <svg viewBox="0 0 96 96" width={size} height={size} aria-hidden="true">
      <circle cx="48" cy="48" r="46" fill="#f1f7f2" />
      <rect x="28" y="30" width="40" height="36" rx="6" fill="#ffffff" stroke="#9bbfa4" strokeWidth="2" />
      <path d="M36 44 h24 M36 52 h16" stroke="#9bbfa4" strokeWidth="2" strokeLinecap="round" />
    </svg>
  );
}