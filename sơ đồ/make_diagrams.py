"""Tạo toàn bộ sơ đồ HLT-07 dưới dạng file draw.io.
Chạy: python3 make_diagrams.py  -> tạo HLT-07_so_do.drawio (nhiều trang) và các file lẻ."""
import xml.etree.ElementTree as ET
from drawio_lib import Diagram, save

ACTOR = "shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;"
UC = "ellipse;whiteSpace=wrap;html=1;"
BOX = "rounded=0;whiteSpace=wrap;html=1;"
LINE = "endArrow=none;html=1;rounded=0;"
ASSOC = "endArrow=none;html=1;rounded=0;"
DASH = "endArrow=open;endFill=0;dashed=1;html=1;rounded=0;"
SYNC = "endArrow=block;endFill=1;html=1;rounded=0;"

pages = []

# ---------- 1. Use case tổng quát ----------
d = Diagram("1. Use case tổng quát")
d.v("sys", "", 260, 40, 620, 720, "rounded=0;whiteSpace=wrap;html=1;fillColor=none;")
d.v("sysname", "HỆ THỐNG TƯ VẤN DINH DƯỠNG", 300, 50, 540, 30, "text;html=1;align=center;fontStyle=1;")
ucs = [
 ("uc01","UC01 Đăng nhập",300,100),("uc02","UC02 Quản lý tài khoản",560,100),
 ("uc03","UC03 Quản lý hồ sơ",300,170),("uc04","UC04 Ghi nhật ký ăn uống",560,170),
 ("uc05","UC05 Soạn kế hoạch",300,240),("uc06","UC06 Duyệt kế hoạch",560,240),
 ("uc07","UC07 Áp dụng & theo dõi",300,310),("uc08","UC08 Quản lý nguồn",560,310),
 ("uc09","UC09 Xem lịch sử hiệu chỉnh",300,380),("uc10","UC10 Duyệt mẫu trong thư viện",560,380),
 ("uc11","UC11 Chọn mẫu & tạo bản nháp",300,450),("uc12","UC12 Sinh mẫu nháp tự động",560,450),
]
for k,l,x,y in ucs:
    d.v(k, l, x, y, 220, 55, UC)
actors = [("a_nd","Người dùng",40,120),("a_cgs","Chuyên gia\ndinh dưỡng",40,300),
          ("a_cgd","Chuyên gia\nduyệt",40,480),("a_qtv","Quản trị viên",40,640),
          ("a_hs","Hệ thống\n(tự động)",960,450),("a_oidc","OIDC Provider",960,120)]
for k,l,x,y in actors:
    d.v(k, l, x, y, 60, 80, ACTOR)
links = [
 ("a_nd","uc01"),("a_nd","uc03"),("a_nd","uc04"),("a_nd","uc07"),
 ("a_cgs","uc01"),("a_cgs","uc03"),("a_cgs","uc05"),("a_cgs","uc07"),
 ("a_cgd","uc01"),("a_cgd","uc06"),("a_cgd","uc10"),
 ("a_qtv","uc01"),("a_qtv","uc02"),("a_qtv","uc08"),("a_qtv","uc09"),
 ("a_hs","uc11"),("a_hs","uc12"),("a_hs","uc07"),("a_oidc","uc01"),
]
for i,(a,u) in enumerate(links):
    d.e(f"l{i}", a, u, "", ASSOC)
d.e("inc1","uc05","uc03","<<include>>",DASH)
d.e("inc2","uc06","uc01","<<include>>",DASH)
d.e("ext1","uc07","uc09","<<extend>>",DASH)
pages.append(d)

# ---------- 2. Sơ đồ lớp mức phân tích ----------
d = Diagram("2. Sơ đồ lớp phân tích")
def cls(key, name, attrs, x, y, w=230):
    h = 34 + 16 * len(attrs)
    body = "\n".join(attrs)
    d.v(key, f"<b>{name}</b>\n────────\n{body}", x, y, w, h,
        "rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacingLeft=6;")
cls("nd","NguoiDung",["- maNguoiDung","- hoTen","- ngaySinh"],40,60)
cls("cg","ChuyenGia",["- maChuyenGia","- vaiTro: Soan | Duyet"],40,240)
cls("hs","HoSoDinhDuong",["- chieuCao","- canNang","- diUng","- thuocNhomRuiRoCao"],40,380)
cls("kh","KeHoachDinhDuong",["- maKeHoach","- phienBan","- trangThai","- tuNgay, denNgay"],380,60)
cls("kn","KhuyenNghi",["- noiDung","- thuTu"],380,300)
cls("pd","PhieuDuyet",["- ketQua","- lyDo","- thoiDiem"],700,60)
cls("na","NhatKyAnUong",["- ngayAn","- buaAn","- khoiLuong"],700,280)
cls("ma","MonAn",["- tenMon","- nangLuong"],1000,280)
cls("ng","NguonThamKhao",["- tenNguon","- donViCongBo","- namCongBo","- dangHoatDong"],1000,60)
cls("nk","NhatKyHieuChinh",["- tenBang, maBanGhi","- hanhDong","- giaTriCu, giaTriMoi"],380,480)
cls("mk","MauKeHoach",["- nhomDoiTuong","- mucTieu","- trangThai","- phienBan"],700,480)
d.e("r1","nd","hs","1 — 1",LINE)
d.e("r2","nd","kh","1 — n",LINE)
d.e("r3","kh","kn","1 — n",LINE)
d.e("r4","kh","pd","1 — n",LINE)
d.e("r5","kn","ng","n — n",LINE)
d.e("r6","nd","na","1 — n",LINE)
d.e("r7","na","ma","n — 1",LINE)
d.e("r8","mk","kh","1 — n (tạo bản nháp)",DASH)
d.e("r9","cg","kh","soạn / duyệt",LINE)
pages.append(d)

