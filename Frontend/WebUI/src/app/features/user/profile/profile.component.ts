import { Component, inject, OnInit, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService, UserProfile, TeamAvatar } from '../../../core/services/user.service';


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
  avatars = this.userService.avatars;

  // Profil Güncelleme Formu
  profileForm!: FormGroup;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';


  isProfileModalOpen = false;
  isAvatarSelectionView = false; // Form mu yoksa Takım listesi mi görünecek?
  tempAvatarPath: string | undefined = '';

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
      email: [{ value: '', disabled: true }],
      teamAvatarId: [null] // E-Posta değiştirilemez (Güvenlik gereği kilitli)
    });
  }

  private patchFormValues(data: UserProfile) {
    this.profileForm.patchValue({
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      teamAvatarId: data.teamAvatarId
    });
    this.tempAvatarPath = data.avatarPath;
  }

  openProfileModal() {
    const current = this.user();
    if (current) {
      // Modalı her açtığımızda formu son temiz haline sıfırlıyoruz (Kullanıcı vazgeçmiş olabilir)
      this.patchFormValues(current);
      this.isAvatarSelectionView = false; // Form ekranından başla
      this.isProfileModalOpen = true;
    }
  }

  closeModal() {
    this.isProfileModalOpen = false;
    this.isAvatarSelectionView = false;
  }

  
  openAvatarSelection() {
    this.isAvatarSelectionView = true; // Formu gizle, avatar grid'ini aç

    // Veritabanından takımları çek (Eğer daha önce çekilmediyse)
    if (this.avatars().length === 0) {
      this.userService.getTeamAvatars().subscribe({
        next: (response) => {
          if (response.success) {
            this.avatars.set(response.data);
          }
        },
        error: (err) => console.error('Avatarlar yüklenemedi:', err)
      });
    }
  }

  confirmAvatarSelection(avatar: TeamAvatar) {
    // Formun içine seçilen Takım ID'sini basıyoruz (henüz DB'ye gitmedi)
    this.profileForm.patchValue({ teamAvatarId: avatar.id });
    // Modaldaki önizleme resmini değiştiriyoruz
    this.tempAvatarPath = avatar.imagePath; 
    
    // Form ekranına geri dön
    this.isAvatarSelectionView = false;
  }

  cancelAvatarSelection() {
    // Form ekranına hiçbir şeyi değiştirmeden geri dön
    this.isAvatarSelectionView = false;
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

    const updatePayload = this.profileForm.getRawValue();

    
    this.userService.updateProfile(updatePayload).subscribe({
      next: (res) => {
        
        this.isSubmitting = false;
        this.closeModal();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = 'Güncelleme sırasında bir hata oluştu.';
        console.log("saaa",updatePayload)
        console.error(err);
      }
    });
  }
}