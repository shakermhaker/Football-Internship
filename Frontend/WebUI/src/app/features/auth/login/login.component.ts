import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterModule
  ],
  templateUrl: './login.component.html',
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
            Swal.fire({
            title: 'Giriş Başarılı!',
            text: 'Halı saha paneline yönlendiriliyorsunuz...',
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
          });
          console.log('Giriş Başarılı, .NET Cevabı:', response);
          
          // Eğer API'den bir JWT Token dönüyorsa, bunu tarayıcıya kaydedebiliriz:
          // localStorage.setItem('token', response.token); 
          
          // Giriş başarılı olunca kullanıcıyı ana sayfaya (sahalara) yönlendiriyoruz
          this.router.navigate(['/']);
        },
        error: (err) => {
          const errorBody = err.error;
          const msg = errorBody?.message || errorBody?.Message || '';

          // 🎯 SENARYO A: E-POSTA DOĞRULANMAMIŞSA ÖZEL SWEETALERT
          if (msg === 'Lütfen e-posta adresinizi doğrulayın.') {
            Swal.fire({
              title: 'E-posta Doğrulanmamış!',
              text: `${this.loginForm.value.email} adresine gönderdiğimiz linke tıklayarak hesabınızı aktifleştirmeniz gerekiyor.`,
              icon: 'warning',
              showCancelButton: true,
              confirmButtonColor: '#ffc107', // Metronic warning (Sarı)
              cancelButtonColor: '#d33',     // Kırmızı
              confirmButtonText: '📧 Linki Tekrar Gönder',
              cancelButtonText: 'Kapat'
            }).then((result) => {
              
              if (result.isConfirmed) {
                this.resendVerificationEmail(this.loginForm.value.email);
              }
            });
          } 
          // 🎯 SENARYO B: ŞİFRE YANLIŞ VEYA KULLANICI YOKSA STANDART HATA POP-UP'I
          else {
            Swal.fire({
              title: 'Giriş Başarılı Olmadı',
              text: msg || 'E-posta adresiniz veya şifreniz hatalı.',
              icon: 'error',
              confirmButtonColor: '#f1416c', // Metronic danger (Kırmızı)
              confirmButtonText: 'Tekrar Dene'
            });
          }
        }
      });
    }
  }
  // 🌟 BONUS: Tekrar Gönder Butonuna Basıldığında Çalışacak SweetAlert Akışı
  resendVerificationEmail(email: string) {
    // 1. Kullanıcıya mailin gönderildiğini belirten yükleniyor animasyonu açıyoruz
    Swal.fire({
      title: 'Gönderiliyor...',
      text: 'Lütfen bekleyin, yeni doğrulama maili hazırlanıyor.',
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });

    // 2. Servis isteğini atıyoruz
    this.authService.sendVerificationEmail(email).subscribe({
      next: () => {
        // 3. Başarılı olursa yeşil onay animasyonlu pop-up basıyoruz!
        Swal.fire({
          title: 'Harika!',
          text: '📧 Yeni doğrulama bağlantısı e-posta adresinize başarıyla gönderildi. Lütfen gelen kutunuzu kontrol edin.',
          icon: 'success',
          confirmButtonColor: '#50cd89', // Metronic success (Yeşil)
          confirmButtonText: 'Anladım'
        });
      },
      error: () => {
        // Hata olursa kırmızı uyarı
        Swal.fire({
          title: 'Hata Oluştu!',
          text: 'Mail gönderilirken bir sorun oluştu, lütfen daha sonra tekrar deneyin.',
          icon: 'error',
          confirmButtonColor: '#f1416c',
          confirmButtonText: 'Tamam'
        });
      }
    });
  }
 
}