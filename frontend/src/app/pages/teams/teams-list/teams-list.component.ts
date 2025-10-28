import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { TeamService } from '../../../services/api/team.service';
import { Team } from '../../../models/team';
import { AuthenticationService } from '@app/services/api/authentication.service';

interface PageResponse<T> {
  content: T[];
  totalElements: number;
  number?: number;         // página actual (0-based)
  size?: number;           // tamaño de página
  totalPages?: number;
}

@Component({
  selector: 'app-teams-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './teams-list.html',
  styleUrls: ['./teams-list.scss']
})
export class TeamsListComponent implements OnInit {
  displayedColumns: string[] = ['id', 'name', 'coach', 'city', 'actions'];
  dataSource: Team[] = [];

  search = '';

  // Paginación (UI usa 1-based, Spring es 0-based; el service se encarga)
  page = 1;
  pageSize = 10;
  totalItems = 0;

  loading = false;
  errorMsg = '';

  constructor(
    private teamService: TeamService,
    private auth: AuthenticationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadTeams();
  }

  loadTeams() {
    this.loading = true;
    this.errorMsg = '';

    this.teamService.getTeams(this.page, this.pageSize, this.search)
      .subscribe({
        next: (res: any) => {
          // Soporta:
          // - { content, totalElements } (Spring Page)
          // - { items, totalCount } (API antigua)
          // - Team[] simple
          if (Array.isArray(res)) {
            this.dataSource = res as Team[];
            this.totalItems = this.dataSource.length;
          } else {
            const pageRes = (res as PageResponse<Team>);
            this.dataSource = (pageRes.content ?? (res.items as Team[])) ?? [];
            this.totalItems = (pageRes.totalElements ?? (res.totalCount as number)) ?? this.dataSource.length;

            // Si el backend devuelve number/size, sincroniza UI
            if (typeof pageRes.number === 'number') this.page = pageRes.number + 1;
            if (typeof pageRes.size === 'number') this.pageSize = pageRes.size;
          }
        },
        error: (err) => {
          console.error('Error cargando equipos', err);
          this.errorMsg = 'No se pudo cargar la lista de equipos.';
        },
        complete: () => { this.loading = false; }
      });
  }

  applyFilter() {
    this.page = 1;
    this.loadTeams();
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1; // MatPaginator es 0-based
    this.pageSize = event.pageSize;
    this.loadTeams();
  }

  deleteTeam(id: number) {
    if (!confirm('¿Eliminar equipo?')) return;
    this.teamService.delete(id).subscribe({
      next: () => this.loadTeams(),
      error: (err) => console.error('Error eliminando equipo', err)
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  // Opcional: trackBy para performance
  trackById = (_: number, item: Team) => item.id;
}
