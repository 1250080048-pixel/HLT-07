import { useState } from "react";

export type MonTD = {
  ngayThu: number;
  buaAn: string;
  maMonAn: string;
  tenMon: string;
  khauPhan: string;
  nhomThucPham: string;
  nangLuong: number | null;
  tenMonGoc?: string | null;
};

const TEN_BUA: Record<string, string> = {
  Sang: "Bữa sáng",
  Trua: "Bữa trưa",
  Toi: "Bữa tối",
  Phu: "Bữa phụ",
};

export default function ThucDonView({
  mon,
  onXoa,
  ngayChon,
  onDoiNgay,
  ngayMacDinh,
}: {
  mon: MonTD[];
  onXoa?: (ngay: number, bua: string, maMon: string) => void;
  ngayChon?: number;
  onDoiNgay?: (ngay: number) => void;
  ngayMacDinh?: number;
}) {
  const [noiBo, setNoiBo] = useState(ngayMacDinh ?? 1);
  const ngay = ngayChon ?? noiBo;

  const chonNgay = (n: number) => {
    setNoiBo(n);
    onDoiNgay?.(n);
  };

  const ngayDuocChon = mon.filter((m) => m.ngayThu === ngay);

  return (
    <>
      <div className="row" style={{ marginBottom: 12 }}>
        {[1, 2, 3, 4, 5, 6, 7].map((n) => (
          <button
            key={n}
            className={n === ngay ? "btn" : "btn secondary"}
            onClick={() => chonNgay(n)}
          >
            Ngày {n}
          </button>
        ))}
      </div>

      {Object.keys(TEN_BUA).map((bua) => {
        const ds = ngayDuocChon.filter((m) => m.buaAn === bua);
        return (
          <div key={bua} style={{ marginBottom: 12 }}>
            <strong>{TEN_BUA[bua]}</strong>
            {ds.length === 0 ? (
              <div className="muted">Chưa có món.</div>
            ) : (
              <ul>
                {ds.map((m) => (
                  <li key={m.maMonAn}>
                    {m.tenMon} — {m.khauPhan}
                    {m.nangLuong != null && (
                      <span className="muted"> (ước lượng {m.nangLuong} kcal)</span>
                    )}
                    {m.tenMonGoc && (
                      <span className="badge info" style={{ marginLeft: 8 }}>
                        thay cho {m.tenMonGoc} (dị ứng)
                      </span>
                    )}
                    {onXoa && (
                      <button
                        className="btn danger"
                        style={{ marginLeft: 8, padding: "2px 8px" }}
                        onClick={() => onXoa(m.ngayThu, m.buaAn, m.maMonAn)}
                      >
                        Xóa
                      </button>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </div>
        );
      })}
    </>
  );
}