import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';
import { HowItWorksComponent } from './features/how-it-works/how-it-works'; // <-- Yeni import
import { AboutComponent } from './features/about/about';           // <-- Yeni import
import { BusinessLayoutComponent } from './shared/layouts/business-layout.component';
import { HomeComponent } from './features/home/home.component';
import { FootballfieldsComponent } from './features/footballfields/footballfields.component';
import { MyFieldsComponent } from './features/business/my-fields/my-fields.component';
import { AddFieldComponent } from './features/business/add-field/add-field.component';

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
  {
    path: 'business-panel',
    component: BusinessLayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'my-fields',
        loadComponent: () => import('./features/business/my-fields/my-fields.component').then(m => m.MyFieldsComponent)
      },
      {
        path: 'my-fields/add',
        component: AddFieldComponent
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
      },
      { 
        path: '', // Boş path = Ana sayfa
        component: HomeComponent 
      },
      { path: '', component: HomeComponent },
      // Yeni rotamızı buraya ekledik:
      { path: 'fields', component: FootballfieldsComponent },
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
     {
    path: 'business',
    component: MainLayoutComponent,
    children: [
      {
        path: 'business-register',
        loadComponent: () => import('./features/business/business-register/business-register.component').then(m => m.BusinessRegisterComponent)
      }

      
    ]
  },
  // 3. Fallback/Catch-All Rota
  // 3. Fallback: Bilinmeyen yolda login'e değil, ana sayfaya atalım
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full'
  }
];