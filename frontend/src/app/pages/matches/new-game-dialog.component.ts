import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

// 🔄 Nueva ubicación del servicio API
import { ApiService } from '../../services/api/api.service';

@Component({
  selector: 'app-new-game-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './new-game-dialog.component.html',
  styleUrls: ['./new-game-dialog.component.scss']
})
export class NewGameDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private dialogRef = inject(MatDialogRef<NewGameDialogComponent, any>);

  teams: Array<{ id: number; name: string }> = [];
  loading = true;

  form = this.fb.group({
    homeTeamId: [null as number | null, Validators.required],
    awayTeamId: [null as number | null, Validators.required],
    minutes: [10, [Validators.required, Validators.min(1)]]
  });

  ngOnInit() {
    this.api.listTeams().subscribe({
      next: (teams: any[]) => {
        this.teams = teams.sort((a, b) => a.name.localeCompare(b.name));
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  swap() {
    const home = this.form.value.homeTeamId;
    const away = this.form.value.awayTeamId;
    this.form.patchValue({ homeTeamId: away, awayTeamId: home });
  }

  cancel() {
    this.dialogRef.close();
  }

  save() {
    if (this.form.invalid) return;
    const { homeTeamId, awayTeamId, minutes } = this.form.value;
    if (homeTeamId === awayTeamId) return;

    this.dialogRef.close({
      homeTeamId,
      awayTeamId,
      quarterDurationSeconds: Math.round(Number(minutes) * 60)
    });
  }
}
