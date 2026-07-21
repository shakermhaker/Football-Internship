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
  // ... diğer kodların (varsa) ...

  this.locationService.getDistrictsByCityId(this.selectedCityId).subscribe({
    next: (res) => { 
      // Hatanın çözümü burada: res.data YERİNE direkt res yazıyoruz!
      this.districts.set(res); 
    },
    error: (err) => { 
      console.warn('Bu ilin ilçesi yok veya çekilemedi, sorun değil.');
      this.districts.set([]); // Hata alırsak listeyi boşaltıyoruz
    }
  });
}
  onSearch() {
  console.log("BUTONA BASILDI, cityId:", this.selectedCityId); // Çalışıp çalışmadığını konsoldan anlarız

  this.router.navigate(['/fields'], {  // DİKKAT: /footballfields DEĞİL, /fields YAZIYORUZ
    queryParams: {
      cityId: this.selectedCityId,         
      districtId: this.selectedDistrictId,
      search: this.searchText
    }
  });

  
  // Listeleme sayfasına (örneğin /fields) yönlendirirken filtreleri URL'ye iliştiriyoruz
  this.router.navigate(['/fields'], {
    queryParams: {
      districtId: this.selectedDistrictId, // Sadece ilçe ID'si yeterli
      search: this.searchText
    }
  });
  }
}