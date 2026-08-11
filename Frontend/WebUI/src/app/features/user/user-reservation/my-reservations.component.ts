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

  // 🚀 Filtreleme Değişkenleri
  searchTerm: string = '';
  filterDate: string = '';
  
  // 🚀 YENİ: Statü Filtresi için Değişkenler
  statusOptions: string[] = ['Hepsi', 'Onaylandı', 'İptal Edildi', 'Tamamlandı'];
  selectedStatus: string = 'Hepsi';

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
          // Backend'den 'Bitti' geliyorsa arayüzde daha şık durması için 'Tamamlandı' yapıyoruz
          const formattedData = res.data.map(r => ({
            ...r,
            statusName: r.statusName === 'Bitti' ? 'Tamamlandı' : r.statusName
          }));
          
          this.allReservations.set(formattedData);
          this.filteredReservations.set(formattedData);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Rezervasyonlar çekilirken hata oluştu:', err);
        this.isLoading.set(false);
      }
    });
  }

  // 🚀 GÜNCELLENDİ: Statü Filtresi de Eklendi
  applyFilters() {
    let currentData = this.allReservations();

    // 1. Statü Filtresi
    if (this.selectedStatus !== 'Hepsi') {
      currentData = currentData.filter(r => r.statusName === this.selectedStatus);
    }

    // 2. Metin Filtresi
    if (this.searchTerm.trim() !== '') {
      const lowerTerm = this.searchTerm.toLocaleLowerCase('tr-TR');
      
      currentData = currentData.filter(r => 
        r.businessName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.footballFieldName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.cityName.toLocaleLowerCase('tr-TR').includes(lowerTerm) ||
        r.districtName.toLocaleLowerCase('tr-TR').includes(lowerTerm)
      );
    }

    // 3. Tarih Filtresi
    if (this.filterDate) {
      currentData = currentData.filter(r => {
        const resDate = r.reservationDate.split('T')[0]; 
        return resDate === this.filterDate;
      });
    }

    this.filteredReservations.set(currentData);
  }

  // Statü sekmesine tıklandığında çalışır
  setStatusFilter(status: string) {
    this.selectedStatus = status;
    this.applyFilters();
  }

  clearFilters() {
    this.searchTerm = '';
    this.filterDate = '';
    this.selectedStatus = 'Hepsi';
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
        const currentAll = this.allReservations();
        const updatedAll = currentAll.map(r => 
          r.reservationId === reservationId ? { ...r, statusName: 'İptal Edildi' } : r
        );
        
        this.allReservations.set(updatedAll);
        this.applyFilters(); 
        
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
    return timeStr.substring(0, 5); 
  }
}