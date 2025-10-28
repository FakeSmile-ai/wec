import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type TeamSide = 'home' | 'away';
export interface MatchModel {
  id: number;
  homeTeamId: number;
  awayTeamId: number;
  homeTeamName: string;
  awayTeamName: string;
  homeScore: number;
  awayScore: number;
  foulsHome: number;
  foulsAway: number;
  quarter: number;
  timeRemaining: number;
  timerRunning: boolean;
  status: string;
  dateTime: string;
}

export interface ProgramMatchPayload {
  homeTeamId: number;
  awayTeamId: number;
  /** Fecha en formato YYYY-MM-DD */
  date: string;
  /** Hora en formato HH:mm */
  time: string;
  quarterDurationSeconds?: number;
}

@Injectable({ providedIn: 'root' })
export class MatchesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/matches';

  getMatches(): Observable<MatchModel[]> {
    return this.http.get<MatchModel[]>(this.baseUrl);
  }

  getMatch(matchId: number): Observable<MatchModel> {
    return this.http.get<MatchModel>(`${this.baseUrl}/${matchId}`);
  }

  programMatch(payload: ProgramMatchPayload): Observable<MatchModel> {
    return this.http.post<MatchModel>(`${this.baseUrl}/programar`, payload);
  }

  addScore(matchId: number, team: TeamSide, points: number): Observable<MatchModel> {
    return this.http.post<MatchModel>(`${this.baseUrl}/${matchId}/score`, { team, points });
  }

  addFoul(matchId: number, team: TeamSide, amount: number = 1): Observable<MatchModel> {
    return this.http.post<MatchModel>(`${this.baseUrl}/${matchId}/foul`, { team, amount });
  }

  updateTimer(matchId: number, action: 'start' | 'pause' | 'resume' | 'reset' | 'set', timeRemaining?: number): Observable<MatchModel> {
    const body: any = { action };
    if (typeof timeRemaining === 'number') {
      body.timeRemaining = timeRemaining;
    }
    return this.http.patch<MatchModel>(`${this.baseUrl}/${matchId}/timer`, body);
  }

  nextQuarter(matchId: number): Observable<MatchModel> {
    return this.http.patch<MatchModel>(`${this.baseUrl}/${matchId}/quarter`, {});
  }

  finishMatch(matchId: number, scores?: { homeScore?: number; awayScore?: number }): Observable<MatchModel> {
    return this.http.patch<MatchModel>(`${this.baseUrl}/${matchId}/finish`, scores ?? {});
  }
}
