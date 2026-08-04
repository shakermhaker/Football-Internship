import { Component, OnInit,OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReservationService, FootballFieldScheduleDto, PriceScheduleDto, CreateReservationDto } from '../../core/services/reservation.service';
import { UserService } from '../../core/services/user.service';
import { BusinessService, BusinessDetailDto } from '../../core/services/business.service';
import * as signalR from '@microsoft/signalr';

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
export class ReservationComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router); // 🚀 Router eklendi

  private reservationService = inject(ReservationService);
  private userService = inject(UserService); // 🚀 Kullanıcı durumu için eklendi
  private businessService = inject(BusinessService);

  
  businessDetail = signal<BusinessDetailDto | null>(null);

  // İşlenmiş, arayüze basılmaya hazır veriler
  groupedFields = signal<FieldWithGroupedSchedules[]>([]);
  isLoading = signal<boolean>(true);
  selectedDate: string = '';
  minDate: string = ''; 
  bookedScheduleIds = signal<number[]>([]);
  businessId: number = 0;


  private hubConnection!: signalR.HubConnection;
  heldScheduleIds = signal<number[]>([]);
  myActiveHold: { scheduleId: number, expiresAt: number } | null = null;
  countdownText = signal<string>('05:00');
  private timerInterval: any;

  isModalOpen = signal<boolean>(false);
  isLoginModalOpen = signal<boolean>(false);

  selectedSlot: PriceScheduleDto | null = null;
  selectedFieldName: string = '';
  cardNumber: string = '';
  isSubmitting = false;
  errorMessage = '';

  ngOnInit() {
    this.businessId = Number(this.route.snapshot.paramMap.get('id'));

    const today = new Date();
    this.selectedDate = today.toISOString().split('T')[0];
    this.minDate = this.selectedDate;

    if (this.businessId) {
      this.fetchBusinessDetails(this.businessId);
      this.fetchSchedules(this.businessId, this.selectedDate);
      this.fetchBookedSlots(this.businessId, this.selectedDate);
      
      // 🚀 SAYFA AÇILINCA ODAYA BAĞLAN
      this.startSignalRConnection();
    }
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.invoke('LeaveBusinessGroup', this.businessId.toString()).catch(err => console.error(err));
      this.hubConnection.stop();
    }
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  private startSignalRConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7074/reservationHub') // Kendi portuna göre kontrol et!
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('SignalR Bağlandı! Odaya giriliyor...');
        this.hubConnection.invoke('JoinBusinessGroup', this.businessId.toString());
      })
      .catch(err => console.log('SignalR Hatası: ' + err));

    // 1. Birisi slotu sepetine attı (Hold)
    this.hubConnection.on('SlotHeld', (data: { scheduleId: number, date: string }) => {
      // Ekranda seçili tarihle eşleşiyorsa ve BU SLOT BENİM SEPETİMDE DEĞİLSE turuncuya boya
      if (data.date === this.selectedDate && this.myActiveHold?.scheduleId !== data.scheduleId) {
        this.heldScheduleIds.update(ids => [...ids, data.scheduleId]);
      }
    });

    // 2. Biri ödemeyi tamamladı (Booked)
    this.hubConnection.on('SlotBooked', (data: { scheduleId: number, date: string }) => {
      if (data.date === this.selectedDate) {
        this.heldScheduleIds.update(ids => ids.filter(id => id !== data.scheduleId)); // Turuncudan çıkar
        this.bookedScheduleIds.update(ids => [...ids, data.scheduleId]); // Kırmızıya ekle
      }
    });

    // 3. Süre bitti veya sepetten çıkardı (Freed)
    this.hubConnection.on('SlotFreed', (data: { scheduleId: number, date: string }) => {
      if (data.date === this.selectedDate) {
        this.heldScheduleIds.update(ids => ids.filter(id => id !== data.scheduleId)); // Turuncudan çıkar (Yeşile döner)
      }
    });
  }

  fetchBusinessDetails(id: number) {
    // 🚀 Servisteki yeni ismiyle (getBusinessDetails) çağırıyoruz
    this.businessService.getBusinessDetails(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          // Resimleri isCover = true olan başa gelecek şekilde sırala
          if (res.data.images && res.data.images.length > 0) {
            res.data.images.sort((a, b) => (a.isCover === b.isCover) ? 0 : a.isCover ? -1 : 1);
          }
          this.businessDetail.set(res.data);
        }
      },
      error: (err) => console.error('İşletme detayı çekilemedi', err)
    });
  }

  getFullImagePath(path: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `https://localhost:7074${path}`; // Kendi portuna göre kontrol et!
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

  isSlotInPast(slotStartTime: string): boolean {
    if (!slotStartTime || !this.selectedDate) return false;

    const now = new Date();
    // Saat dilimi kaymalarını önleyerek bugünün tarihini YYYY-MM-DD formatında alıyoruz
    const todayStr = new Date(now.getTime() - (now.getTimezoneOffset() * 60000)).toISOString().split('T')[0];

    // Sadece "Bugün" seçiliyse saat kontrolü yap
    if (this.selectedDate === todayStr) {
      const currentHour = now.getHours();
      const currentMinute = now.getMinutes();
      
      const [slotHourStr, slotMinuteStr] = slotStartTime.split(':');
      const slotHour = parseInt(slotHourStr, 10);
      const slotMinute = parseInt(slotMinuteStr, 10);

      // Eğer slotun saati şu anki saatten küçükse (veya aynı saat ama dakika geçmişse)
      if (slotHour < currentHour) return true;
      if (slotHour === currentHour && slotMinute <= currentMinute) return true;
    }

    // Yarın veya sonraki günlerde ise tüm saatler uygundur
    return false;
  }

  // Verilen slot ID'sinin dolu olup olmadığını kontrol eder
  isSlotBooked(scheduleId: number): boolean {
    return this.bookedScheduleIds().includes(scheduleId);
  }

  isSlotHeld(scheduleId: number): boolean {
    return this.heldScheduleIds().includes(scheduleId);
  }

  isMyHold(scheduleId: number): boolean {
    return this.myActiveHold?.scheduleId === scheduleId;
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
    const sId = slot.fieldPriceScheduleId;

    // 1. Zaten Doluysa veya başkasındaysa tıklanamaz
    if (this.isSlotBooked(sId) || this.isSlotHeld(sId)) return; 
    
    // GİRİŞ KONTROLÜ
    const user = this.userService.currentUser();
    if (!user) {
      this.isLoginModalOpen.set(true); 
      return;
    }

    // 2. EĞER BU SLOT ZATEN BENİM SEPETİMDEYSE (Modalı kapatıp tekrar tıklamıştır) -> Sadece modalı aç
    if (this.myActiveHold && this.myActiveHold.scheduleId === sId) {
      this.selectedSlot = slot;
      this.selectedFieldName = fieldName;
      this.isModalOpen.set(true);
      return;
    }

    // 3. EĞER BAŞKA BİR SLOT SEPETİMDEYSE -> İzin verme
    if (this.myActiveHold) {
       alert("Zaten işlemde olan bir rezervasyonunuz var. Lütfen önce onu tamamlayın veya süresinin bitmesini bekleyin.");
       return;
    }

    // 4. İLK DEFA TIKLIYORSA -> API'ye Geçici Kilit (Hold) isteği at!
    // NOT: ReservationService içine holdReservationSlot(businessId, date, scheduleId) metodunu eklediğinden emin ol.
    this.reservationService.holdReservationSlot(this.businessId, this.selectedDate, sId).subscribe({
      next: (res) => {
        if (res.success) {
          this.selectedSlot = slot;
          this.selectedFieldName = fieldName;
          this.cardNumber = ''; 
          this.errorMessage = '';
          
          // Timestamp mantığı: Şu anki saat + 5 Dakika
          this.myActiveHold = {
            scheduleId: sId,
            expiresAt: Date.now() + (5 * 60 * 1000) 
          };
          
          this.startCountdown();
          this.isModalOpen.set(true);
        }
      },
      error: (err) => {
        // Geç kalmış olabiliriz, biz tıklayana kadar başkası almış olabilir
        alert(err.error?.message || "Bu saha az önce başka bir kullanıcı tarafından işlem görmeye başladı!");
        this.heldScheduleIds.update(ids => [...ids, sId]); // UI'ı turuncuya zorla
      }
    });
  }


  private startCountdown() {
    if (this.timerInterval) clearInterval(this.timerInterval);

    this.timerInterval = setInterval(() => {
      if (!this.myActiveHold) {
        clearInterval(this.timerInterval);
        return;
      }

      const timeLeft = this.myActiveHold.expiresAt - Date.now();

      if (timeLeft <= 0) {
        // Süre Doldu!
        clearInterval(this.timerInterval);
        this.myActiveHold = null;
        this.isModalOpen.set(false);
        this.countdownText.set('00:00');
        alert("Rezervasyon süreniz doldu! Saha tekrar boşa çıktı.");
      } else {
        const minutes = Math.floor(timeLeft / 60000);
        const seconds = Math.floor((timeLeft % 60000) / 1000);
        this.countdownText.set(`${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`);
      }
    }, 1000);
  }


   redirectToLogin() {
    this.isLoginModalOpen.set(false);
    this.router.navigate(['/auth/login']);
  }

  closeModal() {
    this.isModalOpen.set(false);
    
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
        this.myActiveHold = null;
        if (this.timerInterval) clearInterval(this.timerInterval);


        this.closeModal();
        this.selectedSlot = null;
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