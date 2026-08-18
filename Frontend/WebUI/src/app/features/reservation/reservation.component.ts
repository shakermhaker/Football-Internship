import { Component, OnInit,OnDestroy, inject, signal, } from '@angular/core';
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

export interface UIDynamicField {
  fieldId: number;
  fieldName: string;
  dynamicDays: UIDynamicDay[];
}

export interface UIDynamicDay {
  date: string;       // API'ye ve Redis'e gidecek "2026-08-11" formatı
  displayDate: string; // Ekranda yazacak "11.08.2026" formatı
  dayName: string;    // "Salı"
  dayId: number;      // 2
  slots: any[];       // O güne ait düz (flat) listeden filtrelenmiş saatler
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
  groupedFields = signal<FootballFieldScheduleDto[]>([]);
  isLoading = signal<boolean>(true);
  selectedDate: string = '';
  minDate: string = ''; 
  bookedScheduleIds = signal<{scheduleId: number, date: string}[]>([]);
  businessId: number = 0;
  
  freeRightCount = signal<number>(0);
  useFreeRight = signal<boolean>(false);
  selectedFieldId: number = 0;

  pageAlert = signal<{ message: string, title: string, type: string } | null>(null);
  private alertTimeout: any;

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }


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
      
      // Eski 3 ayrı fetch yerine artık bunu çağırıyoruz
      this.fetchWeeklyData(this.selectedDate); 
      this.startSignalRConnection();
    }
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.invoke('LeaveBusinessGroup', this.businessId.toString()).catch(err => console.error(err));
      this.hubConnection.stop();
    }
    if (this.timerInterval) clearInterval(this.timerInterval);

    if (this.myActiveHold) {
      console.log("Kullanıcı sayfadan ayrıldı, kilit serbest bırakılıyor...");
      
    
      this.reservationService.cancelHoldSlot(this.businessId, this.myActiveHold.date, this.myActiveHold.scheduleId).subscribe();
      
    
      localStorage.removeItem('ff_active_hold');
    }
  }

  dynamicGroupedFields = signal<UIDynamicField[]>([]);

  // 🚀 GRUPLAMA VE TARİH ATAMA BEYNİ
  buildDynamicCalendar() {
    const fields = this.groupedFields(); // Backend'den gelen düz liste
    if (!fields || fields.length === 0) return;

    const startDate = new Date(this.selectedDate); // Takvimden seçilen gün
    const dynamicFields: UIDynamicField[] = [];

    // 1. ADIM: 7 Günlük tarih iskeletini oluştur
    const weekDates = [];
    for (let i = 0; i < 7; i++) {
      const currentDate = new Date(startDate);
      currentDate.setDate(startDate.getDate() + i);

      // C#'taki DayOfWeek ile eşleştir (Pzt:1, Pazar:7)
      let dayId = currentDate.getDay();
      if (dayId === 0) dayId = 7; 

      // UTC saat farkı yememek için manuel YYYY-MM-DD oluşturuyoruz
      const year = currentDate.getFullYear();
      const month = String(currentDate.getMonth() + 1).padStart(2, '0');
      const day = String(currentDate.getDate()).padStart(2, '0');
      
      const dateStr = `${year}-${month}-${day}`; // Redis ve DB için kritik tarih!
      const displayDate = currentDate.toLocaleDateString('tr-TR'); 

      weekDates.push({ dateStr, displayDate, dayId });
    }

    // 2. ADIM: Düz listeyi 7 günlük takvime dağıt
    for (const field of fields) {
      const dynamicDays: UIDynamicDay[] = [];

      for (const wd of weekDates) {
        
        // 🚀 BÜYÜNÜN KOPTUĞU YER: Sadece o güne (dayId) ait slotları filtrele
        const daySlots = field.schedules.filter((s: any) => s.dayId === wd.dayId);
        
        if (daySlots.length > 0) {
          // Saat sırasına göre diz (Emin olmak için)
          daySlots.sort((a: any, b: any) => a.startTime.localeCompare(b.startTime));

          dynamicDays.push({
            date: wd.dateStr,
            displayDate: wd.displayDate,
            dayName: daySlots[0].dayName,
            dayId: wd.dayId,
            slots: daySlots
          });
        }
      }

      dynamicFields.push({
        fieldId: field.footballFieldId,
        fieldName: field.footballFieldName,
        dynamicDays: dynamicDays
      });
    }

    // Olay tamam, HTML'in önüne hazır yemeği sunuyoruz
    this.dynamicGroupedFields.set(dynamicFields);
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
      
      // 🚀 DÜZELTME 1: Artık obje olarak ekliyoruz.
      // 🚀 DÜZELTME 2: İleri tarihli bir alım da olsa (7 günlük vitrinde görebilmek için) if kısıtlamasını kaldırdık.
      this.bookedScheduleIds.update(ids => [...ids, { scheduleId: data.scheduleId, date: data.date }]);
    });

    // 3. Süre bitti veya sepetten çıkardı (Freed)
    this.hubConnection.on('SlotFreed', (data: { scheduleId: number, date: string }) => {
      // Süre dolduysa veya vazgeçildiyse turuncu listesinden çıkar
      this.heldScheduleIds.update(holds => holds.filter(h => !(h.scheduleId === data.scheduleId && h.date === data.date)));
    });

    this.hubConnection.on('SlotUnlocked', (data: { scheduleId: number, date: string }) => {
      // 🚀 DÜZELTME 3: Tarih kısıtlamasını buradan da kaldırdık, hangi tarihin kilidi açılırsa açılsın yeşile dönsün.
      this.heldScheduleIds.update(ids => ids.filter(h => !(h.scheduleId === data.scheduleId && h.date === data.date)));
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



  // Kullanıcı takvimden yeni bir tarih seçtiğinde tetiklenir
  onDateChange(event: any) {
    const newDate = event.target.value;
    if (newDate && this.businessId) {
      this.selectedDate = newDate;
      // Tarih değişince yeni 7 günlük aralığı çek
      this.fetchWeeklyData(newDate); 
    }
  }

  



  fetchWeeklyData(startDateStr: string) {
    this.isLoading.set(true);

    const start = new Date(startDateStr);
    const end = new Date(start);
    end.setDate(start.getDate() + 6); // 6 gün ekle (toplam 7 gün)
    const endDateStr = this.formatDateForApi(end);

    // 1. Şablonu çek
    this.reservationService.getWeeklyTemplates(this.businessId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.groupedFields.set(res.data);
          
          // Şablon geldikten sonra dinamik takvimi hemen inşa et
          this.buildDynamicCalendar();

          // 2. Kırmızı slotları çek
          this.reservationService.getBookedSlotsByDateRange(this.businessId, startDateStr, endDateStr).subscribe(res2 => {
            if (res2.success && res2.data) {
              this.bookedScheduleIds.set(res2.data);
            } else {
              this.bookedScheduleIds.set([]);
            }
          });

          // 3. Sarı slotları çek
          this.reservationService.getHeldSlotsByDateRange(this.businessId, startDateStr, endDateStr).subscribe(res3 => {
            if (res3.success && res3.data) {
              this.heldScheduleIds.set(res3.data);
            } else {
              this.heldScheduleIds.set([]);
            }
          });
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Veriler çekilirken hata:', err);
        this.isLoading.set(false);
      }
    });
  }


  isSlotInPast(slotStartTime: string, slotDate: string): boolean {
    if (!slotStartTime || !slotDate) return false;

    const now = new Date();
    // Saat dilimi kaymalarını önleyerek bugünün tarihini YYYY-MM-DD formatında alıyoruz
    const todayStr = new Date(now.getTime() - (now.getTimezoneOffset() * 60000)).toISOString().split('T')[0];

    // Eğer kontrol edilen gün, geçmiş bir tarihse direkt true dön (saatleri gizle)
    if (slotDate < todayStr) return true;

    // Sadece kontrol edilen gün "Bugün" ise saat kontrolü yap
    if (slotDate === todayStr) {
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
  isSlotBooked(scheduleId: number, slotDate: string): boolean {
    // bookedSlots yerine bookedScheduleIds kullanıyoruz
    return this.bookedScheduleIds().some(x => x.scheduleId === scheduleId && x.date === slotDate);
  }

  isSlotHeld(scheduleId: number): boolean {
    return this.heldScheduleIds().some(x => x.scheduleId === scheduleId && x.date === this.selectedDate);
  }

  isMyHold(scheduleId: number): boolean {
    return this.myActiveHold?.scheduleId === scheduleId;
  }

  // 🚀 Backend'den gelen düz listeyi, günlere göre (Accordion için) gruplar
  



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

  isSlotHeldByOthers(scheduleId: number, slotDate: string): boolean {
    if (this.myActiveHold?.scheduleId === scheduleId && this.myActiveHold?.date === slotDate) {
      return false; // Kendi kilidim
    }
    // heldSlots yerine heldScheduleIds kullanıyoruz
    return this.heldScheduleIds().some(x => x.scheduleId === scheduleId && x.date === slotDate);
  }

   onSlotSelected(slot: PriceScheduleDto, fieldName: string, slotDate: string, fieldId: number) {
    const sId = slot.fieldPriceScheduleId;

    // 1. 🚀 slotDate parametresini içeriye gönderiyoruz
    if (this.isSlotBooked(sId, slotDate)) return;
    
    // 2. Başkası tarafından tutuluyorsa (Turuncu) tıklanamaz
    if (this.isSlotHeldByOthers(sId, slotDate)) {
      this.showPageAlert("Bu saha şu anda başka biri tarafından işlem görüyor.", "Saha Müsait Değil", "warning");
      return;
    }

  const user = this.userService.currentUser();
  if (!user) {
    this.isLoginModalOpen.set(true); 
    return;
  }

    // 4. EĞER BU SLOT ZATEN KENDİ İŞLEMİMDEYSE (Yeşil - "SİZDE") -> Sadece modalı geri aç
    if (this.myActiveHold && this.myActiveHold.scheduleId === sId && this.myActiveHold.date === slotDate) {
      this.selectedSlot = slot;
      this.selectedFieldName = fieldName;
      this.reopenModal(); 
      return;
    }

  if (this.myActiveHold) {
     this.showPageAlert("Zaten işlemde olan bir rezervasyonunuz var. Lütfen önce onu tamamlayın veya iptal edin.", "İşlem Devam Ediyor");
     return;
  }

  const savedHold = localStorage.getItem('ff_active_hold');
  if (savedHold) {
     const parsed = JSON.parse(savedHold);
     if (parsed.businessId !== this.businessId) {
         this.reservationService.cancelHoldSlot(parsed.businessId, parsed.selectedDate, parsed.myActiveHold.scheduleId).subscribe();
         localStorage.removeItem('ff_active_hold');
     }
  }

    // 6. İLK DEFA TIKLIYORSA -> API'ye Geçici Kilit (Hold) isteği at!
    this.reservationService.holdReservationSlot(this.businessId, slotDate, sId).subscribe({
      next: (res) => {
        if (res.success) {
          this.selectedSlot = slot;
          this.selectedFieldName = fieldName;
          
          // 🚀 SENİN DALINDAN GELEN KODLAR
          this.selectedFieldId = fieldId; 
          this.cardNumber = ''; 
          this.errorMessage = '';
          this.useFreeRight.set(false); 
          this.freeRightCount.set(0);

          // 🚀 BEDAVA HAK SORGUSU
          this.reservationService.checkFreeRights(fieldId).subscribe(rightRes => {
            if (rightRes.success) {
              this.freeRightCount.set(rightRes.data);
            }
          });
          
          // 🚀 DOĞRU TARİH (slotDate) İLE KİLİT OBJESİ
          this.myActiveHold = {
            scheduleId: sId,
            date: slotDate, 
            expiresAt: Date.now() + (5 * 60 * 1000),
            slotName: `${this.formatTime(slot.startTime)} - ${this.formatTime(slot.endTime)}`, 
            price: slot.price 
          };

          localStorage.setItem('ff_active_hold', JSON.stringify({
            businessId: this.businessId,
            selectedDate: slotDate, 
            selectedFieldName: this.selectedFieldName,
            selectedFieldId: this.selectedFieldId, 
            selectedSlot: this.selectedSlot,
            myActiveHold: this.myActiveHold
          }));
          
          this.startCountdown();
          this.isModalOpen.set(true);
        }
      },
      error: (err) => {
         // TEK BİR ERROR BLOĞU (Çakışma temizlendi)
         const errorMsg = err.error?.message || "Bu saha az önce başka bir kullanıcı tarafından işlem görmeye başladı!";
         this.showPageAlert(errorMsg, "Saha Müsait Değil", "warning");
         this.heldScheduleIds.update(holds => [...holds, { scheduleId: sId, date: slotDate }]);
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
    
    if (parsedData.businessId === this.businessId && parsedData.myActiveHold.expiresAt > Date.now()) {
      this.selectedDate = parsedData.selectedDate;
      this.selectedFieldName = parsedData.selectedFieldName;
      this.selectedSlot = parsedData.selectedSlot;
      this.selectedFieldId = parsedData.selectedFieldId || 0; // 🚀 Eklendi
      this.myActiveHold = parsedData.myActiveHold;
      
      // 🚀 F5 atıldıysa API'ye tekrar hakkı soruyoruz
      if (this.selectedFieldId > 0) {
        this.reservationService.checkFreeRights(this.selectedFieldId).subscribe(rightRes => {
          if (rightRes.success) this.freeRightCount.set(rightRes.data);
        });
      }

      this.startCountdown();
    } else {
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
    // 1. Temel Güvenlik (main'den gelen myActiveHold kontrolü eklendi)
    if (!this.selectedSlot || !this.myActiveHold) {
      this.errorMessage = 'Geçersiz işlem. Lütfen tekrar deneyin.';
      return;
    }
  
    // 2. 🚀 SENİN DALINDAN: Sadece Hak kullanmıyorsa kart kontrolü yap
    if (!this.useFreeRight() && !this.cardNumber.trim()) {
      this.errorMessage = 'Lütfen geçerli bir kart numarası giriniz.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    // 3. Payload Hazırlığı
    const payload: any = { // DTO'na useFreeRight eklediysen "any" yerine "CreateReservationDto" kullanabilirsin
      businessId: this.businessId,
      fieldPriceScheduleId: this.selectedSlot.fieldPriceScheduleId,
      
      // 🚀 main'den: Takvimin bugünü değil, tıklanan slotun GERÇEK tarihi
      reservationDate: this.myActiveHold.date, 
      
      finalPrice: this.selectedSlot.price,
      
      // Hak kullanılıyorsa backend'e kart numarası boş gidebilir
      cardNumber: this.useFreeRight() ? '' : this.cardNumber,
      
      // 🚀 YENİ: Backend'in bu rezervasyonun bedava hakla yapıldığını bilmesi için
      useFreeRight: this.useFreeRight() 
    };

    this.reservationService.createReservation(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        
        this.clearMyHoldState();
        this.selectedSlot = null;
        if (this.timerInterval) clearInterval(this.timerInterval);

        this.closeModal();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Rezervasyon oluşturulurken bir hata oluştu. Lütfen tekrar giriş yapıp deneyin.';
        console.error(err);
      }
    });
  }

  this.isSubmitting = true;
  this.errorMessage = '';

  const payload: CreateReservationDto = {
    businessId: this.businessId,
    fieldPriceScheduleId: this.selectedSlot.fieldPriceScheduleId,
    reservationDate: this.selectedDate,
    finalPrice: this.selectedSlot.price,
    cardNumber: this.useFreeRight() ? '' : this.cardNumber, // Hak kullanılıyorsa kart boş gider
    useFreeRight: this.useFreeRight() // 🚀 Eklendi
  };

  this.reservationService.createReservation(payload).subscribe({
    // ... next ve error blokları mevcut haliyle aynı kalıyor ...
    next: (res) => {
      this.isSubmitting = false;
      this.clearMyHoldState();
      this.selectedSlot = null;
      if (this.timerInterval) clearInterval(this.timerInterval);
      this.closeModal();
      this.fetchBookedSlots(this.businessId, this.selectedDate);
    },
    error: (err) => {
      this.isSubmitting = false;
      this.errorMessage = err.error?.message || 'Rezervasyon oluşturulurken bir hata oluştu.';
    }
  });
}

  // "18:00:00" string'ini "18:00" yapar
  formatTime(timeStr: string): string {
    if (!timeStr) return '';
    return timeStr.substring(0, 5); 
  }

  toggleFreeRight(event: any) {
  this.useFreeRight.set(event.target.checked);
  if (this.useFreeRight()) {
    this.cardNumber = ''; // Hak kullanılıyorsa kart bilgisini sıfırla
    this.errorMessage = '';
  }
}
}