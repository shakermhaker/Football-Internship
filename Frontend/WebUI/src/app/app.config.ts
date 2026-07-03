import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';

// PrimeNG 18+ Yeni Tema ve Animasyon Ayarları
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura'; // Yeni nesil modern Aura teması

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideAnimationsAsync(), // PrimeNG input/buton animasyonları için şart
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};