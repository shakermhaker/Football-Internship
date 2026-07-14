import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router'; 
import { AuthService } from '../../../core/services/auth.service';

declare var Swal: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink], 
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  registerForm!: FormGroup;
  isPasswordVisible: boolean = false;
  isConfirmPasswordVisible: boolean = false;

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]], 
      confirmPassword: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required, Validators.minLength(10)]],
      birthDate: ['', [Validators.required]] 
    }, {
      validators: this.passwordMatchValidator 
    });
  }

  togglePasswordVisibility(): void {
    this.isPasswordVisible = !this.isPasswordVisible;
  }

  toggleConfirmPasswordVisibility(): void {
    this.isConfirmPasswordVisible = !this.isConfirmPasswordVisible;
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true }; 
    }
    return null;
  }

  onPhoneNumberInput(event: any): void {
    const input = event.target;
    const sanitizedValue = input.value.replace(/[^0-9 ]/g, '');
    
    input.value = sanitizedValue;
    this.registerForm.get('phoneNumber')?.setValue(sanitizedValue, { emitEvent: false });
  }

  onRegisterSubmit(): void {
    if (this.registerForm.valid) {
      const { confirmPassword, ...signUpData } = this.registerForm.value;
      
      if (signUpData.phoneNumber) {
        signUpData.phoneNumber = signUpData.phoneNumber.replace(/\s+/g, '');
      }

      this.authService.register(signUpData).subscribe({
        next: (response) => {
          console.log('Registration success:', response);

          Swal.fire({
            text: "Kayıt Başarılı, Hesabınızı doğrulamak için lütfen mailinizi kontrol ediniz!",
            icon: "success",
            buttonsStyling: false,
            confirmButtonText: "Tamam!",
            customClass: {
              confirmButton: "btn btn-danger"
            }
          }).then((result: any) => {
            if (result.isConfirmed) {
              this.router.navigate(['/auth/login']);
            }
          });

        },
        error: (err) => {
          console.error('Registration failed:', err);
          
          Swal.fire({
            text: "Kayıt olurken bir sorun oluştu, lütfen bilgilerini kontrol edip tekrar dene.",
            icon: "error",
            buttonsStyling: false,
            confirmButtonText: "Anladım",
            customClass: {
              confirmButton: "btn btn-danger"
            }
          });
        }
      });
    }
  }
}