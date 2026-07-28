import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReservationService, FootballFieldScheduleDto, PriceScheduleDto, CreateReservationDto } from '../../core/services/reservation.service';
import { UserService } from '../../core/services/user.service';

// Accordion için Frontend'e özel gruplanmış yapı
export interface GroupedDaySchedule {
  dayId: number;
  dayName: string;
  slots: PriceScheduleDto[];
}

export interface FieldWithGroupedSchedules {
  fieldId: number;
  fieldName: string;
  days: GroupedDaySchedule[];
}

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation.component.html'
})
export class ReservationComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router); // 🚀 Router eklendi

  private reservationService = inject(ReservationService);
  private userService = inject(UserService); // 🚀 Kullanıcı durumu için eklendi

  


  // İşlenmiş, arayüze basılmaya hazır veriler
  groupedFields = signal<FieldWithGroupedSchedules[]>([]);
  isLoading = signal<boolean>(true);
  selectedDate: string = '';
  minDate: string = ''; 
  bookedScheduleIds = signal<number[]>([]);
  businessId: number = 0;

  isModalOpen = signal<boolean>(false);
  isLoginModalOpen = signal<boolean>(false);

  selectedSlot: PriceScheduleDto | null = null;
  selectedFieldName: string = '';
  cardNumber: string = '';
  isSubmitting = false;
  errorMessage = '';

  ngOnInit() {
    this.businessId = Number(this.route.snapshot.paramMap.get('id'));

    // Bugünün tarihini varsayılan olarak ata (YYYY-MM-DD formatında)
    const today = new Date();
    this.selectedDate = today.toISOString().split('T')[0];
    
    // 🚀 YENİ: Takvimde seçilebilecek minimum tarihi BUGÜN olarak belirliyoruz
    this.minDate = this.selectedDate;

    if (this.businessId) {
      // 🚀 DİKKAT: Sayfa ilk açıldığında da sadece bugünün gününe ait (Örn: Cuma) saatleri çekiyoruz
      this.fetchSchedules(this.businessId, this.selectedDate);
      this.fetchBookedSlots(this.businessId, this.selectedDate);
    }
  }

  fetchSchedules(businessId: number, dateStr: string) {
    this.reservationService.getBusinessFieldSchedules(businessId, dateStr).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.processDataForAccordion(res.data);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Takvim çekilirken hata:', err);
        this.isLoading.set(false);
      }
    });
  }

  // Belirli tarihteki dolu ID'leri backend'den çeker
  fetchBookedSlots(businessId: number, dateStr: string) {
    this.reservationService.getBookedScheduleIdsByDate(businessId, dateStr).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.bookedScheduleIds.set(res.data);
        } else {
          this.bookedScheduleIds.set([]);
        }
      },
      error: (err) => {
        console.error('Dolu slotlar çekilirken hata:', err);
        this.bookedScheduleIds.set([]);
      }
    });
  }

  // Kullanıcı takvimden yeni bir tarih seçtiğinde tetiklenir
  onDateChange(event: any) {
    const newDate = event.target.value;
    if (newDate && this.businessId) {
      this.selectedDate = newDate;
      this.isLoading.set(true); // Veriler gelene kadar loading dönsün
      
      // 🚀 YENİ: Tarih değiştiğinde HEM dolu slotları HEM DE o günün programını yeniden çekiyoruz!
      this.fetchSchedules(this.businessId, newDate);
      this.fetchBookedSlots(this.businessId, newDate);
    }
  }

  // Verilen slot ID'sinin dolu olup olmadığını kontrol eder
  isSlotBooked(scheduleId: number): boolean {
    return this.bookedScheduleIds().includes(scheduleId);
  }

  // 🚀 Backend'den gelen düz listeyi, günlere göre (Accordion için) gruplar
  private processDataForAccordion(data: FootballFieldScheduleDto[]) {
    const processedFields: FieldWithGroupedSchedules[] = [];

    for (const field of data) {
      const daysMap = new Map<number, GroupedDaySchedule>();

      for (const schedule of field.schedules) {
        if (!daysMap.has(schedule.dayId)) {
          daysMap.set(schedule.dayId, {
            dayId: schedule.dayId,
            dayName: schedule.dayName,
            slots: []
          });
        }
        daysMap.get(schedule.dayId)!.slots.push(schedule);
      }

      processedFields.push({
        fieldId: field.footballFieldId,
        fieldName: field.footballFieldName,
        // Map'i Array'e çevir ve DayId'ye göre tekrar sırala (Garanti olsun)
        days: Array.from(daysMap.values()).sort((a, b) => a.dayId - b.dayId)
      });
    }

    this.groupedFields.set(processedFields);
  }

   onSlotSelected(slot: PriceScheduleDto, fieldName: string) {
    if (this.isSlotBooked(slot.fieldPriceScheduleId)) return; 
    
    // 🚀 GİRİŞ KONTROLÜ
    const user = this.userService.currentUser();
    console.log('Rezervasyon tıklaması - Aktif Kullanıcı:', user);

    if (!user) {
      console.log('Kullanıcı giriş yapmamış, uyarı modalı tetikleniyor...');
      this.isLoginModalOpen.set(true); // Signal güncelleniyor
      return;
    }

    // Modal'ı açmak için bilgileri state'e yazıyoruz
    this.selectedSlot = slot;
    this.selectedFieldName = fieldName;
    this.cardNumber = ''; 
    this.errorMessage = '';
    this.isModalOpen.set(true); // Signal güncelleniyor
  }


   redirectToLogin() {
    this.isLoginModalOpen.set(false);
    this.router.navigate(['/auth/login']);
  }

  closeModal() {
    this.isModalOpen.set(false);
    this.selectedSlot = null;
  }
  confirmReservation() {
    if (!this.selectedSlot || !this.cardNumber.trim()) {
      this.errorMessage = 'Lütfen geçerli bir kart numarası giriniz.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const payload: CreateReservationDto = {
      fieldPriceScheduleId: this.selectedSlot.fieldPriceScheduleId,
      reservationDate: this.selectedDate,
      finalPrice: this.selectedSlot.price,
      cardNumber: this.cardNumber
    };

    this.reservationService.createReservation(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.closeModal();
        // 🚀 BAŞARILI! Hemen ardından o günün dolu slotlarını tekrar çekiyoruz 
        // ki ekrandaki buton anında KIRMIZI'ya dönsün!
        this.fetchBookedSlots(this.businessId, this.selectedDate);
      },
      error: (err) => {
        this.isSubmitting = false;
        // Backend'den (ReservationManager'daki ErrorResult'tan) gelen özel uyarı mesajı:
        // "Üzgünüz, bu saha ve saat az önce başka biri tarafından rezerve edildi."
        this.errorMessage = err.error?.message || 'Rezervasyon oluşturulurken bir hata oluştu. Lütfen tekrar giriş yapıp deneyin.';
        console.error(err);
      }
    });
  }

  // "18:00:00" string'ini "18:00" yapar
  formatTime(timeStr: string): string {
    if (!timeStr) return '';
    return timeStr.substring(0, 5); 
  }
}