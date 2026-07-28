import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BusinessService, BusinessDashboardDto } from '../../../core/services/business.service';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-business-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-analytics.component.html'
})
export class BusinessAnalyticsComponent implements OnInit {
  private businessService = inject(BusinessService);
  private userService = inject(UserService);

  // Sinyallerimiz
  isLoading = signal<boolean>(true);
  dashboardData = signal<BusinessDashboardDto | null>(null);
  
  // Yıl ve Ay Kontrolleri
  currentYear = new Date().getFullYear();
  selectedYear = signal<number>(this.currentYear);
  availableYears: number[] = [];
  
  // Varsayılan olarak bulunduğumuz ayı seçili getiririz (1-12)
  selectedMonth = signal<number>(new Date().getMonth() + 1); 
  
  // Aylar Listesi (Tablar için)
  months = [
    { id: 1, name: 'Ocak' }, { id: 2, name: 'Şubat' }, { id: 3, name: 'Mart' },
    { id: 4, name: 'Nisan' }, { id: 5, name: 'Mayıs' }, { id: 6, name: 'Haziran' },
    { id: 7, name: 'Temmuz' }, { id: 8, name: 'Ağustos' }, { id: 9, name: 'Eylül' },
    { id: 10, name: 'Ekim' }, { id: 11, name: 'Kasım' }, { id: 12, name: 'Aralık' }
  ];

  // Kullanıcı bir aya tıkladığında, sahaların O AYA AİT gelirlerini kolayca HTML'e basmak için Computed Signal yazıyoruz.
  selectedMonthFieldStats = computed(() => {
    const data = this.dashboardData();
    const monthId = this.selectedMonth();
    
    if (!data || !data.fieldRevenues) return [];

    // Her bir sahanın içine girip, sadece seçili aya ait veriyi çıkartıyoruz
    return data.fieldRevenues.map(field => {
      const monthData = field.monthlyRevenues.find(m => m.month === monthId);
      return {
        fieldName: field.fieldName,
        revenue: monthData ? monthData.revenue : 0,
        reservationCount: monthData ? monthData.reservationCount : 0
      };
    });
  });

  ngOnInit() {
    // Son 5 yılı filtre seçeneği olarak ekleyelim (Örn: 2026, 2025, 2024...)
    for (let i = 0; i < 5; i++) {
      this.availableYears.push(this.currentYear - i);
    }
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading.set(true);
    
    
    const user = this.userService.currentUser();
    
    const businessId = (user as any)?.businessId; 

    this.businessService.getDashboardSummary(businessId, this.selectedYear()).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.dashboardData.set(res.data);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Dashboard verileri çekilemedi:', err);
        this.isLoading.set(false);
      }
    });
  }

  onYearChange() {
    this.loadDashboardData();
  }

  selectMonth(monthId: number) {
    this.selectedMonth.set(monthId);
  }
}