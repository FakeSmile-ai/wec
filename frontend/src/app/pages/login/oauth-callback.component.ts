import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '@app/services/api/authentication.service';

@Component({
  selector: 'app-oauth-callback',
  standalone: true,
  template: `<p>Procesando login con GitHub...</p>`
})
export class OAuthCallbackComponent implements OnInit {
  private router = inject(Router);
  private auth = inject(AuthenticationService);

  ngOnInit(): void {
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');

    if (token) {
      this.auth.saveFromToken(token);
      this.router.navigateByUrl(this.auth.isAdmin() ? '/admin' : '/scoreboard/1');
    } else {
      console.error('No se encontró el token en la URL');
      this.router.navigateByUrl('/login');
    }
  }
}
