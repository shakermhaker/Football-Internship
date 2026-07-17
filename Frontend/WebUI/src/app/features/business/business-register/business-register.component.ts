import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router'; // Router eklendi
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { NgSelectModule } from '@ng-select/ng-select'; 
import { LocationService } from '../../../core/services/location.service'; 
import { BusinessService } from '../../../core/services/business.service'; // Servisimizi import ettik
import Swal from 'sweetalert2'; // Pop-up için

@Component({
  selector: 'app-business-register',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule, NgSelectModule],
  templateUrl: './business-register.component.html',
  styleUrl: './business-register.component.scss'
})
export class BusinessRegisterComponent implements OnInit {
  private locationService = inject(LocationService);
  private businessService = inject(BusinessService);
  private router = inject(Router); // İşlem bitince yönlendirme yapmak için

  cities = signal<any[]>([]); 
  districts = signal<any[]>([]); 
  
  // DTO'da beklediğimiz alanlar için değişkenler
  businessName: string = '';
  fullAddress: string = '';
  selectedCityId: number | null = null; 
  selectedDistrictId: number | null = null; 

  ngOnInit() {
    this.locationService.getCities().subscribe({
      next: (data) => this.cities.set(data),
      error: (err) => console.error('İller çekilirken hata:', err)
    });
  }

  onCityChange(city: any) {
    this.selectedDistrictId = null;
    this.districts.set([]);

    if (city && city.id) {
      this.locationService.getDistrictsByCityId(city.id).subscribe({
        next: (data) => this.districts.set(data),
        error: (err) => console.error('İlçeler çekilirken hata:', err)
      });
    }
  }

  // ASIL ŞOVUN OLDUĞU YER: KAYDET BUTONU
  saveBusiness() {
    // 1. Ufak bir doğrulama (Eksik veri var mı?)
    if (!this.businessName || !this.fullAddress || !this.selectedDistrictId) {
      Swal.fire('Uyarı', 'Lütfen işletme adı, adres ve ilçe bilgilerini eksiksiz doldurun.', 'warning');
      return;
    }

    // 2. DTO formatında veriyi hazırla
    const payload = {
      name: this.businessName,
      fullAddress: this.fullAddress,
      districtId: this.selectedDistrictId
    };

    // 3. Backend'e fırlat
    this.businessService.addBusiness(payload).subscribe({
      next: (res) => {
        if (res.success) {
          // Başarılıysa o güzel pop-up'ı göster
          Swal.fire({
            icon: 'success',
            title: 'Başarılı!',
            text: 'İşletme oluşturma talebiniz başarıyla iletildi.',
            confirmButtonColor: '#ff7d00' // Senin temanın turuncusu
          }).then(() => {
            this.router.navigate(['/']); // Kullanıcıyı ana sayfaya yönlendir
          });
        }
      },
      error: (err) => {
        console.error('Kayıt hatası:', err);
        Swal.fire('Hata', 'Kayıt sırasında bir sorun oluştu. Giriş yaptığınızdan emin olun.', 'error');
      }
    });
  }
}