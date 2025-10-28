import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Team } from '../../models/team';

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  // ✅ ahora apunta al endpoint correcto del backend
  private apiUrl = 'http://localhost:8082/api/teams';

  constructor(private http: HttpClient) {}

  getTeams(
    page: number = 1,
    pageSize: number = 10,
    search: string = ''
  ): Observable<{ content: Team[]; totalElements: number }> {
    const params = `?page=${page - 1}&size=${pageSize}&search=${encodeURIComponent(search)}`;
    return this.http.get<{ content: Team[]; totalElements: number }>(`${this.apiUrl}${params}`);
  }

  getAll(): Observable<Team[]> {
    return this.getTeams(1, 1000, '').pipe(map(r => r.content));
  }

  getById(id: number): Observable<Team> {
    return this.http.get<Team>(`${this.apiUrl}/${id}`);
  }

  create(team: Team): Observable<Team> {
    return this.http.post<Team>(this.apiUrl, team);
  }

  update(id: number, team: Team): Observable<Team> {
    return this.http.put<Team>(`${this.apiUrl}/${id}`, team);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
