import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';
import { HowItWorksComponent } from './features/how-it-works/how-it-works.component'; // <-- Yeni import
import { AboutComponent } from './features/about/about.component';           // <-- Yeni import
import { BusinessLayoutComponent } from './shared/layouts/business-layout.component';
import { HomeComponent } from './features/home/home.component';
import { FootballfieldsComponent } from './features/footballfields/footballfields.component';
import { MyFieldsComponent } from './features/business/my-fields/my-fields.component';
import { AddFieldComponent } from './features/business/add-field/add-field.component';
import { BusinessMainpanelComponent } from './features/business/business-mainpanel/business-mainpanel.component';
import { ReservationComponent } from './features/reservation/reservation.component'; 
import { MyReservationsComponent } from './features/user/user-reservation/my-reservations.component'; 
import {BusinessAnalyticsComponent} from './features/business/business-analytics/business-analytics.component'

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
      },
      {
        path: 'my-fields/edit/:id', 
        component: AddFieldComponent // DİKKAT: Yeni sayfa yapmadık, yine aynı sayfaya yolladık!
      },
      {
        path: 'mainpanel', // DİKKAT: Sadece mainpanel bıraktık
        loadComponent: () => import('./features/business/business-mainpanel/business-mainpanel.component').then(m => m.BusinessMainpanelComponent)
      },
      {
        path: 'analytics',
        component: BusinessAnalyticsComponent
      },
      {
        path: 'reservations', // DİKKAT: Sadece mainpanel bıraktık
        loadComponent: () => import('./features/business/business-reservations/business-reservations.component').then(m => m.BusinessReservationsComponent)
      },
    ]
  },

  // 2. Ana Uygulama Sayfaları Grubu (MainLayout)
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { 
        path: 'how-it-works', 
        loadComponent: () => import('./features/how-it-works/how-it-works.component').then(m => m.HowItWorksComponent) 
      },
      { 
        path: 'about', 
        loadComponent: () => import('./features/about/about.component').then(m => m.AboutComponent) 
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
      },
      { 
        path: 'my-reservations', 
        component: MyReservationsComponent, 
        
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
      },
      {
        path: ':id/schedules',
        loadComponent: () => import('./features/reservation/reservation.component').then(m => m.ReservationComponent)
      }

      
    ]
  },
  // 3. Fallback/Catch-All Rota
  // 3. Fallback: Bilinmeyen yolda login'e değil, ana sayfaya atalım
 
];