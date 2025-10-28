# ========== PDF: Equipos ==========
from datetime import datetime
from io import BytesIO

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.platypus import Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle

try:
    from zoneinfo import ZoneInfo
except Exception:
    ZoneInfo = None  # fallback


def _parse_color(value: str):
    """Admite #RRGGBB o nombres ('Red','Yellow')."""
    if not value:
        return colors.whitesmoke
    try:
        v = str(value).strip()
        if v.startswith("#"):
            return colors.HexColor(v)
        # nombres -> toColor (puede lanzar)
        return colors.toColor(v)
    except Exception:
        return colors.whitesmoke


def _text_color_for(bg):
    # bg.red/green/blue ∈ [0,1]
    lum = 0.299 * bg.red + 0.587 * bg.green + 0.114 * bg.blue
    return colors.black if lum > 0.5 else colors.white


def _to_local(dt_str: str, tz: str = "UTC") -> str:
    if not dt_str:
        return ""
    try:
        dt = datetime.fromisoformat(dt_str.replace("Z", ""))
        if ZoneInfo and tz and tz.upper() != "UTC":
            try:
                dt = dt.astimezone(ZoneInfo(tz))
            except Exception:
                pass
        return dt.strftime("%Y-%m-%d %H:%M")
    except Exception:
        # si no es ISO, regresa tal cual
        return dt_str


def generate_teams_pdf(
    teams: list[dict], *, tz: str = "UTC", title="Equipos Registrados"
):
    """
    teams: lista de dicts con claves:
      Name/name/teamName, Color/color/hexColor, Created/created/createdAt
    """
    buf = BytesIO()
    doc = SimpleDocTemplate(
        buf, pagesize=A4, topMargin=36, bottomMargin=36, leftMargin=40, rightMargin=40
    )
    styles = getSampleStyleSheet()
    elems = []

    elems.append(Paragraph(title, styles["Title"]))
    now = datetime.utcnow().strftime("%Y-%m-%d %H:%M UTC")
    elems.append(Paragraph(now, styles["Normal"]))
    elems.append(Spacer(1, 12))

    data = [["Nombre", "Color", "Creado"]]
    row_styles = []

    # construir filas
    for idx, t in enumerate(teams, start=1):
        name = (
            t.get("name")
            or t.get("Name")
            or t.get("teamName")
            or t.get("TeamName")
            or ""
        )
        color_val = t.get("color") or t.get("Color") or t.get("hexColor") or ""
        created = (
            t.get("created")
            or t.get("Created")
            or t.get("createdAt")
            or t.get("CreatedAt")
            or ""
        )

        color_obj = _parse_color(color_val)
        txt_color = _text_color_for(color_obj)

        data.append([name, color_val or "-", _to_local(created, tz)])

        # aplicar fondo y color de texto a la celda "Color" (columna 1 -> índice 1)
        row = idx  # por el header
        row_styles += [
            ("BACKGROUND", (1, row), (1, row), color_obj),
            ("TEXTCOLOR", (1, row), (1, row), txt_color),
        ]

    # tabla
    table = Table(data, colWidths=[None, 90, 140], hAlign="LEFT")
    base_style = TableStyle(
        [
            ("GRID", (0, 0), (-1, -1), 0.25, colors.grey),
            ("BACKGROUND", (0, 0), (-1, 0), colors.black),
            ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
            ("ALIGN", (0, 0), (-1, 0), "CENTER"),
            ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
            ("FONTNAME", (0, 1), (-1, -1), "Helvetica"),
            ("FONTSIZE", (0, 0), (-1, -1), 10),
            ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.whitesmoke, colors.lightgrey]),
        ]
    )
    base_style.add(*("ALIGN", (1, 1), (1, -1), "CENTER"))  # centrar columna Color
    for s in row_styles:
        base_style.add(*s)
    table.setStyle(base_style)

    elems.append(table)
    doc.build(elems)
    return buf.getvalue()
