import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; 
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms'; 
import { NgSelectModule } from '@ng-select/ng-select'; 
import { LocationService } from '../../core/services/location.service'; 

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, NgSelectModule], 
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private router = inject(Router);
  private locationService = inject(LocationService);

  // BusinessRegister'daki gibi Signal yapısına geçtik
  cities = signal<any[]>([]);
  districts = signal<any[]>([]);

  // Seçilen veriler ve arama metni
  selectedCityId: number | null = null;
  selectedDistrictId: number | null = null;
  searchText: string = '';

  ngOnInit() {
    this.locationService.getCities().subscribe({
      next: (data) => this.cities.set(data),
      error: (err) => console.error('İller çekilirken hata:', err)
    });
  }

  onCityChange(event: any) {
    // 1. KONTROL: Kullanıcı çarpıya basıp ili silerse veya seçmezse:
    if (!this.selectedCityId) {
      this.districts.set([]); // İlçeleri boşalt
      this.selectedDistrictId = null; // Seçili ilçeyi sıfırla
      return; // Backend'e boşuna istek atma
    }

    this.locationService.getDistrictsByCityId(this.selectedCityId).subscribe({
      next: (res) => { 
        this.districts.set(res); 
      },
      error: (err) => { 
        console.warn('Bu ilin ilçesi yok veya çekilemedi, sorun değil.');
        this.districts.set([]); 
      }
    });
  }

  onSearch() {
    console.log("BUTONA BASILDI, cityId:", this.selectedCityId); 

    // 2. KONTROL: Sadece dolu olanları eklemek için boş bir obje oluşturuyoruz!
    // Böylece URL'de "?cityId=null" gibi saçmalıklar olmaz, backend rahat eder.
    const params: any = {};

    if (this.selectedCityId) {
      params.cityId = this.selectedCityId;
    }

    if (this.selectedDistrictId) {
      params.districtId = this.selectedDistrictId;
    }

    if (this.searchText && this.searchText.trim() !== '') {
      params.search = this.searchText.trim();
    }

    this.router.navigate(['/fields'], { 
      queryParams: params
    });
  }
}