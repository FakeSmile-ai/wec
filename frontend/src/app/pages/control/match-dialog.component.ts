import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatchesService, MatchListItem, PaginatedMatches } from '../../services/api/matches.service';

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
            <td style="padding:8px;">{{ r.homeTeam }}</td>
            <td style="padding:8px;">{{ r.awayTeam }}</td>
            <td style="padding:8px;">{{ r.homeScore }} - {{ r.awayScore }}</td>
            <td style="padding:8px;">{{ r.homeFouls }} - {{ r.awayFouls }}</td>
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

  rows = signal<MatchListItem[]>([]);

  private dialogRef = inject<MatDialogRef<PickMatchDialogComponent, { id: number } | undefined>>(MatDialogRef);
  private matchesSvc = inject(MatchesService);
  private data = inject(MAT_DIALOG_DATA) as unknown;

  ngOnInit(): void { this.load(); }

  private load(): void {
    this.matchesSvc.list({ page: 1, pageSize: 200 }).subscribe({
      next: (resp: PaginatedMatches) => {
        const filtered = (resp.items ?? []).filter(r => this.canPlay(r));
        this.rows.set(filtered);
      },
      error: () => this.rows.set([])
    });
  }

  canPlay(row: MatchListItem): boolean {
    const s = (row.status ?? '').toLowerCase().trim();
    const norm = s === 'programado' ? 'scheduled'
               : s === 'en juego'   ? 'live'
               : s;
    return norm === 'scheduled' || norm === 'live';
  }

  play(row: MatchListItem): void {
    if (!this.canPlay(row)) return;
    this.dialogRef.close({ id: row.id });
  }

  close(): void { this.dialogRef.close(undefined); }
}
