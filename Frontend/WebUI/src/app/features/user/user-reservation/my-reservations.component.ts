import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReservationService, UserReservationDetailDto } from '../../../core/services/reservation.service';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './my-reservations.component.html'
})
export class MyReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);

  isLoading = signal<boolean>(true);
  
  // Orijinal veriler (Backend'den ilk gelen)
  allReservations = signal<UserReservationDetailDto[]>([]);
  
  // Ekranda gösterilecek (Filtrelenmiş) veriler
  filteredReservations = signal<UserReservationDetailDto[]>([]);

  // Filtreleme Değişkenleri (İki Yönlü Bağlama için)
  searchTerm: string = '';
  filterDate: string = '';

  cancelingId = signal<number | null>(null); // Hangi satır iptal ediliyor?
  isCanceling = signal<boolean>(false);

  ngOnInit() {
    this.fetchMyReservations();
  }

  fetchMyReservations() {
    this.isLoading.set(true);
    this.reservationService.getUserReservations().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.allReservations.set(res.data);
          this.filteredReservations.set(res.data); // Başlangıçta hepsi görünür
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Rezervasyonlar çekilirken hata oluştu:', err);
        this.isLoading.set(false);
      }
    });
  }

  applyFilters() {
    let currentData = this.allReservations();

    // 1. İsme veya Şehre/İlçeye Göre Metin Filtresi
    if (this.searchTerm.trim() !== '') {
      // 🚀 ÇÖZÜM: Türkçe diline duyarlı küçük harf çevirimi yapıyoruz! (Büyük/Küçük harf sorunu çözüldü)
      const lowerTerm = this.searchTerm.toLocaleLowerCase('tr-TR');
      
      currentData = currentData.filter(r => 
        r.businessName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.footballFieldName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.cityName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.districtName.toLocaleLowerCase('tr-TR').includes(lowerTerm)
      );
    }

    // 2. Tarihe Göre Filtre (Varsa)
    if (this.filterDate) {
      currentData = currentData.filter(r => {
        // Backend'den YYYY-MM-DDT00:00:00 gibi gelebilir, biz sadece YYYY-MM-DD kısmını kıyaslayacağız
        const resDate = r.reservationDate.split('T')[0]; 
        return resDate === this.filterDate;
      });
    }

    this.filteredReservations.set(currentData);
  }

  // Filtreleri temizleme butonu için
  clearFilters() {
    this.searchTerm = '';
    this.filterDate = '';
    this.filteredReservations.set(this.allReservations());
  }

  
  startCancel(reservationId: number) {
    this.cancelingId.set(reservationId);
  }

  cancelCancel() {
    this.cancelingId.set(null);
  }

  confirmCancel(reservationId: number) {
    this.isCanceling.set(true);
    
    this.reservationService.cancelReservation(reservationId).subscribe({
      next: (res) => {
        // Ekranı baştan yenilemek yerine, SADECE o satırın durumunu güncelliyoruz! (Çok Hızlı)
        const currentAll = this.allReservations();
        const updatedAll = currentAll.map(r => 
          r.reservationId === reservationId ? { ...r, statusName: 'İptal Edildi' } : r
        );
        
        this.allReservations.set(updatedAll);
        this.applyFilters(); // Ekrana yansıt
        
        this.isCanceling.set(false);
        this.cancelingId.set(null);
      },
      error: (err) => {
        console.error('İptal işlemi başarısız oldu:', err);
        this.isCanceling.set(false);
        this.cancelingId.set(null);
      }
    });
  }
  

  formatTime(timeStr: string): string {
    if (!timeStr) return '';
    return timeStr.substring(0, 5); // "19:00:00" -> "19:00"
  }
}