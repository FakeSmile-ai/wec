import { ApplicationConfig, APP_INITIALIZER, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';

import { provideClientHydration } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { authTokenInterceptor } from '@app/core/interceptors/auth-token.interceptor';
import { installStorageDebugging } from '@app/core/debug/debug-storage';

function initDebugFactory() {
  const platformId = inject(PLATFORM_ID);
  return () => {
    if (isPlatformBrowser(platformId)) {
      installStorageDebugging();
      console.log('[DEBUG] storage spy installed');
    }
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideClientHydration(),
    provideHttpClient(withFetch(), withInterceptors([authTokenInterceptor])),
    provideNoopAnimations(),
    { provide: APP_INITIALIZER, useFactory: initDebugFactory, deps: [], multi: true },
  ],
};
