import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatchesService, MatchModel } from '../../services/api/matches.service';

@Component({
  selector: 'app-pick-match-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, NgIf, NgFor],
  template: `
    <h2 mat-dialog-title>Elegir partido</h2>

    <div mat-dialog-content>
      <table class="mat-elevation-z1" style="width:100%; border-collapse: collapse;">
        <thead>
          <tr>
            <th style="text-align:left; padding:8px;">Estado</th>
            <th style="text-align:left; padding:8px;">Local</th>
            <th style="text-align:left; padding:8px;">Visita</th>
            <th style="text-align:left; padding:8px;">Marcador</th>
            <th style="text-align:left; padding:8px;">Faltas</th>
            <th style="text-align:left; padding:8px;">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let r of rows()">
            <td style="padding:8px;">{{ r.status }}</td>
            <td style="padding:8px;">{{ r.homeTeamName || 'Equipo local' }}</td>
            <td style="padding:8px;">{{ r.awayTeamName || 'Equipo visitante' }}</td>
            <td style="padding:8px;">{{ r.homeScore }} - {{ r.awayScore }}</td>
            <td style="padding:8px;">{{ r.foulsHome }} - {{ r.foulsAway }}</td>
            <td style="padding:8px;">
              <button mat-stroked-button color="primary" (click)="play(r)" [disabled]="!canPlay(r)">
                Jugar
              </button>
            </td>
          </tr>

          <tr *ngIf="rows().length === 0">
            <td colspan="6" style="padding:12px; opacity:.7;">
              No hay partidos jugables (Scheduled o Live).
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="close()">Cerrar</button>
    </div>
  `
})
export class PickMatchDialogComponent implements OnInit {

  rows = signal<MatchModel[]>([]);

  private dialogRef = inject<MatDialogRef<PickMatchDialogComponent, { id: number } | undefined>>(MatDialogRef);
  private matchesSvc = inject(MatchesService);
  private data = inject(MAT_DIALOG_DATA) as unknown;

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.matchesSvc.getMatches().subscribe({
      next: matches => {
        const filtered = (matches ?? []).filter(m => this.canPlay(m));
        this.rows.set(filtered);
      },
      error: () => this.rows.set([])
    });
  }

  canPlay(row: MatchModel): boolean {
    const s = (row.status ?? '').toLowerCase().trim();
    const norm = s === 'programado' ? 'scheduled'
               : s === 'en juego'   ? 'live'
               : s;
    return norm === 'scheduled' || norm === 'live';
  }

  play(row: MatchModel): void {
    if (!this.canPlay(row)) return;
    this.dialogRef.close({ id: row.id });
  }

  close(): void { this.dialogRef.close(undefined); }
}
