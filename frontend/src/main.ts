console.log('Zone present?', (globalThis as any).Zone?.version);
import { bootstrapApplication } from '@angular/platform-browser';
import { importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';

console.log('>> Bootstrapping Angular app …');

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    importProvidersFrom(HttpClientModule)
  ]
})
  .then(appRef => {
    console.log('>> Angular app bootstrapped successfully', appRef);
  })
  .catch(err => {
    console.error('>> Bootstrapping Angular app FAILED', err);
  });
