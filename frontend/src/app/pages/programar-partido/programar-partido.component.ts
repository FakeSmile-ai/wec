import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import Swal from 'sweetalert2';

import { MatchesService } from '@app/services/api/matches.service';
import { TeamService } from '@app/services/api/team.service';
import { Team } from '@app/models/team';

@Component({
  selector: 'app-programar-partido',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './programar-partido.component.html',
  styleUrls: ['./programar-partido.component.scss']
})
export class ProgramarPartidoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly matchesService = inject(MatchesService);
  private readonly teamService = inject(TeamService);
  private readonly router = inject(Router);

  teams = signal<Team[]>([]);
  loadingTeams = signal(false);
  submitting = signal(false);

  form = this.fb.nonNullable.group({
    homeTeamId: [0, Validators.required],
    awayTeamId: [0, Validators.required],
    date: ['', Validators.required],
    time: ['', Validators.required],
    quarterDurationSeconds: [600, [Validators.min(60)]]
  });

  sameTeamSelected = computed(() => {
    const value = this.form.getRawValue();
    return value.homeTeamId > 0 && value.homeTeamId === value.awayTeamId;
  });

  ngOnInit(): void {
    this.loadTeams();
  }

  schedule(): void {
    if (this.form.invalid || this.sameTeamSelected()) {
      this.form.markAllAsTouched();
      Swal.fire({
        title: 'Datos incompletos',
        text: 'Selecciona equipos distintos e ingresa fecha y hora válidas.',
        icon: 'warning'
      });
      return;
    }

    const { homeTeamId, awayTeamId, date, time, quarterDurationSeconds } = this.form.getRawValue();
    this.submitting.set(true);
    this.matchesService.programMatch({
      homeTeamId,
      awayTeamId,
      date,
      time,
      quarterDurationSeconds
    }).subscribe({
      next: match => {
        this.submitting.set(false);
        Swal.fire({
          title: 'Partido programado',
          text: `${match.homeTeamName || 'Local'} vs ${match.awayTeamName || 'Visitante'}`,
          icon: 'success',
          confirmButtonText: 'Ir al control'
        }).then(result => {
          if (result.isConfirmed && match.id) {
            this.router.navigate(['/control', match.id]);
          }
        });
      },
      error: error => {
        this.submitting.set(false);
        Swal.fire({
          title: 'Error al programar',
          text: error?.error?.error ?? error?.message ?? 'Error desconocido',
          icon: 'error'
        });
      }
    });
  }

  private loadTeams(): void {
    this.loadingTeams.set(true);
    this.teamService.getAll().subscribe({
      next: teams => {
        this.loadingTeams.set(false);
        this.teams.set(teams ?? []);
      },
      error: error => {
        this.loadingTeams.set(false);
        console.error('Error cargando equipos', error);
        Swal.fire({
          title: 'No se pudieron cargar los equipos',
          text: error?.message ?? 'Intenta nuevamente más tarde.',
          icon: 'error'
        });
      }
    });
  }
}
