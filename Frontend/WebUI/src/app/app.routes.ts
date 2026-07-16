import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';
import { HowItWorksComponent } from './features/how-it-works/how-it-works'; // <-- Yeni import
import { AboutComponent } from './features/about/about';           // <-- Yeni import

export const routes: Routes = [
  // 1. Auth Sayfaları Grubu
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { 
        path: 'register', 
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) 
      },
      { 
        path: 'login', 
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) 
      }
    ]
  },

  // 2. Ana Uygulama Sayfaları Grubu (MainLayout)
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { 
        path: 'how-it-works', 
        loadComponent: () => import('./features/how-it-works/how-it-works').then(m => m.HowItWorksComponent) 
      },
      { 
        path: 'about', 
        loadComponent: () => import('./features/about/about').then(m => m.AboutComponent) 
      }
    ]
  },

  // 3. Fallback: Bilinmeyen yolda login'e değil, ana sayfaya atalım
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];