// src/app/core/guards/admin.guard.ts
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '@app/services/api/authentication.service';

export const adminGuard: CanActivateFn = (_route, state) => {
  const platformId = inject(PLATFORM_ID);
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (!isPlatformBrowser(platformId)) return true;
  if (auth.isAdmin()) return true;

  // Solo redirige; NO borres storage
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
