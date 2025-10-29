import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import Swal from 'sweetalert2';
import {
  TournamentMatchTeamSlot,
  TournamentMatchView,
  TournamentSummary,
  TournamentTeamDetail
} from './tournaments.models';
import { TournamentsStore } from './tournaments.store';
import { MatchCardComponent } from './components/match-card/match-card.component';
import { MatchesService } from '@app/services/api/matches.service';

@Component({
  selector: 'app-tournaments',
  standalone: true,
  imports: [CommonModule, RouterLink, MatchCardComponent],
  templateUrl: './tournaments.component.html',
  styleUrls: ['./tournaments.component.css']
})
export class TournamentsComponent {
  private readonly store = inject(TournamentsStore);
  private readonly matchesService = inject(MatchesService);

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
  }

  protected percent(progress: number): number {
    return Math.round(progress * 100);
  }

  protected async assignMatch(groupId: string, slotIndex: number): Promise<void> {
    try {
      const matches = await firstValueFrom(this.matchesService.getMatches());
      const currentTournament = this.selectedTournament();
      const alreadyAssignedIds = new Set<number>();

      currentTournament?.groups.forEach((group) => {
        group.matches.forEach((match) => {
          if (!match.isPlaceholder) {
            const numericId = Number(match.id);
            if (Number.isFinite(numericId)) {
              alreadyAssignedIds.add(numericId);
            }
          }
        });
      });

      if (currentTournament?.final && !currentTournament.final.isPlaceholder) {
        const numericId = Number(currentTournament.final.id);
        if (Number.isFinite(numericId)) {
          alreadyAssignedIds.add(numericId);
        }
      }

      const candidates = matches.filter((match) => {
        const status = (match.status || '').toLowerCase();
        if (status === 'finished') return false;
        const numericId = Number(match.id);
        if (!Number.isFinite(numericId)) return false;
        return !alreadyAssignedIds.has(numericId);
      });

      if (!candidates.length) {
        await Swal.fire({
          icon: 'info',
          title: 'Sin partidos disponibles',
          text: 'No hay partidos programados que puedan asignarse en este momento.'
        });
        return;
      }

      const options = candidates.reduce<Record<string, string>>((acc, match) => {
        const numericId = Number(match.id);
        const label = `${match.homeTeamName} vs ${match.awayTeamName}`;
        const dateLabel = match.dateTime
          ? new Intl.DateTimeFormat('es-ES', {
              dateStyle: 'medium',
              timeStyle: 'short'
            }).format(new Date(match.dateTime))
          : 'Sin fecha';
        acc[String(numericId)] = `${label} · ${dateLabel} · Estado: ${match.status}`;
        return acc;
      }, {});

      const result = await Swal.fire<string>({
        title: 'Selecciona un partido',
        input: 'select',
        inputOptions: options,
        inputPlaceholder: 'Partido programado',
        showCancelButton: true,
        cancelButtonText: 'Cancelar',
        confirmButtonText: 'Asignar'
      });

      if (result.isConfirmed && result.value) {
        const numericId = Number(result.value);
        await this.store.assignMatchToSlot(groupId, slotIndex, numericId);
        await Swal.fire({
          icon: 'success',
          title: 'Partido asignado',
          timer: 1500,
          showConfirmButton: false
        });
      }
    } catch (error: any) {
      const message =
        error?.error?.error ??
        error?.message ??
        'No se pudo asignar el partido.';
      await Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message
      });
    }
  }

  protected async clearSlot(groupId: string, slotIndex: number): Promise<void> {
    const confirm = await Swal.fire({
      icon: 'warning',
      title: 'Quitar partido',
      text: '¿Deseas quitar el partido seleccionado de este bracket?',
      showCancelButton: true,
      confirmButtonText: 'Sí, quitar',
      cancelButtonText: 'Cancelar'
    });

    if (!confirm.isConfirmed) return;

    try {
      await this.store.assignMatchToSlot(groupId, slotIndex, null);
      await Swal.fire({
        icon: 'success',
        title: 'Slot liberado',
        timer: 1200,
        showConfirmButton: false
      });
    } catch (error: any) {
      const message =
        error?.error?.error ??
        error?.message ??
        'No se pudo quitar el partido.';
      await Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message
      });
    }
  }

}
