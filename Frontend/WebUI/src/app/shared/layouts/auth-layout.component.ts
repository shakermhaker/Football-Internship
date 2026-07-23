import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router'; // <-- RouterLinkActive Eklendi!

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive], // <-- Buraya da eklendi!
  styles: [`
    /* Sabit, seçili olmayan link stili */
    .ff-nav-link {
      color: #7e8299; /* Metronic text-gray-600 */
      font-weight: 500; /* Medium - Çok kalın durmasın */
      padding: 0.65rem 1.5rem;
      border-radius: 50px;
      text-decoration: none;
      transition: all 0.3s ease;
    }

    /* Üzerine gelindiğinde (Hover) */
    .ff-nav-link:hover {
      color: #198754; /* Metronic text-success */
    }

    /* 🚀 SEÇİLİ OLDUĞUNDA (Active Pill Efekti) */
    .ff-nav-link.is-active {
      background-color: #e8fff3 !important; /* Metronic bg-light-success karşılığı */
      color: #198754 !important; /* text-success */
      font-weight: 700; /* Seçiliyken BOLD olsun */
      box-shadow: 0 2px 6px rgba(25, 135, 84, 0.15); /* Hafif gölge ile havaya kalksın */
    }
  `],
  template: `
    <div class="d-flex flex-column min-vh-100" 
         style="background-image: url('/assets/images/ff-background-img.png'); background-size: cover; background-position: center; background-attachment: fixed; background-repeat: no-repeat;">
      <div class="fixed-top d-flex justify-content-center w-100" style="padding-top: 1.5rem; z-index: 1040; pointer-events: none;">
  
  <nav class="navbar d-flex justify-content-between align-items-center px-4 py-2 mx-auto"
       style="background: rgba(255, 255, 255, 0.85); 
              backdrop-filter: blur(16px); 
              -webkit-backdrop-filter: blur(16px);
              border: 1px solid rgba(40, 167, 69, 0.2); 
              border-radius: 50px; 
              box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08), 0 2px 8px rgba(40, 167, 69, 0.1);
              width: 90%; 
              max-width: 1200px;
              pointer-events: auto;
              transition: all 0.3s ease;">
        
        <!-- LOGO -->
        <a routerLink="/" class="fs-4 fw-bolder text-decoration-none d-flex align-items-center gap-2" style="color: #198754; letter-spacing: -0.5px;">
          <i class="ki-duotone ki-element-11 fs-2"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>
          FootballField
        </a>
        
        <!-- CENTER MENU -->
        <div class="menu d-none d-lg-flex gap-2 align-items-center">
          <!-- SADECE ff-nav-link CLASS'I KALDI, ROUTER LINK ACTIVE is-active OLDU -->
          <a routerLink="/" routerLinkActive="is-active" [routerLinkActiveOptions]="{exact: true}" class="ff-nav-link">Ana Sayfa</a>
          <a routerLink="/how-it-works" routerLinkActive="is-active" [routerLinkActiveOptions]="{exact: true}" class="ff-nav-link">Nasıl Çalışır</a>
          <a routerLink="/about" routerLinkActive="is-active" [routerLinkActiveOptions]="{exact: true}" class="ff-nav-link">Hakkımızda</a>
        </div>
        
        <!-- RIGHT ACTIONS (Giriş Yap Butonu) -->
        <div class="d-flex align-items-center gap-3">
            <a routerLink="/auth/login" class="btn btn-sm btn-success fw-bold rounded-pill px-5 shadow-sm d-flex align-items-center gap-2">
              <i class="ki-duotone ki-entrance-left fs-4"><span class="path1"></span><span class="path2"></span></i>
              Giriş Yap
            </a>
        </div>

  </nav>
</div>

      <div class="d-flex flex-column flex-center flex-grow-1 p-5">
        <router-outlet></router-outlet> 
      </div>
      
    </div>
  `
})
export class AuthLayoutComponent {}