# ---------- 3. ERD ----------
d = Diagram("3. ERD cơ sở dữ liệu")
def tbl(key, name, cols, x, y, w=250):
    h = 30 + 15 * len(cols)
    body = "\n".join(cols)
    d.v(key, f"<b>{name}</b>\n{body}", x, y, w, h,
        "rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacingLeft=6;fontSize=11;")
tbl("t_nd","nguoi_dung",["PK ma_nguoi_dung","ho_ten","ngay_sinh"],40,60)
tbl("t_hs","ho_so_dinh_duong",["PK ma_ho_so","FK ma_nguoi_dung","chieu_cao","can_nang","thuoc_nhom_rui_ro_cao"],40,240)
tbl("t_kh","ke_hoach_dinh_duong",["PK ma_ke_hoach","FK ma_nguoi_dung","FK ma_nguoi_soan","phien_ban","trang_thai","tu_ngay","den_ngay"],380,60)
tbl("t_kn","khuyen_nghi",["PK ma_khuyen_nghi","FK ma_ke_hoach","noi_dung","thu_tu"],380,300)
tbl("t_kns","khuyen_nghi_nguon",["PK,FK ma_khuyen_nghi","PK,FK ma_nguon"],380,440,250)
tbl("t_ng","nguon_tham_khao",["PK ma_nguon","ten_nguon","don_vi_cong_bo","nam_cong_bo","dang_hoat_dong"],720,300)
tbl("t_pd","phieu_duyet",["PK ma_phieu","FK ma_ke_hoach","FK ma_nguoi_duyet","ket_qua","ly_do","thoi_diem"],720,60)
tbl("t_na","nhat_ky_an_uong",["PK ma_nhat_ky","FK ma_nguoi_dung","FK ma_mon_an","ngay_an","bua_an","khoi_luong"],40,460)
tbl("t_ma","mon_an",["PK ma_mon_an","ten_mon","nang_luong"],40,640)
tbl("t_mk","mau_ke_hoach",["PK ma_mau","nhom_doi_tuong","muc_tieu","phien_ban","trang_thai","nguoi_soan","nguoi_duyet"],1000,60)
tbl("t_mkn","mau_khuyen_nghi",["PK,FK ma_mau","PK thu_tu","noi_dung","FK ma_nguon"],1000,300)
tbl("t_tc","tieu_chi_chon_mau",["PK ma_tieu_chi","FK ma_mau","thuoc_tinh","toan_tu","gia_tri"],1000,450)
tbl("t_ht","hang_doi_tu_dong",["PK ma_viec","loai_viec","ma_doi_tuong","uu_tien","trang_thai"],720,500)
tbl("t_nk","nhat_ky_hieu_chinh",["PK ma_nhat_ky","ten_bang","ma_ban_ghi","hanh_dong","gia_tri_cu, gia_tri_moi"],380,640,260)
d.e("f1","t_nd","t_hs","1 — 1",LINE)
d.e("f2","t_nd","t_kh","1 — n",LINE)
d.e("f3","t_kh","t_kn","1 — n",LINE)
d.e("f4","t_kn","t_kns","1 — n",LINE)
d.e("f5","t_ng","t_kns","1 — n",LINE)
d.e("f6","t_kh","t_pd","1 — n",LINE)
d.e("f7","t_nd","t_na","1 — n",LINE)
d.e("f8","t_ma","t_na","1 — n",LINE)
d.e("f9","t_mk","t_mkn","1 — n",LINE)
d.e("f10","t_mk","t_tc","1 — n",LINE)
d.e("f11","t_ng","t_mkn","1 — n",LINE)
pages.append(d)

# ---------- 4. Sơ đồ trạng thái KeHoachDinhDuong ----------
d = Diagram("4. Trạng thái kế hoạch")
d.v("s0","",40,200,24,24,"ellipse;fillColor=#000000;strokeColor=none;")
d.v("nhap","Nháp",120,190,130,50)
d.v("cho","Chờ duyệt",330,190,130,50)
d.v("dduyet","Đã duyệt",540,190,130,50)
d.v("bitl","Bị trả lại",330,320,130,50)
d.v("dap","Đang áp dụng",750,190,140,50)
d.v("kt","Kết thúc",960,190,130,50)
d.v("huy","Hủy",120,320,130,50)
d.v("sf","",1110,200,24,24,"ellipse;fillColor=#000000;strokeColor=#000000;")
d.e("t0","s0","nhap","tạo kế hoạch",SYNC)
d.e("t1","nhap","cho","gửi duyệt (đủ nguồn, BR04)",SYNC)
d.e("t2","cho","dduyet","duyệt (người duyệt khác người soạn, BR02)",SYNC)
d.e("t3","cho","bitl","trả lại kèm lý do",SYNC)
d.e("t4","bitl","nhap","sửa, tạo phiên bản mới",SYNC)
d.e("t5","dduyet","dap","áp dụng (không chồng lấn)",SYNC)
d.e("t6","dap","kt","hết hạn hoặc kết thúc sớm",SYNC)
d.e("t7","nhap","huy","hủy",SYNC)
d.e("t8","bitl","huy","hủy",SYNC)
d.e("t9","kt","sf","",SYNC)
pages.append(d)

# ---------- 5. Tuần tự UC06 Duyệt kế hoạch ----------
d = Diagram("5. Tuần tự UC06 Duyệt kế hoạch")
lifes = [("ex","Chuyên gia duyệt",40),("ui","Giao diện duyệt",240),("ctl","KeHoachController",460),
         ("svc","DuyetKeHoachService",700),("dom","KeHoachDinhDuong",940),("db","Cơ sở dữ liệu",1180)]
for k,l,x in lifes:
    d.v(k+"_h", l, x, 30, 160, 44, "rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;")
    d.v(k+"_l", "", x+78, 74, 4, 560, "shape=line;strokeWidth=1;dashed=1;direction=south;")
