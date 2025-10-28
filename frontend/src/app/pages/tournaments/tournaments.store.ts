import { Injectable, computed, inject, signal } from '@angular/core';
import {
  TournamentSummary,
  TournamentTeamDetail,
  TournamentViewModel,
  UpdateMatchRequest
} from './tournaments.models';
import { TournamentsApiService } from '@app/services/api/tournaments.service';

function ensureTeamIndex(tournament: TournamentViewModel): Record<string, TournamentTeamDetail> {
  if (tournament.teamsIndex && Object.keys(tournament.teamsIndex).length) {
    return tournament.teamsIndex;
  }
  return tournament.teams.reduce<Record<string, TournamentTeamDetail>>((acc, team) => {
    acc[team.id] = team;
    return acc;
  }, {});
}

@Injectable({ providedIn: 'root' })
export class TournamentsStore {
  private readonly api = inject(TournamentsApiService);

  private readonly summariesState = signal<TournamentSummary[]>([]);
  private readonly selectedTournamentId = signal<string | null>(null);
  private readonly tournamentDetailState = signal<TournamentViewModel | null>(null);
  private readonly focusedTeamId = signal<string | null>(null);

  private readonly loadingSummariesState = signal<boolean>(false);
  private readonly loadingDetailState = signal<boolean>(false);
  private readonly errorState = signal<string | null>(null);

  readonly tournaments = computed<TournamentSummary[]>(() => this.summariesState());
  readonly selectedTournament = computed<TournamentViewModel | null>(() => this.tournamentDetailState());
  readonly loadingSummaries = computed<boolean>(() => this.loadingSummariesState());
  readonly loadingDetail = computed<boolean>(() => this.loadingDetailState());
  readonly loading = computed<boolean>(() => this.loadingSummariesState() || this.loadingDetailState());
  readonly error = computed<string | null>(() => this.errorState());

  readonly activeTeam = computed<TournamentTeamDetail | null>(() => {
    const teamId = this.focusedTeamId();
    const tournament = this.tournamentDetailState();
    if (!teamId || !tournament) return null;
    const index = ensureTeamIndex(tournament);
    return index[teamId] ?? null;
  });

  constructor() {
    void this.loadTournaments();
  }

  async refresh(): Promise<void> {
    await this.loadTournaments();
  }

  async selectTournament(id: string): Promise<void> {
    if (id === this.selectedTournamentId()) return;
    await this.loadTournament(id);
  }

  focusTeam(id: string | null): void {
    this.focusedTeamId.set(id);
  }

  async updateMatchResult(tournamentId: string, matchId: string, scoreA: number, scoreB: number): Promise<void> {
    const payload: UpdateMatchRequest = { scoreA, scoreB, status: 'finished' };
    try {
      const updated = await this.api.updateMatch(tournamentId, matchId, payload);
      const normalized: TournamentViewModel = {
        ...updated,
        teamsIndex: ensureTeamIndex(updated)
      };
      this.tournamentDetailState.set(normalized);
      this.selectedTournamentId.set(normalized.id);
      this.syncSummary(normalized);
    } catch (error) {
      console.error('Failed to update match result', error);
      this.errorState.set('No se pudo actualizar el resultado del partido.');
    }
  }

  private async loadTournaments(): Promise<void> {
    this.loadingSummariesState.set(true);
    this.errorState.set(null);
    try {
      const summaries = await this.api.listTournaments();
      this.summariesState.set(summaries);

      const preferredId = this.selectedTournamentId();
      const targetId =
        preferredId && summaries.some((item) => item.id === preferredId)
          ? preferredId
          : summaries[0]?.id ?? null;

      if (targetId) {
        await this.loadTournament(targetId);
      } else {
        this.tournamentDetailState.set(null);
        this.selectedTournamentId.set(null);
      }
    } catch (error) {
      console.error('Failed to load tournaments', error);
      this.errorState.set('No se pudieron cargar los torneos. Inténtalo nuevamente.');
    } finally {
      this.loadingSummariesState.set(false);
    }
  }

  private async loadTournament(id: string): Promise<void> {
    this.loadingDetailState.set(true);
    this.errorState.set(null);
    try {
      const detail = await this.api.getTournament(id);
      const normalized: TournamentViewModel = {
        ...detail,
        teamsIndex: ensureTeamIndex(detail)
      };
      this.tournamentDetailState.set(normalized);
      this.selectedTournamentId.set(id);
      this.focusedTeamId.set(null);
      this.syncSummary(normalized);
    } catch (error) {
      console.error('Failed to load tournament detail', error);
      this.errorState.set('No se pudo cargar el torneo seleccionado.');
    } finally {
      this.loadingDetailState.set(false);
    }
  }

  private syncSummary(detail: TournamentViewModel): void {
    this.summariesState.update((items) =>
      items.map((item) =>
        item.id === detail.id
          ? {
              ...item,
              progress: detail.progress,
              matchesPlayed: detail.matchesPlayed,
              totalMatches: detail.totalMatches,
              scheduleLabel: detail.scheduleLabel
            }
          : item
      )
    );
  }
}
