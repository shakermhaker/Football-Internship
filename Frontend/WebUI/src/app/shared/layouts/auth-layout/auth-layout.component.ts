import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet], // Sadece router-outlet kullanacağımız için bunu import ediyoruz
  template: `
    <div class="auth-container" style="min-height: 100vh; display: flex; justify-content: center; align-items: center; background-color: #f4f6f9;">
      
      <div style="background: white; padding: 2.5rem; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.05); width: 100%; max-width: 450px;">
        
        <router-outlet></router-outlet>
        
      </div>

    </div>
  `,
  styles: [`
    /* İleride sadece bu layout'a özel CSS yazmak istersen buraya yazabilirsin */
  `]
})
export class AuthLayoutComponent {
  // Layout'un TypeScript mantığı gerekirse buraya gelecek
}