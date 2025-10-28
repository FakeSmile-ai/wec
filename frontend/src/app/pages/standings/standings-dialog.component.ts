import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface StandingRow {
  id: number;
  name: string;
  color?: string;
  wins: number;
}

@Component({
  selector: 'app-standings-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './standings-dialog.component.html',
  styleUrls: ['./standings-dialog.component.scss']
})
export class StandingsDialogComponent {
  private dialogRef = inject(MatDialogRef<StandingsDialogComponent>);
  data = inject(MAT_DIALOG_DATA) as { rows: StandingRow[] };
  rows: StandingRow[] = [];

  constructor() {
    this.rows = (this.data?.rows ?? []).slice();
  }

  close(): void {
    this.dialogRef.close();
  }
}
