import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '@app/services/api/enviroments';
import { tap } from 'rxjs/operators';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
  private baseUrl = environment.apiBaseUrl + '/auth';
  private currentUserSubject = new BehaviorSubject<any>(this.getUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  /** LOGIN normal (usuario/contraseña) */
  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, { username, password }).pipe(
      tap((resp: any) => {
        if (resp?.token) {
          localStorage.setItem('token', resp.token);
          localStorage.setItem('user', JSON.stringify(resp.user));
          this.currentUserSubject.next(resp.user);
        }
      })
    );
  }

  /** Inicia login con GitHub OAuth */
  githubLoginRedirect() {
    window.location.href = `${this.baseUrl}/github/login`;
  }

  /** Guardar usuario manualmente (ya lo hace el tap) */
  saveUser(resp: any) {
    if (resp?.token) {
      localStorage.setItem('token', resp.token);
      localStorage.setItem('user', JSON.stringify(resp.user));
      this.currentUserSubject.next(resp.user);
    }
  }

  /** Guarda usuario desde un token JWT OAuth */
  saveFromToken(token: string) {
    try {
      localStorage.setItem('token', token);
      const payload = JSON.parse(atob(token.split('.')[1]));
      localStorage.setItem('user', JSON.stringify(payload));
      this.currentUserSubject.next(payload);
    } catch (err) {
      console.error('Error procesando token OAuth:', err);
      localStorage.clear();
    }
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUser(): any {
    const u = localStorage.getItem('user');
    return u ? JSON.parse(u) : null;
  }

  isAdmin(): boolean {
    const u = this.getUser();
    return u?.role === 'Admin';
  }

  logout() {
    localStorage.clear();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }
}
