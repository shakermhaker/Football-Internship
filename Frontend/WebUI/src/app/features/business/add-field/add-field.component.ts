import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormArray, Validators } from '@angular/forms';
import { FootballFieldService } from '../../../core/services/football-field.service';
import { UserService } from '../../../core/services/user.service';
import { Router, RouterLink } from '@angular/router'; 
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-field.component.html',
  styleUrl: './add-field.component.scss' // SCSS kullanıyoruz
})
export class AddFieldComponent implements OnInit {
  private fb = inject(FormBuilder);
  private fieldService = inject(FootballFieldService);
  private userService = inject(UserService);
  private router = inject(Router);

  // Günlerin Listesi
  days = [
    { id: 1, name: 'Pazartesi' }, { id: 2, name: 'Salı' },
    { id: 3, name: 'Çarşamba' }, { id: 4, name: 'Perşembe' },
    { id: 5, name: 'Cuma' }, { id: 6, name: 'Cumartesi' }, { id: 7, name: 'Pazar' }
  ];

  // Ana Form
  fieldForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    // Dış katman: Grupların listesi
    scheduleGroups: this.fb.array([]) 
  });

  ngOnInit() {
    // Sayfa açıldığında 1 tane boş grup gelsin
    this.addScheduleGroup();
  }

  // --- GETTER METOTLARI (HTML'de form elemanlarına rahat ulaşmak için) ---
  get scheduleGroups() {
    return this.fieldForm.get('scheduleGroups') as FormArray;
  }

  getPeriods(groupIndex: number) {
    return this.scheduleGroups.at(groupIndex).get('periods') as FormArray;
  }

  // ==========================================
  // 1. KATMAN: GRUP İŞLEMLERİ (Yeşil İkonlar)
  // ==========================================
  
  addScheduleGroup() {
    const group = this.fb.group({
      selectedDayIds: [[], Validators.required], // Bu gruba ait seçili günler
      periods: this.fb.array([this.createPeriod()]) // Yeni gruba en az 1 periyot (satır) ekle
    });
    this.scheduleGroups.push(group);
  }

  removeScheduleGroup(index: number) {
    this.scheduleGroups.removeAt(index);
  }

  // ==========================================
  // 2. KATMAN: PERİYOT İŞLEMLERİ (Turuncu İkonlar)
  // ==========================================
  
  createPeriod() {
    return this.fb.group({
      startTime: ['09:00', Validators.required],
      endTime: ['13:00', Validators.required],
      duration: [60, Validators.required],
      price: [1000, Validators.required]
    });
  }

  addPeriod(groupIndex: number) {
    this.getPeriods(groupIndex).push(this.createPeriod());
  }

  removePeriod(groupIndex: number, periodIndex: number) {
    this.getPeriods(groupIndex).removeAt(periodIndex);
  }

  // ==========================================
  // GÜN SEÇİMİ VE KONTROL MANTIĞI
  // ==========================================

  toggleDay(groupIndex: number, dayId: number, event: any) {
    const group = this.scheduleGroups.at(groupIndex);
    let selectedDays = group.get('selectedDayIds')?.value as number[];

    if (event.target.checked) {
      selectedDays.push(dayId);
    } else {
      selectedDays = selectedDays.filter((id: number) => id !== dayId);
    }
    
    group.get('selectedDayIds')?.setValue(selectedDays);
  }

  // KURAL: Eğer bir gün başka bir grupta seçildiyse, bu grupta disabled (pasif) olsun
  isDayDisabled(currentGroupIndex: number, dayId: number): boolean {
    const allGroups = this.scheduleGroups.value;
    return allGroups.some((group: any, index: number) => {
      // Kendi grubu dışındaki diğer gruplara bak, bu dayId orada var mı?
      return index !== currentGroupIndex && group.selectedDayIds.includes(dayId);
    });
  }

  // ==========================================
  // ZAMAN PARÇALAMA YARDIMCI METOTLARI
  // ==========================================
  
  // "09:30" stringini alıp matematikle hesaplanabilir dakikaya (570) çevirir
  private timeToMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
  }

  // 570 dakikayı alıp tekrar "09:30" string formatına çevirir
  private minutesToTime(minutes: number): string {
    const h = Math.floor(minutes / 60).toString().padStart(2, '0');
    const m = (minutes % 60).toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  // ==========================================
  // FORMU KAYDET VE PARÇALA
  // ==========================================

  onSubmit() {
    if (this.fieldForm.valid) {
      const rawData = this.fieldForm.value;

      // 1. SIGNAL'DEN KULLANICI ID'SİNİ ÇEKİYORUZ
      // (Signal değişkeninin adı senin servisinde neyse onu yaz, örn: this.userService.businessId() veya currentUser().id )
      const currentBusinessId = this.userService.currentUser()?.businessId;

      // 2. BACKEND'E GİDECEK KUSURSUZ JSON'I İNŞA EDİYORUZ
      const payload = {
        name: rawData.name, 
        businessId: currentBusinessId, // Signal'den aldığımız ID'yi buraya çaktık
        scheduleGroups: rawData.scheduleGroups.map((group: any) => {
          
          let splitPeriods: any[] = []; 

          group.periods.forEach((period: any) => {
            let currentMin = this.timeToMinutes(period.startTime);
            const endMin = this.timeToMinutes(period.endTime);
            const duration = period.duration;

            // Saati maç süresine göre parçala
            while (currentMin + duration <= endMin) {
              splitPeriods.push({
                startTime: this.minutesToTime(currentMin),
                endTime: this.minutesToTime(currentMin + duration),
                duration: duration,
                price: period.price
              });
              
              currentMin += duration; 
            }
          });

          return {
            selectedDayIds: group.selectedDayIds,
            periods: splitPeriods 
          };
        })
      };

      console.log("🚀 BACKEND'E GİDECEK DATA:", payload);

      // 3. SERVİSE İSTEK AT VE SWEETALERT İLE SONUCU GÖSTER
      this.fieldService.addWithSchedules(payload).subscribe({
        next: (response) => {
          Swal.fire({
            title: 'Harika!',
            text: 'Halı saha ve tüm rezervasyon periyotları başarıyla eklendi.',
            icon: 'success',
            confirmButtonText: 'Tamam',
            confirmButtonColor: '#50cd89' // Metronic yeşili
          }).then((result) => {
            if (result.isConfirmed) {
              // İsteğe bağlı: Başarılı olunca formu sıfırla veya başka sayfaya yönlendir
              this.fieldForm.reset();
              this.router.navigate(['/business-panel/my-fields']);
              // this.addScheduleGroup(); // Sıfırlandıktan sonra 1 tane boş grup eklemek için
            }
          });
        },
        error: (err) => {
          Swal.fire({
            title: 'Eyvah!',
            text: err.error?.message || 'Kayıt sırasında beklenmeyen bir hata oluştu.',
            icon: 'error',
            confirmButtonText: 'Kapat',
            confirmButtonColor: '#f1416c' // Metronic kırmızısı
          });
          console.error("❌ HATA:", err);
        }
      });

    } else {
      // FORM GEÇERSİZSE UYARI VER
      Swal.fire({
        title: 'Eksik Bilgi!',
        text: 'Lütfen saha adını ve tüm rezervasyon alanlarını eksiksiz doldurun.',
        icon: 'warning',
        confirmButtonText: 'Anladım',
        confirmButtonColor: '#fd7e14' // Metronic turuncusu
      });

      this.fieldForm.markAllAsTouched();

      // Hangi alanın boş olduğunu konsola yazdır (Senin geliştirme yaparken görmen için)
      Object.keys(this.fieldForm.controls).forEach(key => {
        const control = this.fieldForm.get(key);
        if (control?.invalid) {
          console.error(`DİKKAT: '${key}' alanı hatalı veya boş! Durum:`, control.errors);
        }
      });
    }
  }
}