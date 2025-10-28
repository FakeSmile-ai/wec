import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TournamentMatchTeamSlot, TournamentMatchView } from '../../tournaments.models';

@Component({
  selector: 'app-tournament-match-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './match-card.component.html',
  styleUrls: ['./match-card.component.scss']
})
export class MatchCardComponent {
  @Input({ required: true }) match!: TournamentMatchView;
  @Input() highlightTeamId: string | null = null;

  @Output() teamFocus = new EventEmitter<string | null>();

  /** Clase CSS según estado del partido */
  protected statusClass(): string {
    switch (this.match.status) {
      case 'live':
        return 'is-live';
      case 'finished':
        return 'is-finished';
      default:
        return '';
    }
  }

  /** Determina si el equipo fue ganador */
  protected isWinner(slot: TournamentMatchTeamSlot): boolean {
    return Boolean(slot.id && !slot.isPlaceholder && this.match.winnerId === slot.id);
  }

  /** Determina si el equipo está resaltado */
  protected isHighlighted(slot: TournamentMatchTeamSlot): boolean {
    return Boolean(slot.id && slot.id === this.highlightTeamId);
  }

  /** Muestra el marcador o guion si aún no existe */
  protected displayScore(slot: TournamentMatchTeamSlot): string {
    if (slot.score === null || slot.score === undefined) return '—';
    return String(slot.score);
  }

  /** Emite evento de enfoque / clic en equipo */
  protected handleFocus(slot: TournamentMatchTeamSlot): void {
    if (!slot.id || slot.isPlaceholder) return;
    this.teamFocus.emit(slot.id);
  }
}
