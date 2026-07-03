import { Routes } from '@angular/router';
// Yeni kurduğumuz Shared katmanından Layout'u çağırıyoruz
import { AuthLayoutComponent } from './shared/layouts/auth-layout/auth-layout.component';
// Yeni kurduğumuz Features katmanından Register sayfasını çağırıyoruz
import { RegisterComponent } from './features/auth/register/register.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayoutComponent, // Çerçevemiz
    children: [
      { path: 'register', component: RegisterComponent }, // Çerçevenin içine basılacak sayfa
      { path: '', redirectTo: 'register', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: 'auth/register', pathMatch: 'full' }
];