import { Component, OnInit, inject , ChangeDetectorRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // 🚀 ngModel için FormsModule eklendi!
import { ReservationService } from '../../../core/services/reservation.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-business-reservations',
  standalone: true,
  imports: [CommonModule, FormsModule], // 🚀 FormsModule buraya da eklendi
  templateUrl: './business-reservations.component.html',
  styleUrls: ['./business-reservations.component.scss']
})
export class BusinessReservationsComponent implements OnInit {

  private reservationService = inject(ReservationService);
  private cdr = inject(ChangeDetectorRef);
  
  weekDays: any[] = [];
  selectedDate: Date = new Date();
  selectedDateStr: string = ''; // Input'a bağlanacak YYYY-MM-DD formatı
  currentMonthYear: string = ''; // Örn: "Temmuz 2026"
  currentBusinessId: number | null = null;

  summaryData = {
    totalReservations: 0,
    earliestAndLatestTime: '-',
    totalUniqueCustomers: 0,
    reservations: [] as any[]
  };

  ngOnInit(): void {
    const savedBusinessId = localStorage.getItem('businessId');
    if (savedBusinessId) {
      this.currentBusinessId = Number(savedBusinessId);
    }

    // Başlangıç tarihi olarak bugünü ayarla
    this.selectedDate = new Date();
    this.selectedDateStr = this.formatDateForInput(this.selectedDate);

    this.generateWeek(this.selectedDate);
    this.loadReservationsForDate(this.selectedDate);
  }

  // Takvimi verilen tarihin bulunduğu haftaya göre oluşturur
  generateWeek(referenceDate: Date) {
    const currentDay = referenceDate.getDay();
    const distanceToMonday = currentDay === 0 ? -6 : 1 - currentDay; // Pazartesiyi bul
    const monday = new Date(referenceDate);
    monday.setDate(referenceDate.getDate() + distanceToMonday);

    this.weekDays = [];
    const dayNames = ['PAZ', 'PZT', 'SAL', 'ÇAR', 'PER', 'CUM', 'CMT'];
    const monthNames = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

    for (let i = 0; i < 7; i++) {
      const date = new Date(monday);
      date.setDate(monday.getDate() + i);

      this.weekDays.push({
        name: dayNames[date.getDay()],
        date: date.getDate(),
        fullDate: date,
        // Günleri karşılaştırarak aktif olanı bul
        active: date.toDateString() === this.selectedDate.toDateString()
      });
    }

    // Başlığı güncelle (Haftanın perşembe gününün ayını baz alarak daha tutarlı bir ay gösterimi yaparız)
    const thursday = new Date(monday);
    thursday.setDate(monday.getDate() + 3);
    this.currentMonthYear = `${monthNames[thursday.getMonth()]} ${thursday.getFullYear()}`;
  }

  // Haftalık takvimden güne tıklandığında
  // Haftalık takvimden güne tıklandığında
  // Haftalık takvimden güne tıklandığında
  selectDay(day: any) {
    this.weekDays.forEach(d => d.active = false);
    day.active = true;
    
    // 🎯 1. ÇÖZÜM: Angular'ın değişimi kesin algılaması için yeni referans (new Date) atıyoruz
    this.selectedDate = new Date(day.fullDate);
    this.selectedDateStr = this.formatDateForInput(this.selectedDate); 
    
    // 🎯 2. ÇÖZÜM: Yeni veriler gelene kadar ekranı anında temizle! 
    // Böylece "Acaba tıklayamadım mı?" hissiyatı ortadan kalkar, tablo anında boşalır ve dolar.
    this.summaryData = {
      totalReservations: 0,
      earliestAndLatestTime: '-',
      totalUniqueCustomers: 0,
      reservations: []
    };

    this.loadReservationsForDate(this.selectedDate);
  }

