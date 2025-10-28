import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

import { PlayerService } from '../../../services/api/player.service';
import { TeamService } from '../../../services/api/team.service';
import { Team } from '../../../models/team';
import { Player } from '../../../models/player';

@Component({
  selector: 'app-player-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatSelectModule,
    MatIconModule
  ],
  templateUrl: './player-form.component.html',
  styleUrls: ['./player-form.component.scss']
})
export class PlayerFormComponent implements OnInit {
  form!: FormGroup;
  id?: number;
  teams: Team[] = [];

  constructor(
    private fb: FormBuilder,
    private playerService: PlayerService,
    private teamService: TeamService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      age: [null, [Validators.required, Validators.min(1)]],
      position: [''],
      teamId: [null]
    });

    this.teamService.getTeams(1, 100).subscribe({
      next: (res: any) => (this.teams = res.content ?? res.items ?? []),
      error: (err) => console.error('Error cargando equipos:', err)
    });

    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      this.playerService.getById(this.id).subscribe({
        next: (p) => this.form.patchValue(p),
        error: (err) => console.error('Error cargando jugador:', err)
      });
    }
  }

  save(): void {
    if (this.form.invalid) return;
    const payload: Player = this.form.value;
    const done = () => this.router.navigate(['/players']);

    if (this.id) {
      this.playerService.update(this.id, payload).subscribe({ next: done });
    } else {
      this.playerService.create(payload).subscribe({ next: done });
    }
  }
}
