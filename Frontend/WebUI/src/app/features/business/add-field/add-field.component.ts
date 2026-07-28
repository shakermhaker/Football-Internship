import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormArray, Validators } from '@angular/forms';
import { FootballFieldService } from '../../../core/services/football-field.service';
import { UserService } from '../../../core/services/user.service';
import Swal from 'sweetalert2';
import { ActivatedRoute, Router } from '@angular/router'; // Router eklendi

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
  private route = inject(ActivatedRoute);
  private router = inject(Router); // İşlem bitince listeye dönmek için

  isEditMode = false; 
  editFieldId: number | null = null;
  
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
    // 1. URL'in sonunda ID var mı diye bakıyoruz (Örn: /edit/2)
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      
      if (id) {
        // ID varsa, burası DÜZENLEME (Edit) sayfasıdır!
        this.isEditMode = true;
        this.editFieldId = +id; // Sayıya çevirdik
        this.loadFieldData(this.editFieldId); // Backend'e koş ve veriyi getir
      } else {
        // ID yoksa, burası YENİ EKLEME sayfasıdır!
        this.isEditMode = false;
        this.addScheduleGroup(); // 1 tane boş grup ekle
      }
    });
  }

  // ==========================================
  // DÜZENLEME MODU: VERİYİ FORMA DOLDURMA
  // ==========================================
  loadFieldData(id: number) {
  this.fieldService.getFieldById(id).subscribe({
    next: (response: any) => {
      const fieldData = response.data;
      console.log("🔥 Düzenlenecek Saha Backend'den Geldi:", fieldData);

      // 1. Saha adını form kutucuğuna basıyoruz
      this.fieldForm.patchValue({
        name: fieldData.name // veya fieldData.Name (JSON'da küçük harf 'name' görünüyor)
      });

      // 2. Formdaki boş grupları temizliyoruz
      this.scheduleGroups.clear();

      // 3. Backend'den gelen grupları tek tek form array'ine ekliyoruz
      if (fieldData.scheduleGroups && fieldData.scheduleGroups.length > 0) {
        fieldData.scheduleGroups.forEach((group: any) => {
          
          // DİKKAT: fb.control ekledik ve camelCase/PascalCase hatasına karşı garantiye aldık!
          const groupForm = this.fb.group({
            // Backend'den gelen diziyi (array) güvenli bir şekilde FormControl içine atıyoruz:
            selectedDayIds: this.fb.control(group.selectedDayIds || group.SelectedDayIds || [], Validators.required),
            periods: this.fb.array([])
          });

          // Grubun içindeki periyotları ekliyoruz
          const periodsArray = groupForm.get('periods') as FormArray;
          const rawPeriods = group.periods || group.Periods || [];

          rawPeriods.forEach((period: any) => {
            const periodForm = this.fb.group({
              startTime: [period.startTime || period.StartTime, Validators.required],
              endTime: [period.endTime || period.EndTime, Validators.required],
              duration: [period.duration || period.Duration, Validators.required],
              price: [period.price || period.Price, Validators.required]
            });
            periodsArray.push(periodForm);
          });

          this.scheduleGroups.push(groupForm);
        });
      } else {
        this.addScheduleGroup();
      }
    },
    error: (err) => {
      console.error("Veri çekilirken hata:", err);
    }
  });
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

  isDayDisabled(currentGroupIndex: number, dayId: number): boolean {
    const allGroups = this.scheduleGroups.value;
    return allGroups.some((group: any, index: number) => {
      return index !== currentGroupIndex && group.selectedDayIds.includes(dayId);
    });
  }

  // ==========================================
  // ZAMAN PARÇALAMA YARDIMCI METOTLARI
  // ==========================================
  private timeToMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
  }

  private minutesToTime(minutes: number): string {
    const h = Math.floor(minutes / 60).toString().padStart(2, '0');
    const m = (minutes % 60).toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  // ==========================================
  // FORMU KAYDET VE PARÇALA (EKLEME & GÜNCELLEME)
  // ==========================================
  onSubmit() {
    if (this.fieldForm.valid) {
      const rawData = this.fieldForm.value;
      const currentBusinessId = this.userService.currentUser()?.businessId;

      const payload: any = {
        name: rawData.name, 
        businessId: currentBusinessId, 
        scheduleGroups: rawData.scheduleGroups.map((group: any) => {
          
          let splitPeriods: any[] = []; 

          group.periods.forEach((period: any) => {
            let currentMin = this.timeToMinutes(period.startTime);
            const endMin = this.timeToMinutes(period.endTime);
            const duration = period.duration;

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

      // 🔥 EĞER DÜZENLEME MODUNDAYSAK PAYLOAD'A ID EKLİYORUZ
      if (this.isEditMode) {
        payload.id = this.editFieldId;
      }

      console.log(this.isEditMode ? "🚀 GÜNCELLEME İÇİN GİDEN DATA:" : "🚀 YENİ KAYIT İÇİN GİDEN DATA:", payload);

      
      const request$ = this.isEditMode 
      ? this.fieldService.updateField(this.editFieldId!, payload) // Hem ID'yi hem payload'ı verdik!
      : this.fieldService.addWithSchedules(payload);

      request$.subscribe({
        next: (response) => {
          Swal.fire({
            title: 'Harika!',
            text: this.isEditMode ? 'Halı saha başarıyla güncellendi.' : 'Halı saha başarıyla eklendi.',
            icon: 'success',
            confirmButtonText: 'Tamam',
            confirmButtonColor: '#50cd89'
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
            text: err.error?.message || 'İşlem sırasında beklenmeyen bir hata oluştu.',
            icon: 'error',
            confirmButtonText: 'Kapat',
            confirmButtonColor: '#f1416c'
          });
          console.error("❌ HATA:", err);
        }
      });

    } else {
      Swal.fire({
        title: 'Eksik Bilgi!',
        text: 'Lütfen saha adını ve tüm rezervasyon alanlarını eksiksiz doldurun.',
        icon: 'warning',
        confirmButtonText: 'Anladım',
        confirmButtonColor: '#fd7e14' 
      });

      this.fieldForm.markAllAsTouched();
    }
  }
}