  // Date picker'dan tarih değiştirildiğinde
  // Date picker'dan tarih değiştirildiğinde
  // Date picker'dan tarih değiştirildiği an (Boşluğa tıklamayı beklemeden) tetiklenir
  onDateChange(event: any) {
    const newDate = event.target.value;
    
    if (newDate && this.currentBusinessId) {
      this.selectedDateStr = newDate;
      this.selectedDate = new Date(newDate); // Takvimi güncellemek için referans
      
      // Takvimin haftasını yeniden hesapla
      this.generateWeek(this.selectedDate);

      // Veriler gelene kadar tabloyu şokla temizle
      this.summaryData = {
        totalReservations: 0,
        earliestAndLatestTime: '-',
        totalUniqueCustomers: 0,
        reservations: []
      };

      // Yeni tarihe göre istek at
      this.loadReservationsForDate(this.selectedDate);
    }
  }

  // Bir önceki haftaya git
  prevWeek() {
    this.selectedDate.setDate(this.selectedDate.getDate() - 7);
    this.selectedDate = new Date(this.selectedDate); // Referansı tetikle
    this.selectedDateStr = this.formatDateForInput(this.selectedDate);
    this.generateWeek(this.selectedDate);
    this.loadReservationsForDate(this.selectedDate);
  }

  // Bir sonraki haftaya git
  nextWeek() {
    this.selectedDate.setDate(this.selectedDate.getDate() + 7);
    this.selectedDate = new Date(this.selectedDate); // Referansı tetikle
    this.selectedDateStr = this.formatDateForInput(this.selectedDate);
    this.generateWeek(this.selectedDate);
    this.loadReservationsForDate(this.selectedDate);
  }

  // Tarihi YYYY-MM-DD formatına çevirir (Saat farkından gün kaymasını önler)
  // Tarihi YYYY-MM-DD formatına çevirir (Saat dilimi kaymalarını KESİN olarak engeller)
  formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = ('0' + (date.getMonth() + 1)).slice(-2);
    const day = ('0' + date.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }

  loadReservationsForDate(date: Date) {
    if (!this.currentBusinessId) return;

    // Senin yazdığın tarih formatlayıcı veya ISO split kullanabilirsin
    const formattedDate = date.toISOString().split('T')[0];

    this.reservationService.getDailyReservations(this.currentBusinessId, formattedDate).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.summaryData = response.data;
          
          // 🎯 ÇÖZÜM İŞTE BURASI: Angular'a "Hemen HTML'i güncelle" emri veriyoruz!
          this.cdr.detectChanges(); 
        }
      },
      error: (err) => {
        console.error('Rezervasyonlar çekilirken hata oluştu:', err);
        this.summaryData = {
          totalReservations: 0,
          earliestAndLatestTime: '-',
          totalUniqueCustomers: 0,
          reservations: []
        };
        // 🎯 Hata durumunda da ekranı güncellemesi için
        this.cdr.detectChanges();
      }
    });
  }
  cancelReservation(reservationId: number) {
    Swal.fire({
      title: 'Emin misiniz?',
      text: "Bu rezervasyonu iptal etmek istediğinize emin misiniz? İşlem geri alınamaz.",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#f1416c',
      cancelButtonColor: '#b5b5c3',
      confirmButtonText: 'Evet, İptal Et!',
      cancelButtonText: 'Vazgeç'
    }).then((result) => {
      if (result.isConfirmed) {
        this.reservationService.cancelReservationByBusiness(reservationId).subscribe({
          next: (response: any) => {
            Swal.fire({
              title: 'İptal Edildi!',
              text: response.message || 'Rezervasyon başarıyla iptal edildi.',
              icon: 'success',
              confirmButtonColor: '#50cd89'
            });
            // İşlem bitince tabloyu güncelliyoruz
            this.loadReservationsForDate(this.selectedDate); 
          },
          error: (err) => {
            Swal.fire({
              title: 'Hata!',
              text: err.error?.message || 'İptal işlemi sırasında bir hata oluştu.',
              icon: 'error',
              confirmButtonColor: '#f1416c'
            });
          }
        });
      }
    });
  }
}