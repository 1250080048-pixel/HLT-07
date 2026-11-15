export function ThongBao({ loi, ok }: { loi?: string; ok?: string }) {
  if (loi) return <div className="msg-err">{loi}</div>;
  if (ok) return <div className="msg-ok">{ok}</div>;
  return null;
}
