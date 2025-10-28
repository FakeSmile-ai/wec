# app/clients.py
from __future__ import annotations

from datetime import datetime
from typing import Any, Optional

import httpx

from .config import (
    TEAMS_API_BASE,
    PLAYERS_API_BASE,
    MATCHES_API_BASE,
    TEAMS_API_TOKEN,
    PLAYERS_API_TOKEN,
    MATCHES_API_TOKEN,
    choose_header,
)

# -------------------------
# Utilidades de parseo
# -------------------------
def _as_list_items(data: Any) -> list[dict[str, Any]]:
    if isinstance(data, list):
        return data
    if isinstance(data, dict):
        for k in ("items", "content", "results"):
            v = data.get(k)
            if isinstance(v, list):
                return v
        v = data.get("data")
        if isinstance(v, list):
            return v
        if isinstance(v, dict):
            for k in ("items", "content", "results"):
                vv = v.get(k)
                if isinstance(vv, list):
                    return vv
        return []
    return []

async def fetch_teams(
    x_api_auth: str | None = None, x_teams_auth: str | None = None
) -> list[dict[str, Any]]:
    url = f"{TEAMS_API_BASE}/api/teams"
    headers = choose_header(x_teams_auth, x_api_auth, TEAMS_API_TOKEN)

    # Intentamos varios esquemas de paginación conocidos
    schemes = [
        ("page", "size", 0),       # Spring Data por defecto (0-based)
        ("page", "pageSize", 0),   # Tu front usa page/pageSize
        ("pageNumber", "pageSize", 1),
        ("skip", "take", 0),
    ]

    async with httpx.AsyncClient(timeout=30) as cx:
        for page_key, size_key, start in schemes:
            page = start
            page_size = 500
            acc: list[dict[str, Any]] = []
            while True:
                params = {page_key: page, size_key: page_size}
                r = await cx.get(url, headers=headers, params=params)
                if r.status_code >= 400:
                    break  # probamos el siguiente esquema
                items = _as_list_items(r.json())
                if not items:
                    break
                acc.extend(items)
                # Si vinieron menos que el tamaño solicitado, no hay más páginas
                if len(items) < page_size:
                    return acc
                page += 1

        # Fallback final: sin paginación (por si tu endpoint soporta lista completa)
        r = await cx.get(url, headers=headers)
        r.raise_for_status()
        return _as_list_items(r.json())


async def fetch_team_by_id(
    team_id: str, x_api_auth: Optional[str] = None, x_teams_auth: Optional[str] = None
) -> dict[str, Any] | None:
    url = f"{TEAMS_API_BASE}/api/teams/{team_id}"
    headers = choose_header(x_teams_auth, x_api_auth, TEAMS_API_TOKEN)
    async with httpx.AsyncClient(timeout=30) as cx:
        r = await cx.get(url, headers=headers)
        if r.status_code == 404:
            return None
        r.raise_for_status()
        return r.json()

async def fetch_teams_map(
    x_api_auth: Optional[str] = None, x_teams_auth: Optional[str] = None
) -> dict[str, str]:
    teams = await fetch_teams(x_api_auth, x_teams_auth)
    mapping: dict[str, str] = {}
    for t in teams:
        tid = str(t.get("id") or t.get("Id") or "")
        name = t.get("name") or t.get("Name") or t.get("teamName") or t.get("TeamName") or tid
        if tid:
            mapping[tid] = str(name)
    return mapping

# -------------------------
# Players-service
# -------------------------
async def fetch_players(
    team_id: Optional[str] = None,
    x_api_auth: Optional[str] = None,
    x_players_auth: Optional[str] = None,
) -> list[dict[str, Any]]:
    """
    GET /api/players?teamId=...
    Nuevo esquema: id, name, age, position, team_id, createdat, updatedat
    """
    url = f"{PLAYERS_API_BASE}/api/players"
    headers = choose_header(x_players_auth, x_api_auth, PLAYERS_API_TOKEN)
    params: dict[str, Any] = {}
    if team_id:
        params["teamId"] = team_id
    async with httpx.AsyncClient(timeout=30) as cx:
        r = await cx.get(url, params=params, headers=headers)
        r.raise_for_status()
        return _as_list_items(r.json())

async def fetch_player_by_id(
    player_id: str,
    x_api_auth: Optional[str] = None,
    x_players_auth: Optional[str] = None,
) -> dict[str, Any] | None:
    url = f"{PLAYERS_API_BASE}/api/players/{player_id}"
    headers = choose_header(x_players_auth, x_api_auth, PLAYERS_API_TOKEN)
    async with httpx.AsyncClient(timeout=30) as cx:
        r = await cx.get(url, headers=headers)
        if r.status_code == 404:
            return None
        r.raise_for_status()
        return r.json()

# -------------------------
# Matches-service
# -------------------------
async def fetch_matches(
    from_date: Optional[str] = None,
    to_date: Optional[str] = None,
    x_api_auth: Optional[str] = None,
    x_matches_auth: Optional[str] = None,
) -> list[dict[str, Any]]:
    """
    GET /api/matches?from=...&to=...
    Esquema: Id, HomeTeamId, AwayTeamId, HomeScore, AwayScore, Period, Status, DateMatch, QuarterDurationSeconds
    """
    url = f"{MATCHES_API_BASE}/api/matches"
    headers = choose_header(x_matches_auth, x_api_auth, MATCHES_API_TOKEN)
    params: dict[str, Any] = {}
    if from_date:
        params["from"] = from_date
    if to_date:
        params["to"] = to_date

    async with httpx.AsyncClient(timeout=30) as cx:
        r = await cx.get(url, params=params, headers=headers)
        r.raise_for_status()
        items = _as_list_items(r.json())

    # Filtro defensivo por si el servicio aún no filtra
    if from_date or to_date:
        f_dt = _parse_iso(from_date)
        t_dt = _parse_iso(to_date)
        filtered: list[dict[str, Any]] = []
        for m in items:
            d = _parse_iso(str(m.get("DateMatch") or m.get("dateMatch") or m.get("date")))
            if f_dt and (not d or d < f_dt):
                continue
            if t_dt and (not d or d > t_dt):
                continue
            filtered.append(m)
        items = filtered
    return items

async def fetch_match_by_id(
    match_id: str,
    x_api_auth: Optional[str] = None,
    x_matches_auth: Optional[str] = None,
) -> dict[str, Any] | None:
    url = f"{MATCHES_API_BASE}/api/matches/{match_id}"
    headers = choose_header(x_matches_auth, x_api_auth, MATCHES_API_TOKEN)
    async with httpx.AsyncClient(timeout=30) as cx:
        r = await cx.get(url, headers=headers)
        if r.status_code == 404:
            return None
        r.raise_for_status()
        return r.json()
