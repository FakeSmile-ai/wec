import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  TournamentMatchTeamSlot,
  TournamentMatchView,
  TournamentSummary,
  TournamentTeamDetail
} from './tournaments.models';
import { TournamentsStore } from './tournaments.store';
import { MatchCardComponent } from './components/match-card/match-card.component';

@Component({
  selector: 'app-tournaments',
  standalone: true,
  imports: [CommonModule, RouterLink, MatchCardComponent],
  templateUrl: './tournaments.component.html',
  styleUrls: ['./tournaments.component.css']
})
export class TournamentsComponent {
  private readonly store = inject(TournamentsStore);

  protected readonly tournaments = this.store.tournaments;
  protected readonly selectedTournament = this.store.selectedTournament;
  protected readonly activeTeam = this.store.activeTeam;
  protected readonly loading = this.store.loading;
  protected readonly error = this.store.error;
  protected readonly activeTeamId = computed(() => this.activeTeam()?.id ?? null);

  protected trackByTournament(_: number, item: TournamentSummary): string {
    return item.id;
  }

  protected trackByMatch(_: number, item: TournamentMatchView): string {
    return item.id;
  }

  protected trackByTeam(_: number, item: TournamentMatchTeamSlot): string {
    return item.id ?? item.displayName;
  }

  protected selectTournament(id: string): void {
    void this.store.selectTournament(id);
  }

  protected focusTeam(teamId: string | null): void {
    this.store.focusTeam(teamId);
  }

  protected clearTeam(): void {
    this.store.focusTeam(null);
  }

  protected teamAccent(team: TournamentTeamDetail | null): string | null {
    if (!team) return null;
    return `linear-gradient(135deg, ${team.palette.primary}, ${team.palette.secondary})`;
    // si prefieres sólido: return team.palette.primary;
  }

  protected percent(progress: number): number {
    return Math.round(progress * 100);
  }
}
