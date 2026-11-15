const pad = (n: number) => String(n).padStart(2, "0");

/** Ngày hôm nay theo giờ máy (yyyy-mm-dd), tránh lệch múi giờ như toISOString. */
export function ngayHomNay(): string {
  const d = new Date();
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

/** Ngày thứ mấy (1 đến 7) của kế hoạch, tính theo ngày bắt đầu. */
export function ngayCuaKeHoach(tuNgay: string): number {
  const bd = new Date(tuNgay.slice(0, 10) + "T00:00:00");
  const now = new Date();
  now.setHours(0, 0, 0, 0);
  const diff = Math.floor((now.getTime() - bd.getTime()) / 86400000);
  return ((diff % 7) + 7) % 7 + 1;
}