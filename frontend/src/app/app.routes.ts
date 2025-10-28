// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from '@app/guards/auth.guard';
import { adminGuard } from '@app/guards/admin.guard';

export const routes: Routes = [
  // Inicio
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  // --- Login ---
  {
    path: 'login',
    loadComponent: () =>
      import('@app/pages/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'oauth/callback',
    loadComponent: () =>
      import('@app/pages/login/oauth-callback.component').then(
        m => m.OAuthCallbackComponent
      ),
  },

  // --- Redirecciones ---
  { path: 'score', pathMatch: 'full', redirectTo: 'score/1' },
  { path: 'control', pathMatch: 'full', redirectTo: 'control/1' },

  // --- Admin Dashboard ---
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/admin/admin-dashboard.component').then(
        m => m.AdminDashboardComponent
      ),
  },

  // --- Reports ---
  {
    path: 'admin/reports',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/reports/reports-page.component').then(
        m => m.ReportsPage
      ),
  },

  // --- Scoreboard ---
  {
    path: 'score/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@app/pages/scoreboard/scoreboard.component').then(
        m => m.ScoreboardComponent
      ),
  },

  // --- Control Panel ---
  {
    path: 'control/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@app/pages/control/control-panel.component').then(
        m => m.ControlPanelComponent
      ),
  },

  // --- Players ---
  {
    path: 'players',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/players/players-list/players-list.component').then(
        m => m.PlayersListComponent
      ),
  },
  {
    path: 'players/create',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/players/player-form/player-form.component').then(
        m => m.PlayerFormComponent
      ),
  },
  {
    path: 'players/edit/:id',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/players/player-form/player-form.component').then(
        m => m.PlayerFormComponent
      ),
  },

  // --- Teams ---
  {
    path: 'teams',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/teams/teams-list/teams-list.component').then(
        m => m.TeamsListComponent
      ),
  },
  {
    path: 'teams/create',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/teams/team-form/team-form.component').then(
        m => m.TeamFormComponent
      ),
  },
  {
    path: 'teams/edit/:id',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/teams/team-form/team-form.component').then(
        m => m.TeamFormComponent
      ),
  },

  // --- Tournaments ---
  {
    path: 'tournaments',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('@app/pages/tournaments/tournaments.component').then(
        m => m.TournamentsComponent
      ),
  },

  // Fallback
  { path: '**', redirectTo: 'login' },
];
