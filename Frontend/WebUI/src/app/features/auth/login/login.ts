import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(1)]]
  });

  onSubmit() {
    if (this.loginForm.valid) {
      // Servisi çağırıp veriyi C# backend'e yolluyoruz
      this.authService.login(this.loginForm.value).subscribe({
        next: (response: any) => {
          console.log('Giriş Başarılı, .NET Cevabı:', response);
          
          // Eğer API'den bir JWT Token dönüyorsa, bunu tarayıcıya kaydedebiliriz:
          // localStorage.setItem('token', response.token); 
          
          // Giriş başarılı olunca kullanıcıyı ana sayfaya (sahalara) yönlendiriyoruz
          this.router.navigate(['/']);
        },
        error: (err) => {
          console.error('Giriş Hatası:', err);
          alert('Giriş başarısız! E-posta veya şifre hatalı olabilir.');
        }
      });
    }
  }
}