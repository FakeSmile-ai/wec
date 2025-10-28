import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthenticationService } from '@app/services/api/authentication.service'; // ✅ ruta corregida

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIf],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private auth = inject(AuthenticationService);
  private router = inject(Router);

  username = '';
  password = '';
  errorMessage = '';

  ngOnInit(): void {
    const token = this.auth.getToken();
    if (token) this.redirectAfterLogin();
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.auth.login(this.username, this.password).subscribe({
      next: (resp) => {
        this.auth.saveUser(resp);
        this.redirectAfterLogin();
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.message || 'Usuario o contraseña inválidos.';
      }
    });
  }

  onLoginWithGitHub(): void {
    this.auth.githubLoginRedirect();
  }

  private redirectAfterLogin(): void {
    if (this.auth.isAdmin()) {
      this.router.navigateByUrl('/admin');
    } else {
      this.router.navigateByUrl('/scoreboard/1');
    }
  }
}
