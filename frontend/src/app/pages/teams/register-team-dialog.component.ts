import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-register-team-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './register-team-dialog.component.html',
  styleUrls: ['./register-team-dialog.component.scss']
})
export class RegisterTeamDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<RegisterTeamDialogComponent, any>);

  // Form principal
  form = this.fb.group({
    name: ['', Validators.required],
    color: ['#2e7dff'],
    players: this.fb.array([])
  });

  // Subform para agregar jugadores
  newPlayer = this.fb.group({
    number: [null as number | null],
    name: ['', Validators.required]
  });

  get players(): FormArray {
    return this.form.get('players') as FormArray;
  }

  addPlayer(): void {
    const { number, name } = this.newPlayer.value;
    if (!name || name.trim() === '') return;

    this.players.push(
      this.fb.group({
        number: [number ?? null],
        name: [name.trim(), Validators.required]
      })
    );

    this.newPlayer.reset({ number: null, name: '' });
  }

  removePlayer(index: number): void {
    this.players.removeAt(index);
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(): void {
    if (this.form.invalid) return;

    const { name, color, players } = this.form.value;
    const formattedPlayers = (players ?? []).map((p: any) => ({
      number: p.number != null && p.number !== '' ? Number(p.number) : undefined,
      name: String(p.name).trim()
    }));

    this.dialogRef.close({
      name: name?.trim(),
      color: color?.trim() || undefined,
      players: formattedPlayers
    });
  }
}
