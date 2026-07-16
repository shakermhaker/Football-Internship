import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-business-register',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './business-register.component.html',
  styleUrl: './business-register.component.scss'
})
export class BusinessRegisterComponent {
  // Reactive Forms veya API istekleri için gerekli mantık buraya gelecek kanka
}