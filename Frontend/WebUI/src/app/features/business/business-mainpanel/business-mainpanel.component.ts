import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { BusinessService } from '../../../core/services/business.service';
import { LocationService } from '../../../core/services/location.service';
import Swal from 'sweetalert2';
import { ImageCropperComponent, ImageCroppedEvent } from 'ngx-image-cropper';

export interface BusinessImage {
  id: number;
  imagePath: string;
  isCover: boolean;
}

export interface BusinessDetail {
  businessId: number;
  name: string;
  city: string;
  district: string;
  fullAddress: string;
  images: BusinessImage[];
}

@Component({
  selector: 'app-business-mainpanel',
  templateUrl: './business-mainpanel.component.html',
  styleUrls: ['./business-mainpanel.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule, 
    ImageCropperComponent
  ]
})
export class BusinessMainpanelComponent implements OnInit {
  businessForm!: FormGroup;
  
  currentBusinessId: number | null = null; 
  imageBaseUrl: string = 'https://localhost:7074'; 

  coverImage: BusinessImage | null = null;
  galleryImages: BusinessImage[] = [];
  cities = signal<any[]>([]);
  districts = signal<any[]>([]);  

  pendingCoverFile: File | null = null;
  pendingCoverPreview: string | null = null; 
  
  pendingGalleryPreviews: { file: File, url: string }[] = [];

  // 🎯 KIRPMA MODALI İÇİN YENİ DEĞİŞKENLER
  isCropModalOpen = false;
  imageChangedEvent: any = ''; 
  croppedImageBlob: Blob | null | undefined = null;
  croppingType: 'cover' | 'gallery' | null = null; // Ne tür bir resim kırptığımızı bilmek için

  constructor(
    private fb: FormBuilder,
    private businessService: BusinessService,
    private locationService: LocationService
  ) {}

  ngOnInit(): void {
    this.businessForm = this.fb.group({
      name: ['', Validators.required],
      city: ['', Validators.required],
      district: ['', Validators.required],
      address: ['', Validators.required]
    });

    this.locationService.getCities().subscribe({
      next: (data) => {
        this.cities.set(data);
        const savedBusinessId = localStorage.getItem('businessId'); 
        
        if (savedBusinessId) {
          this.currentBusinessId = Number(savedBusinessId);
          this.loadBusinessData(); 
        } else {
          const currentUser = this.businessService.user; 
          if (currentUser && currentUser.businessId) {
            this.currentBusinessId = currentUser.businessId;
            this.loadBusinessData();
          } else {
            console.error('Kullanıcıya ait bir businessId bulunamadı!');
          }
        }
      },
      error: (err) => console.error('İller çekilirken hata:', err)
    });
  }

  loadBusinessData() {
    if (!this.currentBusinessId) return;

    this.businessService.getBusinessDetails(this.currentBusinessId).subscribe({
      next: (response: any) => {
        const details: BusinessDetail = response.data; 

        this.businessForm.patchValue({
          name: details.name,
          city: details.city, 
          address: details.fullAddress
        });

        const currentCity = this.cities().find(c => c.name === details.city || c.id === details.city);

        if (currentCity) {
          this.locationService.getDistrictsByCityId(currentCity.id).subscribe({
            next: (data) => {
              this.districts.set(data);
              this.businessForm.patchValue({ district: details.district });
            },
            error: (err) => console.error('İlçeler çekilirken hata:', err)
          });
        }

        this.coverImage = details.images.find((img: BusinessImage) => img.isCover) || null;
        this.galleryImages = details.images.filter((img: BusinessImage) => !img.isCover);
      },
      error: (err) => {
        console.error('İşletme verileri çekilirken hata oluştu:', err);
      }
    });
  }

  onCityChange(event: any) {
    const selectedCity = typeof event === 'object' ? event.target?.value : event;
    const city = this.cities().find(c => c.id == selectedCity || c.name === selectedCity);

    this.districts.set([]);
    this.businessForm.get('district')?.setValue(''); 

    if (city && city.id) {
      this.locationService.getDistrictsByCityId(city.id).subscribe({
        next: (data) => this.districts.set(data),
        error: (err) => console.error('İlçeler çekilirken hata:', err)
      });
    }
  }

