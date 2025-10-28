import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

// Angular Material
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { PlayerService } from '@app/services/api/player.service';
import { Player } from '@app/models/player';
import { AuthenticationService } from '@app/services/api/authentication.service';

@Component({
  selector: 'app-players-list',
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
  templateUrl: './players-list.html',
  styleUrls: ['./players-list.scss']
})
export class PlayersListComponent implements OnInit {
  // columnas del backend
  displayedColumns: string[] = ['id', 'name', 'age', 'position', 'teamId', 'actions'];

  // datasets
  private allPlayers: Player[] = [];
  dataSource: Player[] = [];

  // paginación
  totalItems = 0;
  page = 1;
  pageSize = 10;

  // filtros
  teamId: number | null = null;
  search = '';

  private playerService = inject(PlayerService);
  private auth = inject(AuthenticationService);
  private router = inject(Router);

  ngOnInit(): void {
    this.loadPlayers();
  }

  loadPlayers(): void {
    this.playerService.getAll().subscribe({
      next: (res) => {
        // ✅ El backend devuelve { items, totalCount }
        this.allPlayers = res.items ?? [];
        this.totalItems = res.totalCount ?? this.allPlayers.length;

        this.applyFilter();
      },
      error: (err) => console.error('Error cargando jugadores:', err)
    });
  }

  applyFilter(): void {
    let rows = [...this.allPlayers];

    if (this.teamId != null) {
      rows = rows.filter(p => p.teamId === this.teamId);
    }

    if (this.search?.trim()) {
      const q = this.search.toLowerCase();
      rows = rows.filter(p =>
        (p.name ?? '').toLowerCase().includes(q) ||
        String(p.age ?? '').includes(q) ||
        (p.position ?? '').toLowerCase().includes(q) ||
        String(p.teamId ?? '').includes(q)
      );
    }

    this.totalItems = rows.length;
    this.page = 1;
    this.applyPage(rows);
  }

  onPageChange(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.applyPage(this.getFilteredSnapshot());
  }

  private getFilteredSnapshot(): Player[] {
    let rows = [...this.allPlayers];

    if (this.teamId != null) {
      rows = rows.filter(p => p.teamId === this.teamId);
    }

    if (this.search?.trim()) {
      const q = this.search.toLowerCase();
      rows = rows.filter(p =>
        (p.name ?? '').toLowerCase().includes(q) ||
        String(p.age ?? '').includes(q) ||
        (p.position ?? '').toLowerCase().includes(q) ||
        String(p.teamId ?? '').includes(q)
      );
    }

    return rows;
  }

  private applyPage(rows: Player[]): void {
    const start = (this.page - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.dataSource = rows.slice(start, end);
  }

  deletePlayer(id: number): void {
    if (!confirm('¿Eliminar jugador?')) return;
    this.playerService.delete(id).subscribe({
      next: () => this.loadPlayers(),
      error: (err) => console.error('Error eliminando jugador:', err)
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  // --- KPIs (solo lectura; no afectan la lógica existente) ---
get totalPlayersKpi(): number {
  // total actual según filtros/paginación aplicados
  return this.totalItems ?? 0;
}

get activePlayersKpi(): number {
  // si no tienes un flag de "activo", usamos el total filtrado
  return this.getFilteredSnapshot().length;
}

get avgAgeKpi(): number {
  const rows = this.getFilteredSnapshot();
  if (!rows.length) return 0;
  const sum = rows.reduce((acc, p) => acc + (Number(p.age) || 0), 0);
  return Math.round(sum / rows.length);
}
}
