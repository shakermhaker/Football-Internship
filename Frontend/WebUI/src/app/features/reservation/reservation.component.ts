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

  pageAlert = signal<{ message: string, title: string, type: string } | null>(null);
  private alertTimeout: any;


  private hubConnection!: signalR.HubConnection;
  heldScheduleIds = signal<{scheduleId: number, date: string}[]>([]);
  myActiveHold: { 
  scheduleId: number, 
  date: string, // 🚀 YENİ
  expiresAt: number, 
  slotName: string, 
  price: number 
} | null = null;
  countdownText = signal<string>('05:00');
  private timerInterval: any;

  isModalOpen = signal<boolean>(false);
  isLoginModalOpen = signal<boolean>(false);
  isCancelingHold: boolean = false;

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
      this.restoreHoldState();
      this.fetchBusinessDetails(this.businessId);

      if (!this.selectedDate) {
         const today = new Date();
         this.selectedDate = today.toISOString().split('T')[0];
      }


      this.fetchSchedules(this.businessId, this.selectedDate);
      this.fetchBookedSlots(this.businessId, this.selectedDate);
      this.fetchHeldSlots(this.businessId, this.selectedDate);
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
      // Gelen kilit sinyali BİZE ait değilse listeye (turuncuya) ekle
      if (!(this.myActiveHold?.scheduleId === data.scheduleId && this.myActiveHold?.date === data.date)) {
        this.heldScheduleIds.update(holds => [...holds, { scheduleId: data.scheduleId, date: data.date }]);
      }
    });

    // 2. Biri ödemeyi tamamladı (Booked)
    this.hubConnection.on('SlotBooked', (data: { scheduleId: number, date: string }) => {
      // Başkası kilitlediği slotu satın aldıysa turuncu (işlemde) listesinden çıkar
      this.heldScheduleIds.update(holds => holds.filter(h => !(h.scheduleId === data.scheduleId && h.date === data.date)));
      
      // Ve kırmızı (dolu) listesine ekle (Eğer ekrandaki tarihe aitse)
      if (data.date === this.selectedDate) {
        this.bookedScheduleIds.update(ids => [...ids, data.scheduleId]);
      }
    });

    // 3. Süre bitti veya sepetten çıkardı (Freed)
    this.hubConnection.on('SlotFreed', (data: { scheduleId: number, date: string }) => {
      // Süre dolduysa veya vazgeçildiyse turuncu listesinden çıkar
      this.heldScheduleIds.update(holds => holds.filter(h => !(h.scheduleId === data.scheduleId && h.date === data.date)));
    });

    this.hubConnection.on('SlotUnlocked', (data: { scheduleId: number, date: string }) => {
      if (data.date === this.selectedDate) {
        // Turuncudan (İşlemden) çıkar, tekrar Yeşile (Boş) döndür
        this.heldScheduleIds.update(ids => ids.filter(h => !(h.scheduleId === data.scheduleId && h.date === data.date)));
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
      this.fetchHeldSlots(this.businessId, newDate);
    }
  }

  fetchHeldSlots(businessId: number, dateStr: string) {
    this.reservationService.getHeldScheduleIdsByDate(businessId, dateStr).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          // Gelen düz ID listesini, bizim objeli state yapımıza çeviriyoruz
          const formattedHolds = res.data.map(id => ({ scheduleId: id, date: dateStr }));
          this.heldScheduleIds.set(formattedHolds);
        }
      },
      error: (err) => console.error('İşlemdeki slotlar çekilirken hata:', err)
    });
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
    return this.heldScheduleIds().some(x => x.scheduleId === scheduleId && x.date === this.selectedDate);
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



  showPageAlert(message: string, title: string = 'Uyarı', type: string = 'warning') {
    this.pageAlert.set({ message, title, type });
    
    if (this.alertTimeout) clearTimeout(this.alertTimeout);
    
    // 5 saniye sonra otomatik gizle
    this.alertTimeout = setTimeout(() => {
      this.pageAlert.set(null);
    }, 5000);
  }

  // 🚀 YENİ METOT: Kullanıcı alert'i çarpıdan kendi kapatmak isterse
  closePageAlert() {
    this.pageAlert.set(null);
    if (this.alertTimeout) clearTimeout(this.alertTimeout);
  }

  isSlotHeldByOthers(scheduleId: number): boolean {
    if (this.myActiveHold?.scheduleId === scheduleId && this.myActiveHold?.date === this.selectedDate) {
      return false; // Bu kilit bana ait, başkasına değil!
    }
    return this.heldScheduleIds().some(x => x.scheduleId === scheduleId && x.date === this.selectedDate);
  }

   onSlotSelected(slot: PriceScheduleDto, fieldName: string) {
    const sId = slot.fieldPriceScheduleId;

    // 1. Zaten Kesin Doluysa (Kırmızı) tıklanamaz
    if (this.isSlotBooked(sId)) return; 
    
    // 2. Başkası tarafından tutuluyorsa (Turuncu) tıklanamaz
    if (this.isSlotHeldByOthers(sId)) {
      this.showPageAlert("Bu saha şu anda başka biri tarafından işlem görüyor.", "Saha Müsait Değil", "warning");
      return;
    }

    // 3. GİRİŞ KONTROLÜ
    const user = this.userService.currentUser();
    if (!user) {
      this.isLoginModalOpen.set(true); 
      return;
    }

    // 4. EĞER BU SLOT ZATEN KENDİ İŞLEMİMDEYSE (Yeşil - "SİZDE") -> Sadece modalı geri aç
    if (this.myActiveHold && this.myActiveHold.scheduleId === sId) {
      this.selectedSlot = slot;
      this.selectedFieldName = fieldName;
      this.reopenModal(); 
      return;
    }

    // 5. EĞER BAŞKA BİR SLOT İŞLEMİMDEYSE -> Yeni bir tane almasına izin verme
    if (this.myActiveHold) {
       this.showPageAlert("Zaten işlemde olan bir rezervasyonunuz var. Lütfen önce onu tamamlayın veya iptal edin.", "İşlem Devam Ediyor");
       return;
    }

    // 6. İLK DEFA TIKLIYORSA -> API'ye Geçici Kilit (Hold) isteği at!
    this.reservationService.holdReservationSlot(this.businessId, this.selectedDate, sId).subscribe({
      next: (res) => {
        if (res.success) {
          this.selectedSlot = slot;
          this.selectedFieldName = fieldName;
          this.cardNumber = ''; 
          this.errorMessage = '';
          
          // Kilit bilgilerini oluştur
          this.myActiveHold = {
            scheduleId: sId,
            date: this.selectedDate,
            expiresAt: Date.now() + (5 * 60 * 1000),
            slotName: `${this.formatTime(slot.startTime)} - ${this.formatTime(slot.endTime)}`, 
            price: slot.price 
          };

          // F5 (Sayfa Yenileme) atılırsa unutmamak için tarayıcı hafızasına yaz!
          localStorage.setItem('ff_active_hold', JSON.stringify({
            businessId: this.businessId,
            selectedDate: this.selectedDate,
            selectedFieldName: this.selectedFieldName,
            selectedSlot: this.selectedSlot,
            myActiveHold: this.myActiveHold
          }));
          
          this.startCountdown();
          this.isModalOpen.set(true);
        }
      },
      error: (err) => {
         const errorMsg = err.error?.message || "Bu saha az önce başka bir kullanıcı tarafından işlem görmeye başladı!";
         this.showPageAlert(errorMsg, "Saha Müsait Değil", "warning");
         // Senkronizasyon kaçmışsa (Örn: SignalR o an kopmuşsa) manuel olarak turuncu listesine ekle
         this.heldScheduleIds.update(holds => [...holds, { scheduleId: sId, date: this.selectedDate }]);
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
        this.showPageAlert("Rezervasyon süreniz doldu! Saha tekrar boşa çıktı.", "Süre Bitti", "danger");
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


  
  cancelMyHold() {
    if (!this.myActiveHold) return;

    const { scheduleId, date } = this.myActiveHold;
    
    // 1. ANINDA EKRANI TEMİZLE (Kullanıcı bekletilmez, banner hemen yok olur)
    const slotIdToUnlock = scheduleId;
    this.clearMyHoldState(); 

    // 2. ARKA PLANDA SUNUCUYA BİLDİR (Cevap beklemeyiz, yangını söndürür)
    this.reservationService.cancelHoldSlot(this.businessId, date, slotIdToUnlock).subscribe({
      next: (res) => {
        console.log("Sunucu kilidi başarıyla kaldırdı.");
      },
      error: (err) => {
        console.error("Sunucu tarafında iptal hatası (Ama frontend temizlendi):", err);
      }
    });
  }

  // 🚀 YARDIMCI METOT: Sepet ve Sayaç Temizleme
  private clearMyHoldState() {
    // İptal edilen slotun ID'sini saklayalım ki listelerden hemen uçurabilelim
    const releasedScheduleId = this.myActiveHold?.scheduleId;

    this.myActiveHold = null;
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.isModalOpen.set(false);
    this.isCancelingHold = false;
    
    localStorage.removeItem('ff_active_hold'); // Hafızadan sil

    // 🚀 SIFIR EKSTRA DB YÜKÜ: Sadece lokal sinyallerden bu ID'yi filtreleyip çıkarıyoruz!
    if (releasedScheduleId) {
      this.heldScheduleIds.update(holds => holds.filter(h => h.scheduleId !== releasedScheduleId));
    }
  }

  private restoreHoldState() {
    const savedData = localStorage.getItem('ff_active_hold');
    if (savedData) {
      const parsedData = JSON.parse(savedData);
      
      // 1. Bu kilit bu işletmeye mi ait? VE 2. Süresi hala dolmamış mı?
      if (parsedData.businessId === this.businessId && parsedData.myActiveHold.expiresAt > Date.now()) {
        
        console.log("F5 atıldı, yarım kalan işlem kurtarıldı!");
        
        this.selectedDate = parsedData.selectedDate;
        this.selectedFieldName = parsedData.selectedFieldName;
        this.selectedSlot = parsedData.selectedSlot;
        this.myActiveHold = parsedData.myActiveHold;
        
        this.startCountdown();
        
      } else {
        // Süresi geçmiş veya başka işletmeye aitse çöpü temizle
        localStorage.removeItem('ff_active_hold');
      }
    }
  }

  closeModal() {
    this.isModalOpen.set(false);
    
  }

  reopenModal() {
    if (this.myActiveHold) {
      this.isModalOpen.set(true);
    }
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
        this.clearMyHoldState();
        this.selectedSlot = null;
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