import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Player } from '../../models/player';

interface PlayerApi {
  id: number;
  name: string;
  age: number;
  position?: string | null;
  team_id?: number | null;
}

interface PlayerApiResponse {
  items: PlayerApi[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class PlayerService {
  private apiUrl = 'http://localhost:3000/api/players';

  constructor(private http: HttpClient) {}

  // 🔄 Conversión de API → Modelo interno
  private fromApi(p: PlayerApi): Player {
    return {
      id: p.id,
      name: p.name,
      age: p.age,
      position: p.position ?? null,
      teamId: p.team_id ?? null
    };
  }

  // 🔄 Conversión inversa (Modelo → API)
  private toApi(p: Partial<Player>): Partial<PlayerApi> {
    return {
      id: p.id,
      name: p.name,
      age: p.age,
      position: p.position ?? null,
      team_id: p.teamId ?? null
    };
  }

  // ✅ Obtener todos los jugadores con formato de backend ({items, totalCount})
  getAll(): Observable<{ items: Player[]; totalCount: number }> {
    return this.http.get<PlayerApiResponse>(this.apiUrl).pipe(
      map(res => ({
        items: res.items.map(r => this.fromApi(r)),
        totalCount: res.totalCount
      }))
    );
  }

  // ✅ Obtener jugador por ID
  getById(id: number): Observable<Player> {
    return this.http.get<PlayerApi>(`${this.apiUrl}/${id}`).pipe(
      map(r => this.fromApi(r))
    );
  }

  // ✅ Crear jugador
  create(player: Player): Observable<Player> {
    const body = this.toApi(player);
    return this.http.post<PlayerApi>(this.apiUrl, body).pipe(
      map(r => this.fromApi(r))
    );
  }

  // ✅ Actualizar jugador
  update(id: number, player: Partial<Player>): Observable<Player> {
    const body = this.toApi(player);
    return this.http.put<PlayerApi>(`${this.apiUrl}/${id}`, body).pipe(
      map(r => this.fromApi(r))
    );
  }

  // ✅ Eliminar jugador
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // ✅ Obtener jugadores por equipo
  getByTeam(teamId: number): Observable<Player[]> {
    return this.http
      .get<PlayerApiResponse>(`/api/teams/${teamId}/players`)
      .pipe(map(res => res.items.map(r => this.fromApi(r))));
  }
}
