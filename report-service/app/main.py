# app/main.py  (fragmentos clave)
from __future__ import annotations

import io
import os
from typing import Any, Optional

import httpx
from fastapi import Depends, FastAPI, Header, HTTPException, Request, Query
from fastapi.responses import JSONResponse, StreamingResponse
from jose import JWTError, jwt

from . import clients, pdf_utils
from .aggregators import teams_map, match_roster, aggregate_stats_from_matches

app = FastAPI()

AUTH_SECRET = os.getenv("AUTH_SECRET", "change_me")
ALGO = "HS256"

# ---------- Seguridad idéntica a la tuya ----------
async def require_admin(authorization: str | None = Header(default=None)) -> None:
    if not authorization or not authorization.lower().startswith("bearer "):
        raise HTTPException(status_code=401, detail="Missing RS bearer")
    token = authorization.split(" ", 1)[1]
    try:
        payload = jwt.decode(token, AUTH_SECRET, algorithms=[ALGO])
    except JWTError as e:
        raise HTTPException(status_code=401, detail=f"Invalid RS bearer: {e}")
    role = (payload.get("role") or payload.get("roles") or "").lower()
    if "admin" not in str(role):
        raise HTTPException(status_code=403, detail="RS bearer not admin")

# ---------- Health ----------
@app.get("/health")
async def health():
    return {"status":"ok","service":"report-service","port":os.getenv("SERVICE_PORT","8080")}

def _upstream_502(e: httpx.HTTPStatusError) -> HTTPException:
    req = e.request; resp = e.response
    detail = {"upstream_url": str(req.url), "status_code": resp.status_code, "body": (resp.text or "")[:200]}
    return HTTPException(status_code=502, detail=detail)

# ----------------- REPORTES -----------------

# Equipos
@app.get("/reports/teams.pdf", dependencies=[Depends(require_admin)])
async def report_teams(
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        teams = await clients.fetch_teams(x_api_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)
    pdf = pdf_utils.build_pdf_teams(teams)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": 'attachment; filename="equipos.pdf"'})

# Jugadores por equipo
@app.get("/reports/teams/{team_id}/players.pdf", dependencies=[Depends(require_admin)])
async def report_players_by_team(
    team_id: str,
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_players_authorization: str | None = Header(default=None, alias="X-Players-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        players = await clients.fetch_players(team_id, x_api_authorization, x_players_authorization)
        tmap    = await clients.fetch_teams_map(x_api_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)

    team_name = tmap.get(str(team_id))
    pdf = pdf_utils.build_pdf_players_by_team(team_id, players, team_name)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": f'attachment; filename="players_{team_id}.pdf"'})

# Todos los jugadores
@app.get("/reports/players/all.pdf", dependencies=[Depends(require_admin)])
async def report_all_players(
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_players_authorization: str | None = Header(default=None, alias="X-Players-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        players = await clients.fetch_players(None, x_api_authorization, x_players_authorization)
        tmap    = await clients.fetch_teams_map(x_api_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)
    pdf = pdf_utils.build_pdf_all_players(players, tmap)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": 'attachment; filename="players_all.pdf"'})

# Historial de partidos
@app.get("/reports/matches/history.pdf", dependencies=[Depends(require_admin)])
async def report_history(
    from_: str | None = Query(default=None, alias="from"),
    to: str | None = None,
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_matches_authorization: str | None = Header(default=None, alias="X-Matches-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        matches = await clients.fetch_matches(from_, to, x_api_authorization, x_matches_authorization)
        tmap    = await clients.fetch_teams_map(x_api_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)
    pdf = pdf_utils.build_pdf_matches_history(matches, from_, to, tmap)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": 'attachment; filename="history.pdf"'})

# Roster de un partido
@app.get("/reports/matches/{match_id}/roster.pdf", dependencies=[Depends(require_admin)])
async def report_match_roster_pdf(
    match_id: str,
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_matches_authorization: str | None = Header(default=None, alias="X-Matches-Authorization"),
    x_players_authorization: str | None = Header(default=None, alias="X-Players-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        roster = await match_roster(match_id, x_api_authorization, x_matches_authorization, x_players_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)
    pdf = pdf_utils.build_pdf_match_roster(match_id, roster)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": f'attachment; filename="roster_{match_id}.pdf"'})

# Standings / Stats resumen (derivado de matches)
@app.get("/reports/stats/summary.pdf", dependencies=[Depends(require_admin)])
async def report_stats_summary(
    x_api_authorization: str | None = Header(default=None, alias="X-Api-Authorization"),
    x_matches_authorization: str | None = Header(default=None, alias="X-Matches-Authorization"),
    x_teams_authorization: str | None = Header(default=None, alias="X-Teams-Authorization"),
):
    try:
        matches = await clients.fetch_matches(None, None, x_api_authorization, x_matches_authorization)
        tmap    = await teams_map(x_api_authorization, x_teams_authorization)
    except httpx.HTTPStatusError as e:
        raise _upstream_502(e)

    agg = aggregate_stats_from_matches(matches, tmap)

    # Top por Victorias
    wins_sorted = sorted(agg.values(), key=lambda s: (-int(s["wins"]), s["team"]))
    top_wins = [["#", "Equipo", "ID", "PJ", "PG", "PP", "PF", "PC"]]
    for idx, s in enumerate(wins_sorted[:10], 1):
        top_wins.append([idx, s["team"], s["teamId"], s["played"], s["wins"], s["losses"], s["pf"], s["pa"]])

    # Top por PF
    pf_sorted = sorted(agg.values(), key=lambda s: (-int(s["pf"]), s["team"]))
    top_pf = [["#", "Equipo", "ID", "PJ", "PG", "PP", "PF", "PC"]]
    for idx, s in enumerate(pf_sorted[:10], 1):
        top_pf.append([idx, s["team"], s["teamId"], s["played"], s["wins"], s["losses"], s["pf"], s["pa"]])

    # Menos PF
    min_pf_sorted = sorted(agg.values(), key=lambda s: (int(s["pf"]), s["team"]))
    min_pf = [["#", "Equipo", "ID", "PJ", "PG", "PP", "PF", "PC"]]
    for idx, s in enumerate(min_pf_sorted[:10], 1):
        min_pf.append([idx, s["team"], s["teamId"], s["played"], s["wins"], s["losses"], s["pf"], s["pa"]])

    # Menos derrotas
    min_losses_sorted = sorted(agg.values(), key=lambda s: (int(s["losses"]), -int(s["wins"]), s["team"]))
    min_losses = [["#", "Equipo", "ID", "PJ", "PG", "PP", "PF", "PC"]]
    for idx, s in enumerate(min_losses_sorted[:10], 1):
        min_losses.append([idx, s["team"], s["teamId"], s["played"], s["wins"], s["losses"], s["pf"], s["pa"]])

    sections = [
        ("Top por Victorias", top_wins),
        ("Top por Puntos a Favor", top_pf),
        ("Menos Puntos a Favor", min_pf),
        ("Menos Derrotas", min_losses),
    ]
    pdf = pdf_utils.build_pdf_stats_report(sections)
    return StreamingResponse(io.BytesIO(pdf), media_type="application/pdf", headers={"Content-Disposition": 'attachment; filename="stats_summary.pdf"'})

# Manejo genérico de httpx
@app.exception_handler(httpx.RequestError)
async def httpx_request_error_handler(_req: Request, exc: httpx.RequestError):
    return JSONResponse(status_code=502, content={"detail": {"message": str(exc)}})
