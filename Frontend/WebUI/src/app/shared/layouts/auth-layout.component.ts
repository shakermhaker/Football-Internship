import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router'; // <-- RouterLinkActive Eklendi!

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive], // <-- Buraya da eklendi!
  template: `
    <div class="d-flex flex-column min-vh-100" 
         style="background-image: url('/assets/images/ff-background-img.png'); background-size: cover; background-position: center; background-attachment: fixed; background-repeat: no-repeat;">
      
      <nav class="navbar d-flex justify-content-between align-items-center p-4 bg-transparent border-0">
        <a routerLink="/" class="fs-4 fw-bolder text-dark text-decoration-none" style="letter-spacing: -0.5px;">FootballField</a>
        
        <div class="menu d-none d-md-flex gap-4">
          <a routerLink="/" 
             routerLinkActive="border-bottom border-dark border-2" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="text-dark text-decoration-none fw-medium pb-1">Ana Sayfa</a>
             
          <a routerLink="/how-it-works" 
             routerLinkActive="border-bottom border-dark border-2" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="text-dark text-decoration-none fw-medium pb-1">Nasıl Çalışır</a>
             
          <a routerLink="/auth/register" 
             routerLinkActive="border-bottom border-dark border-2" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="text-dark text-decoration-none fw-medium pb-1">Üye Ol</a>
             
          <a routerLink="/about" 
             routerLinkActive="border-bottom border-dark border-2" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="text-dark text-decoration-none fw-medium pb-1">Hakkımızda</a>
        </div>
        
        <a routerLink="/auth/login" class="text-white text-decoration-none fw-bold" 
           style="background-color: #ff7d00; padding: 10px 24px; border-radius: 12px; box-shadow: 0 4px 10px rgba(255, 125, 0, 0.2);">
          Giriş Yap
        </a>
      </nav>

      <div class="d-flex flex-column flex-center flex-grow-1 p-5">
        <router-outlet></router-outlet> 
      </div>
      
    </div>
  `
})
export class AuthLayoutComponent {}