import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="d-flex flex-column min-vh-100" style="background-color: #121417; color: white;">
      
      <header class="p-4 text-center">
        <h1>FootballField</h1>
      </header>

      <div class="d-flex flex-column flex-center flex-grow-1">
        <router-outlet></router-outlet>
      </div>

      <footer class="p-4 text-center">
        © 2026 FootballField, Inc. All rights reserved.
      </footer>
      
    </div>
  `
})
export class AuthLayoutComponent {}