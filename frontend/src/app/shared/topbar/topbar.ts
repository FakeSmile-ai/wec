import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { AuthenticationService } from '@app/services/api/authentication.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, MatButtonModule], // 👈 RouterLink fuera (no se usa)
  templateUrl: './topbar.html',
  styleUrls: ['./topbar.css']
})
export class TopbarComponent {
  @Input() showAdmin = false;
  @Input() showControl = false;
  @Input() showScore = false;
  @Input() showLogout = false;

  @Input() controlId?: number | string | null;
  @Input() scoreId?: number | string | null;

  constructor(
    private router: Router,
    private auth: AuthenticationService
  ) {}

  go(path: string) {
    this.router.navigateByUrl(path);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
