import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router'; // Yönlendirme için bunu ekledik
import { AuthService } from '../../../Core/services/auth.service'

declare var Swal: any;

@Component({
  selector: 'app-register',
  standalone: true, // Kendi kendine yeten modern bağımsız bileşen
  imports: [CommonModule, ReactiveFormsModule], // HTML'de form elementleri ve *ngIf kullanabilmek için şart
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  // 1. DEPENDENCY INJECTION: "new FormBuilder()" falan demiyoruz.
  // Angular'ın hazır yönetim araçlarını inject() fonksiyonu ile içeri enjekte ediyoruz.
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  private router = inject(Router);

  // HTML tarafında [formGroup]="registerForm" diyerek çağırdığımız ana form nesnesi
  registerForm!: FormGroup;
  isPasswordVisible: boolean = false;
  isConfirmPasswordVisible: boolean = false;

  
    
  ngOnInit(): void {
    // 2. FORM ŞEMASI KURULUMU: Bileşen ilk açıldığında tam senin istediğin 5 alanı ve kurallarını tanımlıyoruz
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],                             // Boş geçilemez
      lastName: ['', [Validators.required]],                              // Boş geçilemez
      email: ['', [Validators.required, Validators.email]],               // Boş geçilemez ve Email formatında olmalı
      password: ['', [Validators.required, Validators.minLength(8)]],     // Boş geçilemez ve en az 8 karakter olmalı
      confirmPassword: ['', [Validators.required]],
      phone: ['', [Validators.required, Validators.minLength(10)]]                            // Boş geçilemez
    }, {
      // Neden buraya yazdık? 
      // Tek bir inputun değil, formun iki farklı inputunun birbiriyle kıyaslanması için kuralı tüm gruba (validators) bağlıyoruz.
      validators: this.passwordMatchValidator 
    });
  }

  togglePasswordVisibility(): void {
  // Mevcut durumu tersine çeviriyoruz (true ise false, false ise true yapar)
  this.isPasswordVisible = !this.isPasswordVisible;
}

toggleConfirmPasswordVisibility(): void {
  this.isConfirmPasswordVisible = !this.isConfirmPasswordVisible;
}

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    // Eğer şifreler girilmişse ve birbirine eşit DEĞİLSE hata nesnesi dön
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true }; // HTML tarafında hasError('passwordMismatch') ile yakalayacağımız isim
    }

    // Her şey yolundaysa (eşleşiyorsa) null dön, yani hata yok demektir
    return null;
  }

  onPhoneNumberInput(event: any): void {
    const input = event.target;
    // Regex Açıklaması: [^0-9 ] ifadesi "sayı ve boşluk DIŞINDAKİ her şey" demektir.
    // .replace(/[^0-9 ]/g, '') -> Sayı ve boşluk dışındaki tüm karakterleri siler (boş string ile değiştirir).
    const sanitizedValue = input.value.replace(/[^0-9 ]/g, '');
    
    // Temizlenmiş değeri hem inputun kendisine hem de Angular form kontrolüne geri yazıyoruz
    input.value = sanitizedValue;
    this.registerForm.get('phone')?.setValue(sanitizedValue, { emitEvent: false });
  }

  // Kullanıcı "Kayıt Ol" butonuna bastığında tetiklenecek fonksiyon
onRegisterSubmit(): void {
    if (this.registerForm.valid) {
      const { confirmPassword, ...signUpData } = this.registerForm.value;
      
      if (signUpData.phoneNumber) {
        signUpData.phoneNumber = signUpData.phoneNumber.replace(/\s+/g, '');
      }

      // Backend API'ye istek atıyoruz
      this.authService.register(signUpData).subscribe({
        next: (response) => {
          console.log('Registration success:', response);

          // 3. SWEETALERT VE REDIRECT AKIŞI
          // Metronic'in entegre SweetAlert kütüphanesini tetikliyoruz
          Swal.fire({
            text: "Registration successful! Please check your email to verify your account.",
            icon: "success",
            buttonsStyling: false,
            confirmButtonText: "Ok, got it!",
            customClass: {
              confirmButton: "btn btn-danger" // Metronic'in mavi buton stilini verdik
            }
          }).then((result: any) => {
            // Kullanıcı "Ok, got it!" butonuna bastığı an burası tetiklenir
            if (result.isConfirmed) {
              
              // ANGULAR REDIRECT (Yönlendirme):
              // Bu komut kullanıcıyı tarayıcıda doğrudan '/auth/login' sayfasına uçurur
              this.router.navigate(['/auth/login']);
            }
          });

        },
        error: (err) => {
          console.error('Registration failed:', err);
          
          // Hata durumunda da kullanıcıya kırmızı bir pop-up gösterelim
          Swal.fire({
            text: "Sorry, looks like there are some errors detected, please try again.",
            icon: "error",
            buttonsStyling: false,
            confirmButtonText: "Ok, got it!",
            customClass: {
              confirmButton: "btn btn-danger"
            }
          });
        }
      });
    }
  }
}

