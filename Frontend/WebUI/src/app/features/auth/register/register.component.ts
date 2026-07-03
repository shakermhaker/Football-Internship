import { Component, inject } from '@angular/core'; // inject eklendi
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  
  // Modern DI yaklaşımı ile servisi çağırıyoruz (Constructor'a veda!)
  private authService = inject(AuthService);
  
  registerForm = new FormGroup({
    firstName: new FormControl('', [Validators.required, Validators.minLength(2)]),
    lastName: new FormControl('', [Validators.required, Validators.minLength(2)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)])
  });

  onSubmit() {
    if (this.registerForm.valid) {
      const payload = this.registerForm.value;
      
      this.authService.register(payload).subscribe({
        next: (response) => {
          console.log('Kayıt Başarılı, API\'den gelen cevap:', response);
        },
        error: (err) => {
          console.error('Kayıt işlemi sırasında hata oluştu:', err);
        }
      });
    } else {
      this.registerForm.markAllAsTouched(); 
    }
  }
}