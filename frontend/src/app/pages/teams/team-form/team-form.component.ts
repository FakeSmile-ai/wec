import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

// Angular Material
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { TeamService } from '@app/services/api/team.service';
import { Team } from '../../../models/team';
import { AuthenticationService } from '@app/services/api/authentication.service';
import { Observable } from 'rxjs'; // 👈 necesario para tipar el observable

@Component({
  selector: 'app-team-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './team-form.html',
  styleUrls: ['./team-form.scss']
})
export class TeamFormComponent implements OnInit {
  // 👇 exactamente igual al backend
  team: Team = {
    id: 0,
    name: '',
    coach: '',
    city: ''
  };

  isEdit = false;

  constructor(
    private teamService: TeamService,
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthenticationService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.teamService.getById(+id).subscribe(t => {
        this.team = {
          id: t.id,
          name: t.name ?? '',
          coach: t.coach ?? '',
          city: t.city ?? ''
        };
      });
    }
  }

  save() {
    if (!this.team.name?.trim()) {
      alert('El nombre del equipo es obligatorio.');
      return;
    }

    const payload: Team = {
      id: this.team.id,
      name: this.team.name.trim(),
      coach: this.team.coach?.trim(),
      city: this.team.city?.trim()
    };

    // ✅ asegúrate de tipar correctamente el Observable
    const req$: Observable<Team> = this.isEdit
      ? this.teamService.update(this.team.id, payload)
      : this.teamService.create(payload);

    req$.subscribe({
      next: () => this.router.navigate(['/teams']),
      error: (err) => console.error('Error al guardar el equipo:', err)
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}