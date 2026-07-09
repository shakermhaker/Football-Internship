import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <div class="d-flex flex-column min-vh-100" style="background-color: #121417;">
      
      <nav class="navbar d-flex justify-content-between p-4" style="background-color: #1a1d21; border-bottom: 1px solid #333;">
        <div class="brand text-white fs-3 fw-bold">FootballField</div>
        
        <div class="menu">
          <a routerLink="/" class="text-white text-decoration-none mx-3 hover-turuncu">Ana Sayfa</a>
          <a routerLink="/nasil-calisir" class="text-white text-decoration-none mx-3 hover-turuncu">Nasıl Çalışır</a>
          <a routerLink="/auth/register" class="text-white text-decoration-none mx-3 hover-turuncu">Üye Ol</a>
          <a routerLink="/hakkimizda" class="text-white text-decoration-none mx-3 hover-turuncu">Hakkımızda</a>
        </div>
        <a routerLink="/auth/login" class="btn" style="background-color: #ff7d00; color: white;">Giriş Yap</a>
      </nav>

      <div class="d-flex flex-column flex-grow-1 p-5 text-white">
        <router-outlet></router-outlet>
      </div>

      <footer class="text-center p-4" style="color: #6c757d;">
        © 2026 FootballField, Inc. All rights reserved.
      </footer>
      
    </div>
  `,
  styles: [`
    /* Logonun beyaz kalması için */
    .brand { color: #ffffff !important; text-decoration: none; }
    
    /* İŞTE ÇÖZÜM: Linklerin varsayılan lacivertini bembeyaz yapıp eziyoruz */
    .menu a { color: #ffffff !important; text-decoration: none; transition: 0.3s; }
    
    /* Üzerine gelince turuncu olması için */
    .menu a:hover { color: #ff7d00 !important; }
  `]
})
export class MainLayoutComponent {}