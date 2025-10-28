import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AuthenticationService } from '@app/services/api/authentication.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-admin-menu',
  standalone: true,
  imports: [CommonModule, MatButtonModule, AsyncPipe],
  templateUrl: './admin-menu.html',
  styleUrls: ['./admin-menu.css']
})
export class AdminMenuComponent {
  user$ = this.auth.currentUser$; // observable reactivo

  constructor(private auth: AuthenticationService, private router: Router) {}

  /** Muestra el menú solo si el usuario autenticado es admin */
  isAdmin(user: any): boolean {
    return user?.role?.toLowerCase() === 'admin';
  }

  goTo(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
