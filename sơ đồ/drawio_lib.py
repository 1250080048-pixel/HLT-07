"""Thư viện nhỏ tạo file .drawio (mxGraph XML). Mỗi sơ đồ là một trang (diagram)."""
from xml.sax.saxutils import escape

def _fix(label):
    # Mã hóa <<  >> để draw.io (html=1) hiển thị đúng, không coi là thẻ HTML; giữ nguyên <b>...</b>
    return label.replace("<<", "&lt;&lt;").replace(">>", "&gt;&gt;")

class Diagram:
    def __init__(self, name):
        self.name = name
        self.cells = []
        self.n = 2  # 0 và 1 là gốc
        self.ids = {}

    def _id(self, key):
        if key in self.ids:
            return self.ids[key]
        self.n += 1
        self.ids[key] = str(self.n)
        return self.ids[key]

    def v(self, key, label, x, y, w, h, style="rounded=1;whiteSpace=wrap;html=1;", parent=None):
        cid = self._id(key)
        p = self.ids[parent] if parent else "1"
        self.cells.append(
            f'<mxCell id="{cid}" value="{escape(_fix(label), {chr(10): "&#xa;"})}" style="{escape(style)}" vertex="1" parent="{p}">'
            f'<mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/></mxCell>')
        return cid

    def e(self, key, src, tgt, label="", style="endArrow=block;endFill=1;html=1;rounded=0;"):
        cid = self._id(key)
        self.cells.append(
            f'<mxCell id="{cid}" value="{escape(_fix(label))}" style="{escape(style)}" edge="1" parent="1" '
            f'source="{self.ids[src]}" target="{self.ids[tgt]}">'
            f'<mxGeometry relative="1" as="geometry"/></mxCell>')
        return cid

    def xml_page(self):
        body = "".join(self.cells)
        return (f'<diagram name="{escape(self.name)}" id="{self._id("__page__")}">'
                f'<mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" page="1" pageWidth="1600" pageHeight="1100">'
                f'<root><mxCell id="0"/><mxCell id="1" parent="0"/>{body}</root>'
                f'</mxGraphModel></diagram>')

def save(path, diagrams):
    pages = "".join(d.xml_page() for d in diagrams)
    xml = ('<?xml version="1.0" encoding="UTF-8"?>'
           '<mxfile host="app.diagrams.net" modified="2026-11-12" agent="HLT-07" version="24.0.0">'
           f'{pages}</mxfile>')
    with open(path, "w", encoding="utf-8") as f:
        f.write(xml)
