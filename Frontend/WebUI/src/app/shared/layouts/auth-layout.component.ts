import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet], // HTML içinde <router-outlet> kullanabilmek için ekledik
  template: `
    <div class="d-flex flex-column flex-root" style="background-color: #f7f7f7;">
      <div class="d-flex flex-column flex-center min-vh-100">
        <router-outlet></router-outlet>
      </div>
    </div>
  `
})
export class AuthLayoutComponent {}