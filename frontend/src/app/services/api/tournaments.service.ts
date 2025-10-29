// src/app/core/services/tournaments.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  TournamentSummary,
  TournamentViewModel,
  UpdateMatchRequest
} from '../../pages/tournaments/tournaments.models';

@Injectable({ providedIn: 'root' })
export class TournamentsApiService {
  private readonly http = inject(HttpClient);
  // Usa el proxy dev ya configurado (proxy.conf.json) => http://localhost:5220
  private readonly base = '/api';

  listTournaments(refresh = false): Promise<TournamentSummary[]> {
    const suffix = refresh ? '?refresh=true' : '';
    return firstValueFrom(
      this.http.get<TournamentSummary[]>(`${this.base}/tournaments${suffix}`)
    );
  }

  getTournament(id: string): Promise<TournamentViewModel> {
    return firstValueFrom(
      this.http.get<TournamentViewModel>(`${this.base}/tournaments/${id}`)
    );
  }

  updateMatch(
    tournamentId: string,
    matchId: string,
    payload: UpdateMatchRequest
  ): Promise<TournamentViewModel> {
    return firstValueFrom(
      this.http.patch<TournamentViewModel>(
        `${this.base}/tournaments/${tournamentId}/matches/${matchId}`,
        payload
      )
    );
  }

  assignMatchToSlot(
    tournamentId: string,
    groupId: string,
    slotIndex: number,
    matchId: number | null
  ): Promise<TournamentViewModel> {
    return firstValueFrom(
      this.http.put<TournamentViewModel>(
        `${this.base}/tournaments/${tournamentId}/groups/${groupId}/slots/${slotIndex}`,
        { matchId }
      )
    );
  }
}
