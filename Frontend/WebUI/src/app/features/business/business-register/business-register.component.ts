import { Component, OnInit, inject, signal } from '@angular/core'; // signal eklendi
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { NgSelectModule } from '@ng-select/ng-select'; 
import { LocationService } from '../../../core/services/location.service'; // YOL DÜZELTİLDİ!

@Component({
  selector: 'app-business-register',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule, NgSelectModule],
  templateUrl: './business-register.component.html',
  styleUrl: './business-register.component.scss'
})
export class BusinessRegisterComponent implements OnInit {
  private locationService = inject(LocationService);

  // ARTIK SİNYAL (SIGNAL) KULLANIYORUZ
  cities = signal<any[]>([]); 
  districts = signal<any[]>([]); 
  
  // ngModel ile formda tutacağımız seçili ID'ler normal değişken kalabilir
  selectedCityId: number | null = null; 
  selectedDistrictId: number | null = null; 

  ngOnInit() {
    this.locationService.getCities().subscribe({
      next: (data) => {
        this.cities.set(data); // Signal'e veriyi .set() ile basıyoruz
      },
      error: (err) => console.error('İller çekilirken hata:', err)
    });
  }

  onCityChange(city: any) {
    // İl değiştiğinde eski ilçeleri sıfırla
    this.selectedDistrictId = null;
    this.districts.set([]);

    if (city && city.id) {
      this.locationService.getDistrictsByCityId(city.id).subscribe({
        next: (data) => {
          this.districts.set(data); // Gelen ilçeleri Signal'e bas
        },
        error: (err) => console.error('İlçeler çekilirken hata:', err)
      });
    }
  }
}