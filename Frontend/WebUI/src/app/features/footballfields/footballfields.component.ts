import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FieldCardComponent } from '../../shared/components/field-card/field-card.component';
import { BusinessService } from '../../core/services/business.service';

@Component({
  selector: 'app-footballfields',
  standalone: true,
  imports: [CommonModule, FieldCardComponent], // FieldCard import edilmiş, süper!
  templateUrl: './footballfields.component.html',
  styleUrl: './footballfields.component.scss'
})
export class FootballfieldsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private businessService = inject(BusinessService); // Servis aktifleştirildi

  // Modern Signal yapısını kullanıyoruz
  dbFields = signal<any[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit() {
  this.route.queryParams.subscribe(params => {
    // BURASI ÇOK KRİTİK: URL'den cityId'yi okuyor muyuz?
    const cityId = params['cityId'] ? Number(params['cityId']) : null;
    const districtId = params['districtId'] ? Number(params['districtId']) : null;
    const search = params['search'] || '';

    // Okuduğumuz değerleri servise yolluyoruz
    this.fetchRealFields(cityId, districtId, search);
  });
}

  // Fonksiyonun imzasını 3 parametre alacak şekilde güncelledik
  fetchRealFields(cityId: number | null, districtId: number | null, search: string) {
  // BURAYI EKLİYORUZ: Bakalım veriler servise gitmeden önce ne alemde?
  console.log("🛑 SERVİSE GÖNDERİLEN PARAMETRELER 🛑", {
    SehirID: cityId,
    IlceID: districtId,
    Arama: search
  });

  this.isLoading.set(true);

    // 3. Servise de 3 parametreyi sırasıyla veriyoruz!
    this.businessService.getFilteredBusinesses(cityId, districtId, search).subscribe({
      next: (response) => {
        if (response.success) {
          this.dbFields.set(response.data); 
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Liste çekilirken hata:', err);
        this.isLoading.set(false);
      }
    });
  }
}