import { Component, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService, UserProfile } from '../../../core/services/user.service';


@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  private userService = inject(UserService);
  private fb = inject(FormBuilder);

  // 🎯 Servisteki canlı kullanıcı sinyalini bağlıyoruz
  user = this.userService.currentUser;

  // Profil Güncelleme Formu
  profileForm!: FormGroup;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';

  constructor() {
    this.initForm();

    // 🌟 ANGULAR EFFECT: Sinyal (user) verisi geldiğinde veya değiştiğinde formu otomatik doldurur!
    effect(() => {
      const currentUserData = this.user();
      if (currentUserData) {
        this.patchFormValues(currentUserData);
      }
    });
  }

  ngOnInit() {
   
    if (!this.user()) {
      this.userService.fetchMyProfile().subscribe({
        error: (err) => {
          this.errorMessage = 'Profil bilgileri yüklenemedi.';
          console.error(err);
        }
      });
    }
  }

  private initForm() {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: [{ value: '', disabled: true }] // E-Posta değiştirilemez (Güvenlik gereği kilitli)
    });
  }

  private patchFormValues(data: UserProfile) {
    this.profileForm.patchValue({
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email
    });
  }

  // 🎯 FORM GÖNDERİLME OLAYI
  onSubmit() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    const updatedData = {
      ...this.profileForm.getRawValue(),
      rowGuid: this.user()?.rowGuid // Güvenlik için GUID gönderiyoruz
    };

  }
}