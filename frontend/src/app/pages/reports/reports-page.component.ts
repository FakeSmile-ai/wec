import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ReportsService } from '@app/services/api/reports.service';

type Team = { id: number; name: string };

@Component({
  standalone: true,
  selector: 'app-reports-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss'],
})
export class ReportsPage {
  private http = inject(HttpClient);
  private reports = inject(ReportsService);

  teams = signal<Team[]>([]);
  loadingTeams = signal<boolean>(true);
  teamId = signal<number | null>(null);

  from = signal<string>('');   // yyyy-mm-dd
  to = signal<string>('');
  matchId = signal<number | null>(null);

  errorMsg = signal<string>('');

  constructor() {
    this.loadTeams();
  }

  private loadTeams() {
    this.loadingTeams.set(true);
    this.http.get<any>('/api/teams').subscribe({
      next: (res) => {
        const items: Team[] = Array.isArray(res?.items) ? res.items : (res ?? []);
        this.teams.set(items);
        this.loadingTeams.set(false);
      },
      error: () => {
        this.loadingTeams.set(false);
        this.errorMsg.set('No se pudo cargar la lista de equipos.');
      },
    });
  }

  private saveBlob(blob: Blob, filename: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.download = filename;
    a.href = url;
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(url);
    a.remove();
  }

  // ===== Botones =====
  downloadTeams() {
    this.errorMsg.set('');
    this.reports.downloadTeams().subscribe({
      next: (blob) => this.saveBlob(blob, 'equipos.pdf'),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  downloadPlayersByTeam() {
    this.errorMsg.set('');
    if (!this.teamId()) {
      this.errorMsg.set('Selecciona un equipo.');
      return;
    }
    this.reports.downloadPlayersByTeam(this.teamId()!).subscribe({
      next: (blob) => this.saveBlob(blob, `jugadores_${this.teamId()}.pdf`),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  downloadMatchesHistory() {
    this.errorMsg.set('');
    const params = { from: this.from() || undefined, to: this.to() || undefined };
    this.reports.downloadMatchesHistory(params).subscribe({
      next: (blob) => this.saveBlob(blob, 'historial_partidos.pdf'),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  downloadMatchRoster() {
    this.errorMsg.set('');
    if (!this.matchId()) {
      this.errorMsg.set('Ingresa un Match ID.');
      return;
    }
    this.reports.downloadMatchRoster(this.matchId()!).subscribe({
      next: (blob) => this.saveBlob(blob, `roster_${this.matchId()}.pdf`),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  // NUEVO: standings
  downloadStandings() {
    this.errorMsg.set('');
    this.reports.downloadStandings().subscribe({
      next: (blob) => this.saveBlob(blob, 'standings.pdf'),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  // NUEVOS: todos los jugadores / resumen de estadísticas
  downloadAllPlayers() {
    this.errorMsg.set('');
    this.reports.downloadAllPlayers().subscribe({
      next: (blob) => this.saveBlob(blob, 'players_all.pdf'),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  downloadStatsSummary() {
    this.errorMsg.set('');
    this.reports.downloadStatsSummary().subscribe({
      next: (blob) => this.saveBlob(blob, 'stats_summary.pdf'),
      error: (e: HttpErrorResponse) => this.errorMsg.set(this.humanError(e)),
    });
  }

  private humanError(e: HttpErrorResponse): string {
    if (e.status === 401) return 'No autorizado (revisa sesión / token).';
    if (e.status === 502) return 'Gateway error (¿Nginx inyecta el RS_TOKEN?).';
    if (e.status === 500) return 'Error del servidor de reportes.';
    return `Error ${e.status || ''}`.trim();
  }
}
