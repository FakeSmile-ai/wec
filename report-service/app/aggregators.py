# app/aggregators.py
from __future__ import annotations

from typing import Any, Optional

from . import clients

async def teams_map(x_api=None, x_teams=None) -> dict[str, str]:
    return await clients.fetch_teams_map(x_api, x_teams)

async def match_roster(
    match_id: str,
    x_api: Optional[str] = None,
    x_matches: Optional[str] = None,
    x_players: Optional[str] = None,
    x_teams: Optional[str] = None,
) -> dict[str, Any]:
    """
    Devuelve:
      {
        "match": {...},  # con nombres de equipos resueltos
        "homePlayers": [...],
        "awayPlayers": [...]
      }
    """
    match = await clients.fetch_match_by_id(match_id, x_api, x_matches)
    if not match:
        return {"match": None, "homePlayers": [], "awayPlayers": []}

    home_id = str(match.get("HomeTeamId") or match.get("homeTeamId") or "")
    away_id = str(match.get("AwayTeamId") or match.get("awayTeamId") or "")

    # Players por equipo (desde players-service)
    home_players = await clients.fetch_players(home_id or None, x_api, x_players)
    away_players = await clients.fetch_players(away_id or None, x_api, x_players)

    # Resolver nombres de teams
    tmap = await clients.fetch_teams_map(x_api, x_teams)
    match_resolved = dict(match)
    match_resolved["HomeTeamName"] = tmap.get(home_id, home_id)
    match_resolved["AwayTeamName"] = tmap.get(away_id, away_id)

    return {"match": match_resolved, "homePlayers": home_players, "awayPlayers": away_players}

def _ensure_int(v, default=0) -> int:
    try: return int(v)
    except Exception: return default

def aggregate_stats_from_matches(matches: list[dict[str, Any]], tmap: dict[str, str]) -> dict[str, dict[str, Any]]:
    """
    Calcula PJ/PG/PP/PF/PC por equipo a partir de la lista de partidos (matches-service).
    """
    stats: dict[str, dict[str, Any]] = {}

    def ensure_slot(team_id: str) -> dict[str, Any]:
        if team_id not in stats:
            stats[team_id] = {
                "teamId": team_id,
                "team": tmap.get(team_id, team_id),
                "played": 0, "wins": 0, "losses": 0, "pf": 0, "pa": 0
            }
        return stats[team_id]

    for m in matches:
        h_id = str(m.get("HomeTeamId") or m.get("homeTeamId") or "")
        a_id = str(m.get("AwayTeamId") or m.get("awayTeamId") or "")
        hs   = _ensure_int(m.get("HomeScore") or m.get("homeScore"))
        as_  = _ensure_int(m.get("AwayScore") or m.get("awayScore"))

        if not h_id or not a_id:  # datos incompletos
            continue

        H = ensure_slot(h_id); A = ensure_slot(a_id)
        H["played"] += 1; A["played"] += 1
        H["pf"] += hs; H["pa"] += as_
        A["pf"] += as_; A["pa"] += hs

        if hs > as_:
            H["wins"] += 1; A["losses"] += 1
        elif as_ > hs:
            A["wins"] += 1; H["losses"] += 1

    return stats
