import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { Login } from './features/auth/login/login';
export const routes: Routes = [
  // 1. Auth Sayfaları Grubu
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: 'register', component: RegisterComponent },
      { path: 'login', component: Login } // 2. Buradaki yorum satırını kaldır ve aktif et
    ]
  },

  // 2. Ana Uygulama Sayfaları Grubu
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      // Ana sayfaya gelindiğinde sahaları göstereceğin component buraya gelecek
    ]
  },

  // 3. Fallback/Catch-All Rota
  {
    path: '**',
    redirectTo: 'auth/login', // Saçma bir URL girilirse register yerine login'e atmak daha mantıklı olabilir
    pathMatch: 'full'
  }
];