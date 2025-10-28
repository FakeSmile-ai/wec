import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '@app/services/api/authentication.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const platformId = inject(PLATFORM_ID);
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  if (!isPlatformBrowser(platformId)) return true;

  const token = auth.getToken();
  if (token) return true; // deja pasar, el backend validará

  // Solo redirige. ❌ NO hagas logout aquí.
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
