import { Component, inject ,OnInit} from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common'; 
import { AuthService } from '../../core/services/auth.service'; 
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <div class="d-flex flex-column min-vh-100" 
         style="background-image: url('/assets/images/ff-background-img.png'); background-size: cover; background-position: center; background-attachment: fixed; background-repeat: no-repeat;">
      
      <nav class="navbar d-flex justify-content-between align-items-center p-4 bg-transparent border-0">
        
        <a routerLink="/" class="fs-4 fw-bolder text-dark text-decoration-none" style="letter-spacing: -0.5px;">FootballField</a>
        
        <div class="menu d-none d-md-flex gap-4">
          <a routerLink="/" routerLinkActive="border-bottom border-dark border-2" [routerLinkActiveOptions]="{exact: true}" class="text-dark text-decoration-none fw-medium pb-1">Ana Sayfa</a>
          <a routerLink="/how-it-works" routerLinkActive="border-bottom border-dark border-2" [routerLinkActiveOptions]="{exact: true}" class="text-dark text-decoration-none fw-medium pb-1">Nasıl Çalışır</a>
          <a routerLink="/auth/register" routerLinkActive="border-bottom border-dark border-2" [routerLinkActiveOptions]="{exact: true}" class="text-dark text-decoration-none fw-medium pb-1">Üye Ol</a>
          <a routerLink="/about" routerLinkActive="border-bottom border-dark border-2" [routerLinkActiveOptions]="{exact: true}" class="text-dark text-decoration-none fw-medium pb-1">Hakkımızda</a>
        </div>
        
        <div class="d-flex align-items-center gap-3">
            
            <a *ngIf="authService.isLoggedIn$ | async" 
               routerLink="/business/business-register" 
               class="text-white text-decoration-none fw-bold" 
               style="background-color: #28a745; padding: 10px 24px; border-radius: 12px; box-shadow: 0 4px 10px rgba(40, 167, 69, 0.3);">
              İşletme Hesabı Ol
            </a>

            <div *ngIf="authService.isLoggedIn$ | async" class="position-relative d-flex align-items-center">
                
                <div (click)="isMenuOpen = !isMenuOpen" class="d-flex align-items-center" style="cursor: pointer;">
                     <div class="d-flex justify-content-center align-items-center bg-warning rounded-circle shadow-sm" style="width: 35px; height: 35px;">
                         @if (user()?.avatarPath) {
                              <img [src]="user()?.avatarPath" class="w-100 h-100 object-fit-cover" style="border-radius: 50%;" alt="User Avatar">
                          } @else {
                              <span class="text-white fs-6 fw-bold">{{ user()?.firstName?.charAt(0) || 'U' }}</span>
                          }
                     </div>
                     <span class="ms-3 fw-bold text-dark">
                     {{ user()?.firstName }} {{ user()?.lastName }}
                      </span>
                </div>

                <div *ngIf="isMenuOpen" 
                     class="menu menu-column menu-rounded menu-gray-800 menu-state-bg fw-bold py-4 fs-6 w-200px shadow" 
                     style="position: absolute; right: 0; top: 130%; background-color: white; z-index: 1050; border: 1px solid #f4f4f4; border-radius: 8px;">
                    
                    <div class="menu-item px-5">
                        <a (click)="isMenuOpen = false" routerLink="/user/profile" class="menu-link px-5 text-dark text-decoration-none d-block py-2">Profilim</a>
                    </div>
                    
                    <div class="separator my-1 border-light"></div>

                    <div class="menu-item px-5">
                        <a (click)="logout()" class="menu-link px-5 text-danger text-decoration-none d-block py-2" style="cursor: pointer;">Çıkış Yap</a>
                    </div>
                </div>
            </div>

            <a *ngIf="!(authService.isLoggedIn$ | async)" routerLink="/auth/login" class="text-white text-decoration-none fw-bold" 
               style="background-color: #ff7d00; padding: 10px 24px; border-radius: 12px; box-shadow: 0 4px 10px rgba(255, 125, 0, 0.2);">
              Giriş Yap
            </a>
        </div>

      </nav>

      <div class="d-flex flex-column flex-grow-1 p-5">
        <router-outlet></router-outlet>
      </div>

      <footer class="text-center p-4 text-dark fw-medium">
        © 2026 FootballField, Inc. All rights reserved.
      </footer>
      
    </div>
  `
})
export class MainLayoutComponent implements OnInit{
  authService = inject(AuthService);
  private userService = inject(UserService);
  
  // Menünün açık/kapalı durumunu tutan Angular değişkeni
  isMenuOpen = false; 
  user = this.userService.currentUser;

  logout() {
    this.authService.logout();
    this.isMenuOpen = false;
  }
  ngOnInit() {
    // F5 atıldığında sinyal boşalacağı için veriyi API'den geri çekme güvencemiz:
    if (!this.user()) {
      this.userService.fetchMyProfile().subscribe({
        error: (err) => console.error('Layout profil verisini çekemedi:', err)
      });
    }
  }
}