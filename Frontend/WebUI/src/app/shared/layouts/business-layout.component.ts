import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common'; 
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-business-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <div class="d-flex min-vh-100" style="background-color: #f4f6f9;">
      
      <!-- SOL MENÜ (SIDEBAR) -->
      <div class="bg-dark p-4 shadow-sm" style="width: 280px; z-index: 10;">
        
        <!-- MARKA YAZISI -->
        <a routerLink="/business-panel" class="d-flex align-items-center mb-4 text-decoration-none">
          <span class="fs-4 fw-bolder" style="color: var(--accent-orange);">
            FootballField <span class="fs-6 fw-normal ms-1" style="color: var(--text-light);">İşletme</span>
          </span>
        </a>
        
        <hr style="border-color: var(--text-muted);">
        
        <!-- MENÜ LİNKLERİ -->
        <ul class="nav nav-pills flex-column mb-auto mt-4 gap-2">
          <li class="nav-item">
            <a routerLink="/business-panel/dashboard" 
               routerLinkActive="active fw-bold" 
               [routerLinkActiveOptions]="{exact: true}"
               class="nav-link" 
               style="color: var(--text-light);">
              Ana Panel
            </a>
          </li>
          
          <!-- YENİ EKLENEN: HALISAHALARIM -->
          <li class="nav-item">
            <a routerLink="/business-panel/my-fields" 
               routerLinkActive="active fw-bold" 
               class="nav-link" 
               style="color: var(--text-light);">
              Halısahalarım
            </a>
          </li>

          <li class="nav-item">
            <a routerLink="/business-panel/reservations" 
               routerLinkActive="active fw-bold" 
               class="nav-link" 
               style="color: var(--text-light);">
              Rezervasyonlar
            </a>
          </li>
          <li class="nav-item">
            <a routerLink="/business-panel/settings" 
               routerLinkActive="active fw-bold" 
               class="nav-link" 
               style="color: var(--text-light);">
              Saha Ayarları
            </a>
          </li>
        </ul>
      </div>

      <!-- SAĞ İÇERİK ALANI -->
      <div class="d-flex flex-column flex-grow-1">
        
        <!-- ÜST BİLGİ ÇUBUĞU (HEADER) -->
        <header class="d-flex justify-content-between align-items-center p-4 bg-white shadow-sm" style="height: 80px;">
          <!-- SOL TARAF: İŞLETME ADI -->
          <div class="fs-4 fw-bold text-primary">
            {{ user()?.businessName || 'İşletme Bilgisi Yükleniyor...' }}
          </div>
          
          <!-- SAĞ TARAF: KULLANICIYA DÖNÜŞ BUTONU -->
          <div class="d-flex align-items-center gap-4">
            <span class="badge bg-success px-3 py-2 fs-6 rounded-pill shadow-sm">Yönetici Modu</span>
            
            <a routerLink="/" class="btn btn-outline-dark fw-bold d-flex align-items-center rounded-3">
              <i class="ki-duotone ki-exit-left fs-3 me-2"></i> Kullanıcı Hesabına Dön
            </a>
          </div>
        </header>

        <!-- ASIL İÇERİĞİN GÖSTERİLECEĞİ YER (ROUTER OUTLET) -->
        <main class="p-5 flex-grow-1">
          <router-outlet></router-outlet>
        </main>

      </div>
    </div>
  `
})
export class BusinessLayoutComponent {
  private userService = inject(UserService);
  user = this.userService.currentUser;
}