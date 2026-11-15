import type { ReactNode } from "react";

export default function PageHead({
  nhan,
  tieuDe,
  moTa,
  hanhDong,
  minhHoa,
}: {
  nhan?: string;
  tieuDe: string;
  moTa?: string;
  hanhDong?: ReactNode;
  minhHoa?: ReactNode;
}) {
  return (
    <div className="page-head">
      <div style={{ flex: 1, minWidth: 0 }}>
        {nhan && <div className="eyebrow">{nhan}</div>}
        <h1>{tieuDe}</h1>
        {moTa && <p className="page-sub">{moTa}</p>}
        {hanhDong && <div style={{ marginTop: 16 }}>{hanhDong}</div>}
      </div>
      {minhHoa && <div className="page-art">{minhHoa}</div>}
    </div>
  );
}