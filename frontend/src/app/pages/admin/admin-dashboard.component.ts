import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, ViewChild, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '@app/services/api/authentication.service';
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink,MatIconModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent {
  private auth = inject(AuthenticationService);
  private router = inject(Router);

  /** ID de partido por defecto para los accesos rápidos */
  matchId = 1;

  /** Estado del menú de logout */
  logoutOpen = false;

  @ViewChild('logoutWrap') logoutWrap?: ElementRef<HTMLElement>;

  /** Alterna el menú de logout */
  toggleLogout(event?: MouseEvent): void {
    event?.stopPropagation();
    this.logoutOpen = !this.logoutOpen;
  }

  /** Cierra el menú si se hace clic fuera del área */
  @HostListener('document:click', ['$event'])
  closeIfClickOutside(event: MouseEvent): void {
    if (!this.logoutOpen || !this.logoutWrap) return;
    if (!this.logoutWrap.nativeElement.contains(event.target as Node)) {
      this.logoutOpen = false;
    }
  }

  /** Cierra sesión y redirige al login */
  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  /** Verifica si el usuario es administrador */
  get isAdmin(): boolean {
    try {
      return this.auth.isAdmin();
    } catch {
      return false;
    }
  }
}