msgs = [
 ("ex","ui","1. Chọn kế hoạch và bấm Duyệt"),
 ("ui","ctl","2. PUT /api/ke-hoach/{id}/duyet"),
 ("ctl","svc","3. DuyetAsync(maKeHoach, maNguoiDuyet)"),
 ("svc","dom","4. KiemTraDuocDuyet(maNguoiDuyet)  [BR02]"),
 ("dom","svc","5. Ném lỗi nếu trùng người soạn (nhánh thay thế)"),
 ("svc","db","6. Đọc kế hoạch có khóa lạc quan (phienBan)"),
 ("svc","dom","7. ChuyenSang(DaDuyet)  [BR03]"),
 ("svc","db","8. Ghi phiếu duyệt + nhật ký hiệu chỉnh (1 giao dịch)"),
 ("ctl","ui","9. HTTP 200, trạng thái mới"),
 ("ui","ex","10. Hiển thị kết quả"),
]
y = 130
for i,(a,b,lbl) in enumerate(msgs):
    xa = dict((k,x) for k,_,x in lifes)[a] + 78
    xb = dict((k,x) for k,_,x in lifes)[b] + 78
    d.v(f"p{i}a", "", xa, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
    d.v(f"p{i}b", "", xb, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
    d.e(f"m{i}", f"p{i}a", f"p{i}b", lbl, SYNC if i not in (4,8) else DASH)
    y += 52
pages.append(d)

# ---------- 6. Tuần tự UC11 Chọn mẫu và tạo bản nháp ----------
d = Diagram("6. Tuần tự UC11 Chọn mẫu tự động")
lifes = [("cg","Chuyên gia soạn",40),("svc","KeHoachTaoTuMauService",260),("hs","HoSoDinhDuong",520),
         ("rp","MauKeHoachRepository",760),("kh","KeHoachDinhDuong",1000),("db","Cơ sở dữ liệu",1240)]
for k,l,x in lifes:
    d.v(k+"_h", l, x, 30, 180, 44, "rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;")
    d.v(k+"_l", "", x+88, 74, 4, 600, "shape=line;strokeWidth=1;dashed=1;direction=south;")
msgs = [
 ("cg","svc","1. TaoTuMauAsync(maNguoiDung)"),
 ("svc","hs","2. CoNhomRuiroCao()  [BR08]"),
 ("hs","svc","3. false (nếu true: từ chối, soạn thủ công)"),
 ("svc","rp","4. TimMauDaDuyet(nhom, mucTieu)  [BR06]"),
 ("rp","db","5. SELECT mẫu trạng thái DaDuyetMau"),
 ("svc","svc","6. ChonMauKhopNhat() theo điểm khớp tiêu chí"),
 ("svc","kh","7. TaoBanNhap(mau, hoSo)  trạng thái Nhap"),
 ("kh","db","8. INSERT ke_hoach_dinh_duong"),
 ("svc","cg","9. Trả bản nháp, đưa vào hàng đợi duyệt"),
]
y = 130
xs = dict((k,x) for k,_,x in lifes)
for i,(a,b,lbl) in enumerate(msgs):
    if a == b:
        d.v(f"p{i}a", "", xs[a]+88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
        d.v(f"p{i}b", "", xs[a]+130, y+26, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
        d.e(f"m{i}", f"p{i}a", f"p{i}b", lbl, SYNC)
        y += 60
        continue
    d.v(f"p{i}a", "", xs[a]+88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
    d.v(f"p{i}b", "", xs[b]+88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
    d.e(f"m{i}", f"p{i}a", f"p{i}b", lbl, DASH if i in (2,8) else SYNC)
    y += 52
pages.append(d)

# ---------- 7. Hoạt động: quy trình tự động có chốt chuyên gia ----------
d = Diagram("7. Hoạt động quy trình tự động")
d.v("lane_hs","Hệ thống (tự động)",40,40,700,560,"swimlane;startSize=30;horizontal=0;html=1;")
d.v("lane_cg","Chuyên gia duyệt",40,600,700,220,"swimlane;startSize=30;horizontal=0;html=1;")
P = "lane_hs"
d.v("st","",90,80,24,24,"ellipse;fillColor=#000000;strokeColor=none;",P)
d.v("a1","Nhận yêu cầu tư vấn",60,130,160,50,parent=P)
d.v("dq","Nhóm rủi ro cao?",60,210,160,70,"rhombus;whiteSpace=wrap;html=1;",P)
d.v("a2","Từ chối tự chọn mẫu\n→ soạn thủ công",280,215,170,60,parent=P)
d.v("a3","Chọn mẫu đã duyệt\nkhớp tiêu chí",60,320,160,50,parent=P)
d.v("dm","Có mẫu khớp?",60,400,160,70,"rhombus;whiteSpace=wrap;html=1;",P)
d.v("a4","Thông báo không có mẫu\n→ soạn thủ công",280,410,170,60,parent=P)
d.v("a5","Tạo bản nháp (Nhap)\nđiền số liệu, gắn nguồn",420,320,190,60,parent=P)
d.v("a6","Đưa vào hàng đợi duyệt",420,430,190,50,parent=P)
d.v("a7","Chuyên gia xem bản nháp",420,650,190,50,parent=None)
d.v("dd","Duyệt?",60,660,140,70,"rhombus;whiteSpace=wrap;html=1;")
d.v("a8","Chuyển DaDuyet\nthông báo khách",250,668,180,55)
d.v("a9","Trả lại kèm lý do\nthông báo khách",250,740,180,40)
d.v("a10","Theo dõi tiến độ,\nnhắc ghi nhật ký (tự động)",60,480,180,50,parent=P)
d.v("end","",90,540,24,24,"ellipse;fillColor=#000000;strokeColor=none;",P)
d.e("e1","st","a1",style=SYNC)
d.e("e2","a1","dq",style=SYNC)
d.e("e3","dq","a2","có",SYNC)
d.e("e4","dq","a3","không",SYNC)
d.e("e5","a3","dm",style=SYNC)
d.e("e6","dm","a4","không",SYNC)
d.e("e7","dm","a5","có",SYNC)
d.e("e8","a5","a6",style=SYNC)
d.e("e9","a6","a7","hàng đợi",SYNC)
d.e("e10","a7","dd",style=SYNC)
d.e("e11","dd","a8","duyệt",SYNC)
d.e("e12","dd","a9","trả lại",SYNC)
pages.append(d)

# ---------- 8. Kiến trúc phân tầng ----------
d = Diagram("8. Kiến trúc hệ thống")
layers = [
 ("L1","Tầng giao diện (React + Vite)","Màn hình đăng nhập, hồ sơ, nhật ký, soạn/duyệt kế hoạch, hàng đợi duyệt, thư viện mẫu",40,60),
 ("L2","Tầng API (ASP.NET Core 8)","Controller, policy phân quyền, middleware xác thực, xử lý lỗi thống nhất",40,220),
 ("L3","Tầng Domain (C# thuần)","Entity, trạng thái, quy tắc BR01 đến BR10, dịch vụ điều phối giao dịch",40,380),
 ("L4","Tầng hạ tầng","EF Core, repository, PostgreSQL, nhà cung cấp OIDC (Keycloak)",40,540),
]
for k,t,desc,x,y in layers:
    d.v(k, f"<b>{t}</b>\n{desc}", x, y, 720, 120, "rounded=0;whiteSpace=wrap;html=1;align=center;verticalAlign=middle;fillColor=#f5f5f5;strokeColor=#666666;")
d.v("wk","Worker nền\n(hàng đợi tự động)",840,400,200,80,"rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;")
d.v("oidc","OIDC Provider\n(Keycloak)",840,560,200,70,"rounded=1;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;")
d.e("c1","L1","L2","HTTPS + Bearer token",SYNC)
d.e("c2","L2","L3","gọi dịch vụ",SYNC)
d.e("c3","L3","L4","repository",SYNC)
d.e("c4","wk","L3","xử lý việc",SYNC)
d.e("c5","oidc","L2","xác thực token",DASH)
pages.append(d)


# ---------- PHÂN RÃ USE CASE ----------
TREE = "edgeStyle=orthogonalEdgeStyle;endArrow=none;html=1;rounded=0;"
ROOT = "rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=1;"
CHILD = "rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;"
SUB = "rounded=0;whiteSpace=wrap;html=1;align=left;spacingLeft=6;"

def decomposition(title, root, children, width=230):
    d = Diagram(title)
    n = len(children)
    total = n * (width + 30)
    x0 = max(40, 700 - total // 2)
    d.v("root", root, 540 - 150, 40, 300, 60, ROOT)
    for i, (label, subs) in enumerate(children):
        cx = x0 + i * (width + 30)
        cid = f"c{i}"
        d.v(cid, label, cx, 170, width, 60, CHILD)
        d.e(f"rc{i}", "root", cid, "", TREE)
        for j, s in enumerate(subs):
            sid = f"c{i}s{j}"
            d.v(sid, s, cx + 15, 300 + j * 70, width - 15, 56, SUB)
            d.e(f"cs{i}_{j}", cid, sid, "", TREE)
    return d

pages.append(decomposition("9. Phân rã UC05 Soạn kế hoạch", "UC05 Soạn kế hoạch dinh dưỡng",
 [("5.1 Tạo bản nháp", ["Chọn người dùng phụ trách", "Trạng thái Nháp"]),
  ("5.2 Nhập mục tiêu và khuyến nghị", ["Giai đoạn, mục tiêu năng lượng", "Nội dung từng khuyến nghị"]),
  ("5.3 Gắn nguồn tham khảo", ["Mỗi khuyến nghị ít nhất một nguồn (BR04)", "Chọn từ nguon_tham_khao"]),
  ("5.4 Lưu nháp", ["Sửa tự do khi còn Nháp"]),
  ("5.5 Gửi duyệt", ["Khóa nội dung (BR03)", "Chuyển sang ChoDuyet"])], width=190))

pages.append(decomposition("10. Phân rã UC06 Duyệt kế hoạch", "UC06 Duyệt kế hoạch dinh dưỡng",
 [("6.1 Xem danh sách chờ duyệt", ["Ẩn kế hoạch do chính mình soạn", "Sắp xếp theo thời hạn"]),
  ("6.2 Xem chi tiết và nguồn", ["Khuyến nghị kèm nguồn", "Thông tin người soạn"]),
  ("6.3 Kiểm tra người duyệt", ["Khác người soạn (BR02)", "Trạng thái ChoDuyet"]),
  ("6.4 Ra quyết định", ["Duyệt -> DaDuyet", "Trả lại kèm lý do -> BiTraLai"]),
  ("6.5 Ghi nhận kết quả", ["Ghi phiếu duyệt", "Ghi nhật ký hiệu chỉnh (BR09)"])], width=190))

pages.append(decomposition("11. Phân rã UC07 Áp dụng và theo dõi", "UC07 Áp dụng và theo dõi tiến độ",
 [("7.1 Chọn kế hoạch đã duyệt", ["Trạng thái DaDuyet", "Chọn ngày bắt đầu"]),
  ("7.2 Kiểm tra chồng lấn", ["Không có kế hoạch DangApDung trùng thời gian", "Nếu có: liệt kê"]),
  ("7.3 Kết thúc kế hoạch cũ", ["Chỉ khi có chồng lấn (extend)", "Chuyển sang KetThuc"]),
  ("7.4 Chuyển sang đang áp dụng", ["Ràng buộc chỉ mục duy nhất", "Ghi nhật ký"]),
  ("7.5 Theo dõi tiến độ", ["View vw_tien_do theo tuần", "Nhắc ghi nhật ký tự động"])], width=190))

pages.append(decomposition("12. Phân rã UC10 UC11 UC12 Thư viện mẫu", "Thư viện mẫu kế hoạch",
 [("UC10 Duyệt mẫu", ["Chuyên gia soạn gửi duyệt mẫu", "Chuyên gia khác duyệt hoặc trả lại", "Ngừng dùng khi không còn phù hợp"]),
  ("UC11 Chọn mẫu và tạo nháp", ["Kiểm tra nhóm rủi ro (BR08)", "Lọc mẫu DaDuyetMau (BR06)", "Tạo bản nháp Nhap"]),
  ("UC12 Sinh mẫu nháp tự động", ["Nạp tài liệu nguồn", "Trích xuất khuyến nghị thô", "Loại khuyến nghị không có nguồn (BR10)", "Tạo mẫu NhapMau, chờ duyệt"])], width=280))


# ---------- TRẠNG THÁI PHIẾU DUYỆT VÀ MẪU KẾ HOẠCH ----------
d = Diagram("13. Trạng thái phiếu duyệt")
d.v("s0","",40,200,24,24,"ellipse;fillColor=#000000;strokeColor=none;")
d.v("dx","DangXem\n(người duyệt mở phiếu)",130,180,200,70)
d.v("dd","DaDuyet\n(không sửa được)",440,120,200,70)
d.v("tl","TraLai\n(có lý do, không sửa được)",440,260,220,70)
d.v("sf1","",740,140,24,24,"ellipse;fillColor=#000000;strokeColor=#000000;")
d.v("sf2","",740,280,24,24,"ellipse;fillColor=#000000;strokeColor=#000000;")
d.e("p0","s0","dx","mở phiếu",SYNC)
d.e("p1","dx","dd","duyệt (BR02 đã kiểm tra)",SYNC)
d.e("p2","dx","tl","trả lại kèm lý do",SYNC)
d.e("p3","dd","sf1","",SYNC)
d.e("p4","tl","sf2","",SYNC)
pages.append(d)

d = Diagram("14. Trạng thái mẫu kế hoạch")
d.v("s0","",40,200,24,24,"ellipse;fillColor=#000000;strokeColor=none;")
d.v("nm","NhapMau\n(chuyên gia soạn)",120,180,160,60)
d.v("cd","ChoDuyetMau\n(khóa nội dung)",340,180,170,60)
d.v("dm","DaDuyetMau\n(được dùng để tạo nháp)",570,180,200,60)
d.v("bt","BiTraLaiMau\n(có lý do)",340,320,170,60)
d.v("nd","NgungDung\n(không còn phù hợp)",570,320,200,60)
d.v("sf","",820,200,24,24,"ellipse;fillColor=#000000;strokeColor=#000000;")
d.e("q0","s0","nm","tạo mẫu",SYNC)
d.e("q1","nm","cd","gửi duyệt (đủ nguồn và tiêu chí)",SYNC)
d.e("q2","cd","dm","duyệt (người duyệt khác người soạn, BR07)",SYNC)
d.e("q3","cd","bt","trả lại kèm lý do",SYNC)
d.e("q4","bt","nm","sửa, tạo phiên bản mới",SYNC)
d.e("q5","dm","nd","ngừng dùng",SYNC)
d.e("q6","nd","sf","",SYNC)
pages.append(d)


# ---------- SƠ ĐỒ TUẦN TỰ BỔ SUNG (UC01, UC04, UC05, UC07) ----------
def sequence(title, lifes, msgs, gap=230, step=52):
    d = Diagram(title)
    xs = {}
    for i, (key, label) in enumerate(lifes):
        x = 40 + i * gap
        xs[key] = x
        d.v(key + "_h", label, x, 30, 180, 44, "rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;")
        d.v(key + "_l", "", x + 88, 74, 4, 60 + step * (len(msgs) + 2), "shape=line;strokeWidth=1;dashed=1;direction=south;")
    y = 130
    for i, (a, b, lbl, dashed) in enumerate(msgs):
        if a == "--":                     # dòng chú thích nhánh, không có mũi tên
            d.v(f"n{i}", lbl, 60, y - 10, 560, 22, "text;html=1;align=left;fontStyle=2;")
            y += 26
            continue
        if a == b:
            d.v(f"p{i}a", "", xs[a] + 88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
            d.v(f"p{i}b", "", xs[a] + 130, y + 26, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
            d.e(f"m{i}", f"p{i}a", f"p{i}b", lbl, SYNC)
            y += step + 26
            continue
        d.v(f"p{i}a", "", xs[a] + 88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
        d.v(f"p{i}b", "", xs[b] + 88, y, 2, 2, "ellipse;fillColor=none;strokeColor=none;")
        d.e(f"m{i}", f"p{i}a", f"p{i}b", lbl, DASH if dashed else SYNC)
        y += step
    return d

pages.append(sequence("15. Tuần tự UC01 Đăng nhập",
 [("nd","Người dùng"),("ui","Giao diện React"),("kc","Keycloak (OIDC)"),("api","API")],
 [("nd","ui","1. Bấm Đăng nhập",False),
  ("ui","kc","2. Chuyển hướng /authorize kèm code_challenge (PKCE)",False),
  ("kc","nd","3. Hiển thị form đăng nhập",True),
  ("nd","kc","4. Gửi tên đăng nhập và mật khẩu",False),
  ("kc","ui","5. Redirect kèm authorization code",True),
  ("ui","kc","6. POST /token kèm code_verifier",False),
  ("kc","ui","7. Trả access token và id token",True),
  ("ui","api","8. Gọi API kèm Bearer token",False),
  ("api","ui","9. Trả dữ liệu theo vai trò trong claim",True),
  ("--","--","Nhánh thay thế: sai mật khẩu, Keycloak báo lỗi, không có token",False)]))

pages.append(sequence("16. Tuần tự UC04 Ghi nhật ký ăn uống",
 [("nd","Người dùng"),("ui","Giao diện"),("ctl","NhatKyController"),("svc","NhatKyService"),("dom","NhatKyAnUong"),("db","Cơ sở dữ liệu")],
 [("nd","ui","1. Chọn ngày, bữa, món, khối lượng",False),
  ("ui","ctl","2. POST /api/nhat-ky-an-uong",False),
  ("ctl","svc","3. GhiAsync(maNguoiDung, dto)",False),
  ("svc","db","4. Kiểm tra món có trong mon_an",False),
  ("db","svc","5. Có / không",True),
  ("--","--","Nhánh thay thế: món không có trong danh mục",False),
  ("svc","ctl","6. Lỗi 422 (không tự tạo món mới)",True),
  ("--","--","Nhánh chính: món hợp lệ",False),
  ("svc","dom","7. Tạo NhatKyAnUong(ngay, bua, khoiLuong)",False),
  ("svc","db","8. INSERT nhat_ky_an_uong",False),
  ("ctl","ui","9. HTTP 201",True),
  ("ui","nd","10. Hiển thị bữa ăn trong nhật ký ngày",True)]))

pages.append(sequence("17. Tuần tự UC05 Soạn kế hoạch",
 [("cg","Chuyên gia soạn"),("ui","Giao diện"),("ctl","KeHoachController"),("svc","KeHoachService"),("dom","KeHoachDinhDuong"),("db","Cơ sở dữ liệu")],
 [("cg","ui","1. Tạo kế hoạch nháp cho người dùng",False),
  ("ui","ctl","2. POST /api/ke-hoach",False),
  ("ctl","svc","3. TaoNhapAsync(maNguoiSoan, maNguoiDung)",False),
  ("svc","dom","4. new KeHoachDinhDuong(trangThai = Nhap)",False),
  ("svc","db","5. INSERT ke_hoach_dinh_duong",False),
  ("ctl","ui","6. HTTP 201 và mã kế hoạch",True),
  ("cg","ui","7. Nhập khuyến nghị và gắn nguồn (BR04)",False),
  ("ui","ctl","8. PUT /api/ke-hoach/{id}",False),
  ("ctl","svc","9. CapNhatNhapAsync(maKeHoach, dto)",False),
  ("svc","dom","10. Kiểm tra trạng thái Nhap và mỗi khuyến nghị có nguồn",False),
  ("svc","db","11. UPDATE nội dung và phienBan",False),
  ("cg","ui","12. Bấm gửi duyệt",False),
  ("ui","ctl","13. POST /api/ke-hoach/{id}/gui-duyet",False),
  ("ctl","svc","14. GuiDuyetAsync(maKeHoach)",False),
  ("svc","dom","15. GuiDuyet(): khóa nội dung (BR03)",False),
  ("svc","db","16. UPDATE trang_thai = ChoDuyet",False),
  ("ctl","ui","17. HTTP 200",True)], gap=220))

pages.append(sequence("18. Tuần tự UC07 Áp dụng kế hoạch",
 [("cg","Chuyên gia soạn"),("ui","Giao diện"),("ctl","KeHoachController"),("svc","KeHoachService"),("dom","KeHoachDinhDuong"),("db","Cơ sở dữ liệu")],
 [("cg","ui","1. Chọn kế hoạch đã duyệt và ngày bắt đầu",False),
  ("ui","ctl","2. POST /api/ke-hoach/{id}/ap-dung",False),
  ("ctl","svc","3. ApDungAsync(maKeHoach)",False),
  ("svc","db","4. Tìm kế hoạch DangApDung chồng lấn thời gian",False),
  ("db","svc","5. Danh sách (có thể rỗng)",True),
  ("--","--","Nhánh thay thế: có kế hoạch chồng lấn",False),
  ("svc","dom","6. KetThuc() kế hoạch cũ (extend)",False),
  ("--","--","Nhánh chính: tiếp tục",False),
  ("svc","dom","7. ChuyenSang(DangApDung)",False),
  ("svc","db","8. UPDATE trạng thái và ghi nhật ký (1 giao dịch)",False),
  ("ctl","ui","9. HTTP 200",True),
  ("ui","cg","10. Hiển thị kế hoạch đang áp dụng",True)], gap=220))


# ---------- SƠ ĐỒ HOẠT ĐỘNG CHO TỪNG USE CASE ----------
START = "ellipse;fillColor=#000000;strokeColor=none;"
END = "ellipse;shape=doubleEllipse;fillColor=#000000;strokeColor=#000000;"
ACT = "rounded=1;whiteSpace=wrap;html=1;"
DEC = "rhombus;whiteSpace=wrap;html=1;"

def activity(title, nodes, edges):
    d = Diagram(title)
    for key, label, x, y, kind in nodes:
        if kind == "start":
            d.v(key, "", x, y, 22, 22, START)
        elif kind == "end":
            d.v(key, "", x, y, 26, 26, END)
        elif kind == "dec":
            d.v(key, label, x, y, 150, 70, DEC)
        else:
            d.v(key, label, x, y, 200, 52, ACT)
    for i, (a, b, lbl) in enumerate(edges):
        d.e(f"f{i}", a, b, lbl, SYNC)
    return d

pages.append(activity("19. Hoạt động UC05 Soạn kế hoạch",
 [("s","",240,30,"start"),
  ("n1","Tạo bản nháp (Nhap)",215,80,"act"),
  ("n2","Nhập mục tiêu và khuyến nghị",215,160,"act"),
  ("n3","Gắn nguồn cho từng khuyến nghị",215,240,"act"),
  ("d1","Mọi khuyến nghị\ncó nguồn? (BR04)",190,320,"dec"),
  ("n4","Báo thiếu nguồn,\nkhông cho gửi duyệt",440,330,"act"),
  ("n5","Lưu nháp",215,430,"act"),
  ("d2","Gửi duyệt?",190,510,"dec"),
  ("n6","Tiếp tục soạn",440,520,"act"),
  ("n7","Khóa nội dung,\nchuyển ChoDuyet (BR03)",215,620,"act"),
  ("e","",240,700,"end")],
 [("s","n1",""),("n1","n2",""),("n2","n3",""),("n3","d1",""),
  ("d1","n4","không"),("n4","n3","sửa nguồn"),
  ("d1","n5","có"),("n5","d2",""),
  ("d2","n6","không"),("n6","n2","sửa tiếp"),
  ("d2","n7","có"),("n7","e","")]))

pages.append(activity("20. Hoạt động UC06 Duyệt kế hoạch",
 [("s","",240,30,"start"),
  ("n1","Xem danh sách chờ duyệt",215,80,"act"),
  ("n2","Chọn kế hoạch và xem\nchi tiết, nguồn",215,160,"act"),
  ("d1","Người duyệt\ntrùng người soạn? (BR02)",190,250,"dec"),
  ("n3","Từ chối thao tác,\nghi nhận lần thử",440,260,"act"),
  ("d2","Quyết định?",190,350,"dec"),
  ("n4","Ghi phiếu duyệt\nvà nhật ký (BR09)",440,350,"act"),
  ("n5","Nhập lý do trả lại",215,460,"act"),
  ("n6","Ghi phiếu trả lại\nvà nhật ký (BR09)",440,460,"act"),
  ("n7","Chuyển DaDuyet",440,560,"act"),
  ("n8","Chuyển BiTraLai",215,560,"act"),
  ("e","",240,650,"end"),
  ("e2","",440,650,"end")],
 [("s","n1",""),("n1","n2",""),("n2","d1",""),
  ("d1","n3","có"),("n3","e2",""),
  ("d1","d2","không"),
  ("d2","n4","duyệt"),("n4","n7",""),("n7","e2",""),
  ("d2","n5","trả lại"),("n5","n6",""),("n6","n8",""),("n8","e","")]))

pages.append(activity("21. Hoạt động UC07 Áp dụng và theo dõi",
 [("s","",240,30,"start"),
  ("n1","Chọn kế hoạch DaDuyet\nvà ngày bắt đầu",215,80,"act"),
  ("d1","Có kế hoạch\nchồng lấn?",190,190,"dec"),
  ("n2","Kết thúc kế hoạch cũ\n(KetThuc)",440,200,"act"),
  ("n3","Chuyển DangApDung\n(ràng buộc chỉ mục duy nhất)",215,320,"act"),
  ("n4","Ghi nhật ký hiệu chỉnh",215,400,"act"),
  ("n5","Người dùng ghi nhật ký hằng ngày",215,480,"act"),
  ("n6","Tính tiến độ theo tuần\n(vw_tien_do)",215,560,"act"),
  ("e","",240,650,"end")],
 [("s","n1",""),("n1","d1",""),
  ("d1","n2","có"),("n2","n3",""),
  ("d1","n3","không"),
  ("n3","n4",""),("n4","n5",""),("n5","n6",""),("n6","e","")]))


# ---------- SƠ ĐỒ LỚP THIẾT KẾ THEO CHỨC NĂNG ----------
DEP = "endArrow=open;endFill=0;dashed=1;html=1;rounded=0;"
REAL = "endArrow=block;endFill=0;dashed=1;html=1;rounded=0;"
COMP = "endArrow=diamondThin;endFill=1;html=1;rounded=0;"
ASC = "endArrow=none;html=1;rounded=0;"
LAYER = "rounded=0;whiteSpace=wrap;html=1;fillColor=none;dashed=1;verticalAlign=top;align=left;spacingLeft=6;fontStyle=1;"

def dclass(d, key, name, methods, x, y, w=260, stereo=None, fill="#ffffff"):
    head = f"<<{stereo}>>\n" if stereo else ""
    h = 40 + 16 * len(methods) + (16 if stereo else 0)
    body = "\n".join(methods)
    d.v(key, f"{head}<b>{name}</b>\n────────\n{body}", x, y, w, h,
        f"rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;spacingLeft=6;fillColor={fill};")

def design_page(title):
    d = Diagram(title)
    d.v("L1", "API (Controller)", 20, 20, 300, 760, LAYER)
    d.v("L2", "Application (Service)", 340, 20, 300, 760, LAYER)
    d.v("L3", "Domain (Entity, quy tắc)", 660, 20, 300, 760, LAYER)
    d.v("L4", "Hạ tầng (Repository, EF Core)", 980, 20, 300, 760, LAYER)
    return d

# --- Trang 22: chức năng quản lý kế hoạch (UC05, UC06, UC07) ---
d = design_page("22. Lớp thiết kế: kế hoạch (UC05, UC06, UC07)")
dclass(d,"kc","KeHoachController",["+Tao(dto): IActionResult","+CapNhat(id, dto)","+GuiDuyet(id)","+Duyet(id)","+TraLai(id, lyDo)","+ApDung(id, ngayBatDau)"],40,60,260)
dclass(d,"nc","NhatKyController",["+Ghi(dto)","+DanhSach(tuNgay, denNgay)"],40,400,260)
dclass(d,"ksvc","KeHoachService",["+TaoNhapAsync(maNguoiSoan, maNguoiDung)","+CapNhatNhapAsync(maKeHoach, dto)","+GuiDuyetAsync(maKeHoach)"],370,60,260)
dclass(d,"dsvc","DuyetKeHoachService",["+DuyetAsync(maKeHoach, maNguoiDuyet)","+TraLaiAsync(maKeHoach, maNguoiDuyet, lyDo)"],370,260,260)
dclass(d,"asvc","ApDungKeHoachService",["+ApDungAsync(maKeHoach, ngayBatDau)"],370,400,260)
dclass(d,"kh","KeHoachDinhDuong",["-maKeHoach, maNguoiDung","-maNguoiSoan, phienBan","-trangThai: TrangThaiKeHoach","+KiemTraDuocDuyet(maNguoiDuyet)","+GuiDuyet()","+ChuyenSang(mới)","+KetThuc()","+ThemKhuyenNghi(kn)"],700,60,260)
dclass(d,"kn","KhuyenNghi",["-noiDung, thuTu","+ThemNguon(maNguon)"],700,330,260)
dclass(d,"pd","PhieuDuyet",["+Tao(maKeHoach, maNguoiDuyet, ketQua, lyDo)"],700,470,260)
dclass(d,"bct","BanChuyenTrangThai",["+CoThe(từ, đến): bool"],700,600,260,stereo="static")
dclass(d,"nkhc","NhatKyHieuChinh",["+Tao(tenBang, maBanGhi, hanhDong, nguoi)"],700,700,260)
dclass(d,"ir","<<interface>>\nIKeHoachRepository",["+LayTheoMaAsync(maKeHoach)","+ThemAsync(kh)","+TimChongLanAsync(maNguoiDung, tuNgay, denNgay)"],1020,60,260,fill="#f5f5f5")
dclass(d,"kr","KeHoachRepository",["+LayTheoMaAsync(maKeHoach)","+ThemAsync(kh)","+TimChongLanAsync(...)"],1020,260,260)
dclass(d,"ipr","<<interface>>\nIPhieuDuyetRepository",["+ThemAsync(phieu)"],1020,400,260,fill="#f5f5f5")
dclass(d,"iuow","<<interface>>\nIUnitOfWork",["+BeginAsync()","+CommitAsync()"],1020,520,260,fill="#f5f5f5")
dclass(d,"ef","EfUnitOfWork",["+BeginAsync()","+CommitAsync()","-DbContext: TuVanDbContext"],1020,640,260)
d.e("c1","kc","ksvc","gọi",DEP)
d.e("c2","kc","dsvc","gọi",DEP)
d.e("c3","kc","asvc","gọi",DEP)
d.e("c4","ksvc","kh","thao tác",DEP)
d.e("c5","dsvc","kh","KiemTraDuocDuyet",DEP)
d.e("c6","asvc","kh","ChuyenSang",DEP)
d.e("c7","kh","kn","1 — n",COMP)
d.e("c8","dsvc","pd","tạo phiếu",DEP)
d.e("c9","kh","bct","kiểm tra chuyển",DEP)
d.e("c10","ksvc","ir","dùng",DEP)
d.e("c11","dsvc","ir","dùng",DEP)
d.e("c12","asvc","ir","dùng",DEP)
d.e("c13","kr","ir","realizes",REAL)
d.e("c14","ef","iuow","realizes",REAL)
d.e("c15","dsvc","iuow","giao dịch",DEP)
d.e("c16","dsvc","nkhc","ghi nhật ký",DEP)
pages.append(d)

# --- Trang 23: nhật ký ăn uống và tạo bản nháp từ mẫu (UC04, UC11, UC12) ---
d = design_page("23. Lớp thiết kế: nhật ký và tạo bản nháp (UC04, UC11)")
dclass(d,"nc2","NhatKyController",["+Ghi(dto): IActionResult","+DanhSach(tuNgay, denNgay)"],40,60,260)
dclass(d,"nsvc","NhatKyService",["+GhiAsync(maNguoiDung, dto)","+DanhSachAsync(maNguoiDung, tuNgay, denNgay)"],370,60,260)
dclass(d,"tsvc","KeHoachTaoTuMauService",["+TaoTuMauAsync(maNguoiDung, maMau?)","-ChonMauKhopNhat(danhSach)"],370,300,260)
dclass(d,"sinh","MauSinhTuDongService",["+SinhMauNhapAsync(maNguonList)","-LocKhuyenNghiCoNguon()"],370,540,260)
dclass(d,"nk","NhatKyAnUong",["-maNguoiDung, ngayAn","-buaAn, khoiLuong","+Tao(...)"],700,60,260)
dclass(d,"mon","MonAn",["-maMonAn, tenMon","-nangLuong"],700,240,260)
dclass(d,"ho","HoSoDinhDuong",["-chieuCao, canNang","-thuocNhomRuiRoCao: bool","+CoNhomRuiroCao(): bool"],700,380,260)
dclass(d,"mau","MauKeHoach",["-nhomDoiTuong, mucTieu","-trangThai: TrangThaiMau","+DaDuyet(): bool"],700,540,260)
dclass(d,"irn","<<interface>>\nINhatKyRepository",["+ThemAsync(nk)","+KiemTraMonAsync(maMonAn)"],1020,60,260,fill="#f5f5f5")
dclass(d,"rn","NhatKyRepository",["+ThemAsync(nk)","+KiemTraMonAsync(maMonAn)"],1020,220,260)
dclass(d,"irm","<<interface>>\nIMauKeHoachRepository",["+TimMauDaDuyetAsync(nhom, mucTieu)"],1020,380,260,fill="#f5f5f5")
dclass(d,"rm","MauKeHoachRepository",["+TimMauDaDuyetAsync(nhom, mucTieu)"],1020,540,260)
d.e("d1","nc2","nsvc","gọi",DEP)
d.e("d2","nsvc","irn","dùng",DEP)
d.e("d3","irn","rn","realizes",REAL)
d.e("d4","nsvc","nk","tạo",DEP)
d.e("d5","nk","mon","tham chiếu",ASC)
d.e("d6","nsvc","ho","kiểm tra BR08",DEP)
d.e("d7","tsvc","ho","CoNhomRuiroCao",DEP)
d.e("d8","tsvc","irm","dùng",DEP)
d.e("d9","irm","rm","realizes",REAL)
d.e("d10","tsvc","mau","chọn mẫu",DEP)
d.e("d11","sinh","mau","sinh mẫu NhapMau",DEP)
pages.append(d)

save("HLT-07_so_do.drawio", pages)
save_each = []
import unicodedata, re
def ascii_slug(text):
    t = text.replace("đ", "d").replace("Đ", "D")
    t = unicodedata.normalize("NFKD", t).encode("ascii", "ignore").decode()
    return re.sub(r"[^A-Za-z0-9]+", "_", t).strip("_")
for i, p in enumerate(pages, 1):
    save(f"{i:02d}_{ascii_slug(p.name.split('. ',1)[1])}.drawio", [p])
    save_each.append(p.name)

# Kiểm tra: mọi file phải là XML hợp lệ
ET.parse("HLT-07_so_do.drawio")
print("Đã tạo", len(pages), "sơ đồ:", save_each)
