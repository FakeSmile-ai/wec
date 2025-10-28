import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = '/api';

  getMatch(id: number) {
    return this.http.get<any>(`${this.base}/matches/${id}`).pipe(
      map(match => {
        const timer = match?.timer ?? {};
        const foulsHome = match?.foulsHome ?? match?.homeFouls ?? timer?.fouls?.home ?? 0;
        const foulsAway = match?.foulsAway ?? match?.awayFouls ?? timer?.fouls?.away ?? 0;
        return {
          ...match,
          homeTeam: match?.homeTeamName ?? match?.homeTeam ?? 'Local',
          awayTeam: match?.awayTeamName ?? match?.awayTeam ?? 'Visita',
          homeFouls: foulsHome,
          awayFouls: foulsAway,
          fouls: {
            home: foulsHome,
            away: foulsAway
          },
          timer: {
            running: match?.timerRunning ?? timer?.running ?? false,
            remainingSeconds: match?.timeRemaining ?? timer?.remainingSeconds ?? 0,
            quarter: match?.quarter ?? timer?.quarter ?? 1
          }
        };
      })
    );
  }

  //SCORE
  createScore(id: number, body: { teamId: number; points: 1|2|3; playerId?: number }) {
    return this.http.post(`${this.base}/matches/${id}/score`, body);
  }
  adjustScore(id: number, body: { teamId: number; delta: number }) {
    return this.http.post(`${this.base}/matches/${id}/score/adjust`, body);
  }

  //FOULS
  addFoul(id: number, body: { teamId: number; playerId?: number; type?: string }) {
    return this.http.post(`${this.base}/matches/${id}/fouls`, body);
  }
  adjustFoul(id: number, body: { teamId: number; delta: number }) {
    return this.http.post(`${this.base}/matches/${id}/fouls/adjust`, body);
  }

  //TIMER
  startTimer(id: number, body?: { quarterDurationSeconds?: number }) {
    return this.http.post(`${this.base}/matches/${id}/timer/start`, body ?? {});
  }
  pauseTimer(id: number)  { return this.http.post(`${this.base}/matches/${id}/timer/pause`, {}); }
  resumeTimer(id: number) { return this.http.post(`${this.base}/matches/${id}/timer/resume`, {}); }
  resetTimer(id: number)  { return this.http.post(`${this.base}/matches/${id}/timer/reset`, {}); }

  // Quarter
  advanceQuarter(id: number) { return this.http.post(`${this.base}/matches/${id}/quarters/advance`, {}); }
  autoAdvanceQuarter(id: number) { return this.http.post(`${this.base}/matches/${id}/quarters/auto-advance`, {}); }

  // New game
  newGame(body: { homeName: string; awayName: string; quarterDurationSeconds?: number }) {
    return this.http.post<any>(`${this.base}/matches/new`, body);
  }
  newGameByTeams(body: { homeTeamId: number; awayTeamId: number; quarterDurationSeconds?: number }) {
    return this.http.post<any>(`${this.base}/matches/new-by-teams`, body);
  }

  // Teams
  listTeams() {
    return this.http.get<Array<{ id: number; name: string; color?: string; playersCount: number }>>(
      `${this.base}/teams`
    );
  }
  createTeam(body: { name: string; color?: string; players: { number?: number; name: string }[] }) {
    return this.http.post('/api/teams', body);
  }

  getStandings() {
    return this.http.get<Array<{ id: number; name: string; color?: string; wins: number }>>(
      `${this.base}/standings`
    );
  }



}
