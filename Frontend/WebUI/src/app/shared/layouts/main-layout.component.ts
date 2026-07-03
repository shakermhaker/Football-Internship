import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="main-layout-wrapper">
      <header class="p-4 shadow-sm bg-white">
        <div class="container d-flex justify-content-between align-items-center">
          <strong style="color: #FF385C; font-size: 24px;">airbnb</strong>
          <span class="text-muted">Profile Area</span>
        </div>
      </header>

      <main class="container my-5">
        <router-outlet></router-outlet>
      </main>

      <footer class="bg-light text-center py-4 border-top">
        <p class="mb-0 text-muted">&copy; 2026 Airbnb, Inc. All rights reserved.</p>
      </footer>
    </div>
  `
})
export class MainLayoutComponent {}