import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common'; 
import { AuthService } from '../../core/services/auth.service'; 
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
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
          
          <!-- 🎯 ÖZEL SINIFIMIZ EKLENDİ: Sadece 'ff-nav-link' class'ı var, karmaşa yok! -->
          <a routerLink="/" 
             routerLinkActive="is-active" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="ff-nav-link">Ana Sayfa</a>
             
          <a routerLink="/how-it-works" 
             routerLinkActive="is-active" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="ff-nav-link">Nasıl Çalışır</a>
             
        
             
          <a routerLink="/about" 
             routerLinkActive="is-active" 
             [routerLinkActiveOptions]="{exact: true}" 
             class="ff-nav-link">Hakkımızda</a>
        </div>
        
        <!-- RIGHT ACTIONS -->
        <div class="d-flex align-items-center gap-3">
            
            <!-- GİRİŞ YAPMIŞ KULLANICILAR İÇİN İŞLETME BUTONLARI -->
            <ng-container *ngIf="authService.isLoggedIn$ | async">
                
                <!-- DURUM 1: İşletmesi HİÇ YOK -->
                <a *ngIf="!user()?.hasBusiness" 
                   routerLink="/business/business-register" 
                   class="btn btn-sm btn-light-success fw-bold rounded-pill px-4 d-none d-md-flex align-items-center gap-2">
                  <i class="ki-duotone ki-shop fs-4"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span><span class="path5"></span></i>
                  İşletme Hesabı Ol
                </a>

                <!-- DURUM 2: İşletme Talebi Gönderilmiş Ama ONAYLANMAMIŞ -->
                <button *ngIf="user()?.hasBusiness && !user()?.isBusinessApproved" 
                   class="btn btn-sm btn-light-secondary fw-bold rounded-pill px-4 d-none d-md-flex align-items-center gap-2 cursor-default opacity-75" disabled>
                  <i class="ki-duotone ki-time fs-4"><span class="path1"></span><span class="path2"></span></i>
                  Onay Bekleniyor
                </button>

                <!-- DURUM 3: ONAYLI İŞLETME -->
                <a *ngIf="user()?.hasBusiness && user()?.isBusinessApproved" 
                    routerLink="/business-panel"
                    class="btn btn-sm btn-success fw-bold rounded-pill px-4 d-none d-md-flex align-items-center gap-2 shadow-sm">
                  <i class="ki-duotone ki-chart-simple fs-4"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>
                  İşletme Paneli
                </a>

            </ng-container>

            <!-- KULLANICI PROFİL MENÜSÜ -->
            <div *ngIf="authService.isLoggedIn$ | async" class="position-relative d-flex align-items-center">
                
                <div (click)="isMenuOpen = !isMenuOpen" class="d-flex align-items-center p-1 rounded-pill bg-light-success border border-success border-opacity-25" style="cursor: pointer; transition: all 0.2s;">
                     
                     <div class="d-flex justify-content-center align-items-center bg-success rounded-circle shadow-sm overflow-hidden" style="width: 32px; height: 32px;">
                         @if (user()?.avatarPath) {
                              <img [src]="user()?.avatarPath" class="w-100 h-100 object-fit-cover" alt="User Avatar">
                          } @else {
                              <span class="text-white fs-7 fw-bold">{{ user()?.firstName?.charAt(0) || 'U' }}</span>
                          }
                     </div>
                     <span class="mx-3 fw-bold text-success fs-7">
                        {{ user()?.firstName }}
                     </span>
                     <i class="ki-duotone ki-down fs-5 me-2 text-success"><span class="path1"></span><span class="path2"></span></i>
                </div>

                <div *ngIf="isMenuOpen" 
                     class="menu menu-column menu-rounded menu-gray-800 menu-state-bg fw-bold py-4 fs-6 w-200px shadow-lg" 
                     style="position: absolute; right: 0; top: calc(100% + 10px); background-color: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px); z-index: 1050; border: 1px solid rgba(40, 167, 69, 0.1); border-radius: 16px;">
                    
                    <div class="menu-item px-5">
                        <a (click)="isMenuOpen = false" routerLink="/user/profile" class="menu-link px-5 text-dark text-hover-success text-decoration-none d-flex align-items-center gap-2 py-2">
                          <i class="ki-duotone ki-user fs-4"><span class="path1"></span><span class="path2"></span></i>
                          Profilim
                        </a>
                    </div>
                    
                    <div class="separator my-2 border-gray-200"></div>

                    <div class="menu-item px-5">
                        <a (click)="logout()" class="menu-link px-5 text-danger text-hover-danger text-decoration-none d-flex align-items-center gap-2 py-2" style="cursor: pointer;">
                          <i class="ki-duotone ki-exit-right fs-4"><span class="path1"></span><span class="path2"></span></i>
                          Çıkış Yap
                        </a>
                    </div>
                </div>
            </div>

            <!-- GİRİŞ YAPMAMIŞ KULLANICILAR İÇİN GİRİŞ BUTONU -->
            <a *ngIf="!(authService.isLoggedIn$ | async)" routerLink="/auth/login" class="btn btn-sm btn-success fw-bold rounded-pill px-5 shadow-sm d-flex align-items-center gap-2">
              <i class="ki-duotone ki-entrance-left fs-4"><span class="path1"></span><span class="path2"></span></i>
              Giriş Yap
            </a>
        </div>

  </nav>
</div>

      <div class="d-flex flex-column flex-grow-1 px-5 pb-5 pt-20 pl-10"  style="margin-top: 80px;">
        <router-outlet></router-outlet>
      </div>

      <footer class="text-center p-4 text-dark fw-medium">
        © 2026 FootballField, Inc. All rights reserved.
      </footer>
      
    </div>
  `
})
export class MainLayoutComponent implements OnInit {
  authService = inject(AuthService);
  private userService = inject(UserService);
  private router = inject(Router);
  
  isMenuOpen = false; 
  user = this.userService.currentUser;

  logout() {
    this.authService.logout();
    this.isMenuOpen = false;
  }

  // MainLayoutComponent veya ilgili component içinde
ngOnInit() {
    // Cookie kullanıldığı için Angular tarafında cookie'nin varlığını "doğrudan" bilemeyiz.
    // O yüzden isteği atıyoruz. Eğer ziyaretçiyse arka plan 400 dönecek.
    this.userService.fetchMyProfile().subscribe({
      next: (profile) => {
        // Zaten giriş yapmış, profil verisi signal'e doldu.
      },
      error: (err) => {
        // 🚀 ZİYARETÇİ SENARYOSU: Hata fırlatma, loglama yapma ve login'e YÖNLENDİRME!
        // Sadece currentUser signal'ini null'da tutmasını sağla.
        this.userService.clearUser(); 
      }
    });
  }
}
