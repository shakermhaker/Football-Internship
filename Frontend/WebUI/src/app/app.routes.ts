import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './shared/layouts/auth-layout.component';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';
import { RegisterComponent } from './features/auth/register/register.component';

export const routes: Routes = [
  // 1. Auth Sayfaları Grubu (Navbar/Footer istemediğimiz sayfalar)
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: 'register', component: RegisterComponent },
      // Later we will add login here:
      // { path: 'login', component: LoginComponent }
    ]
  },

  // 2. Ana Uygulama Sayfaları Grubu (Airbnb Navbar/Footer olan sayfalar)
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      // Ana sayfaya gelindiğinde ev ilanlarını göstereceğiz (Şimdilik boş geçiyoruz)
      // { path: '', component: HomeHousesComponent },
      // { path: 'rooms/:id', component: HouseDetailComponent }
    ]
  },

  // 3. Fallback/Catch-All Rota (Kullanıcı saçma bir URL girerse auth/register'a yönlendir)
  {
    path: '**',
    redirectTo: 'auth/register',
    pathMatch: 'full'
  }
];