  // 🎯 GÜNCELLENDİ: Kapak resmi seçilince kırpma modalını açar
  onCoverImageSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      if (this.galleryImages.length >= 4 && this.coverImage) {
        Swal.fire({
          title: 'Sınır Aşıldı!',
          text: 'Kapakla beraber en fazla 5 görsel seçebilirsiniz. Lütfen görsellerinizden birini silin.',
          icon: 'warning',
          confirmButtonText: 'Tamam',
          confirmButtonColor: '#f1416c'
        });
        event.target.value = ''; 
        return;
      }

      this.croppingType = 'cover';
      this.imageChangedEvent = event;
      this.isCropModalOpen = true;
    }
  }

  cancelPendingCover() {
    this.pendingCoverFile = null;
    this.pendingCoverPreview = null;
  }

  // 🎯 GÜNCELLENDİ: Galeri resmi seçilince kırpma modalını açar
  onGalleryImageSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      const totalCurrentImages = this.galleryImages.length + this.pendingGalleryPreviews.length;
      
      if (totalCurrentImages >= 4) { 
        Swal.fire({
          title: 'Sınır Aşıldı!',
          text: 'Bir işletmeye kapak görseli dahil en fazla 5 adet görsel ekleyebilirsiniz.',
          icon: 'warning',
          confirmButtonColor: '#f1416c'
        });
        event.target.value = '';
        return;
      }

      this.croppingType = 'gallery';
      this.imageChangedEvent = event;
      this.isCropModalOpen = true;
    }
  }

  // 🎯 YENİ: Modal içindeki cropper hareket ettikçe kırpılmış veriyi yakalar
  imageCropped(event: ImageCroppedEvent) {
    this.croppedImageBlob = event.blob;
  }

  // 🎯 YENİ: Kırpma iptal edilirse
  closeCropModal() {
    this.isCropModalOpen = false;
    this.imageChangedEvent = '';
    this.croppedImageBlob = null;
    this.croppingType = null;
  }

  // 🎯 YENİ: "Kırp ve Onayla" butonuna basıldığında
  applyCroppedImage() {
    if (!this.croppedImageBlob || !this.croppingType) return;

    // Blob objesini senin C#'ın anlayacağı File objesine çeviriyoruz
    const fileName = `cropped-${new Date().getTime()}.jpg`;
    const fileToUpload = new File([this.croppedImageBlob], fileName, { type: "image/jpeg" });
    const previewUrl = URL.createObjectURL(fileToUpload);

    if (this.croppingType === 'cover') {
      this.pendingCoverFile = fileToUpload;
      this.pendingCoverPreview = previewUrl;
    } else if (this.croppingType === 'gallery') {
      this.pendingGalleryPreviews.push({ file: fileToUpload, url: previewUrl });
    }

    this.closeCropModal();
  }

  removePendingGalleryImage(index: number) {
    this.pendingGalleryPreviews.splice(index, 1);
  }

  onDeleteGalleryImage(imageId: number) {
    Swal.fire({
      title: 'Emin misiniz?',
      text: "Bu görseli kalıcı olarak silmek istediğinize emin misiniz?",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Evet, sil!',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        this.businessService.deleteBusinessImage(imageId).subscribe({
          next: (res) => {
            Swal.fire('Silindi!', 'Görsel başarıyla silindi.', 'success');
            this.loadBusinessData();
          },
          error: (err) => {
            console.error("Görsel silinemedi:", err);
            Swal.fire('Hata!', 'Görsel silinirken bir hata oluştu.', 'error');
          }
        });
      }
    });
  }

  onSubmit() {
    if (this.businessForm.valid) {
      const selectedDistrictName = this.businessForm.value.district;
      const matchedDistrict = this.districts().find(d => d.name === selectedDistrictName);

      if (!matchedDistrict) {
        Swal.fire({
          title: 'Hata!',
          text: 'Lütfen geçerli bir ilçe seçtiğinizden emin olun.',
          icon: 'error',
          confirmButtonColor: '#f1416c'
        });
        return;
      }

      if (!this.currentBusinessId) {
        console.error("İşletme ID'si bulunamadı!");
        return;
      }

      const updateDto = {
        businessId: this.currentBusinessId,
        name: this.businessForm.value.name,
        districtId: matchedDistrict.id, 
        fullAddress: this.businessForm.value.address
      };

      this.businessService.updateBusinessDetails(updateDto).subscribe({
        next: (response: any) => {
          Swal.fire({
            title: 'Başarılı!',
            text: 'İşletme bilgileriniz başarıyla güncellendi.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
          });
          this.loadBusinessData();
        },
        error: (err) => {
          const errorMessage = err.error?.message || err.error?.Message || 'Güncelleme sırasında bir hata oluştu.';
          Swal.fire({
            title: 'Hata!',
            text: errorMessage,
            icon: 'error',
            confirmButtonColor: '#f1416c'
          });
          console.error('Update hatası:', err);
        }
      });
    } else {
      this.businessForm.markAllAsTouched();
    }
  }

  onSaveImages() {
    if (!this.currentBusinessId) return;

    if (!this.pendingCoverFile && this.pendingGalleryPreviews.length === 0) {
      Swal.fire({
        title: 'Bilgi',
        text: 'Kaydedilecek yeni bir görsel seçilmedi. Mevcut görselleriniz zaten aktif.',
        icon: 'info',
        timer: 2000,
        showConfirmButton: false
      });
      return;
    }

    if (this.pendingCoverFile && this.galleryImages.length >= 4 && this.coverImage) {
      Swal.fire({
        title: 'Sınır Aşıldı!',
        text: 'Kapakla beraber en fazla 5 görsel seçebilirsiniz. Lütfen görsellerinizden birini silin.',
        icon: 'warning',
        confirmButtonText: 'Tamam',
        confirmButtonColor: '#f1416c'
      });
      return;
    }

    if (this.pendingCoverFile) {
      this.businessService.addBusinessImage(this.currentBusinessId, this.pendingCoverFile, true).subscribe({
        next: () => {
          this.pendingCoverFile = null;
          this.pendingCoverPreview = null;
          this.loadBusinessData();
          
          if (this.pendingGalleryPreviews.length === 0) {
            Swal.fire({
              title: 'Başarılı!',
              text: 'Kapak görseli başarıyla güncellendi!',
              icon: 'success',
              confirmButtonText: 'Tamam',
              confirmButtonColor: '#00c853'
            });
          }
        },
        error: (err) => {
          console.error("Kapak yüklenirken hata:", err);
          Swal.fire({
            title: 'Hata!',
            text: 'Kapak yüklenirken hata oluştu.',
            icon: 'error',
            confirmButtonColor: '#f1416c'
          });
        }
      });
    }

    if (this.pendingGalleryPreviews.length > 0) {
      this.pendingGalleryPreviews.forEach((item, index) => {
        this.businessService.addBusinessImage(this.currentBusinessId!, item.file, false).subscribe({
          next: () => {
             if (index === this.pendingGalleryPreviews.length - 1) {
               this.pendingGalleryPreviews = [];
               this.loadBusinessData();
               Swal.fire({
                 title: 'Başarılı!',
                 text: 'Yeni görselleriniz başarıyla eklendi!',
                 icon: 'success',
                 confirmButtonText: 'Tamam',
                 confirmButtonColor: '#00c853'
               });
             }
          },
          error: (err) => {
            console.error("Galeri görseli yüklenirken hata:", err);
            Swal.fire({
              title: 'Hata!',
              text: 'Görsel yüklenirken bir hata oluştu.',
              icon: 'error',
              confirmButtonColor: '#f1416c'
            });
          }
        });
      });
    }
  }
}