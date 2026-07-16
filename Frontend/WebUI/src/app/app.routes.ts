import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';

export const routes: Routes = [
  // 1. Auth Sayfaları Grubu (Lazy Loading ile)
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

  // 2. Ana Uygulama Sayfaları Grubu
  {
    path: '',
    component: MainLayoutComponent,
    children: [
       // Sahalar buraya eklenecek
    ]
  },
   {
    path: 'user',
    component: MainLayoutComponent,
    children: [
      {
        path: 'profile',
        loadComponent: () => import('./features/user/profile/profile.component').then(m => m.ProfileComponent)
      }

      
    ]
  },
  // 3. Fallback/Catch-All Rota
  {
    path: '**',
    redirectTo: 'auth/login',
    pathMatch: 'full'
  }
];