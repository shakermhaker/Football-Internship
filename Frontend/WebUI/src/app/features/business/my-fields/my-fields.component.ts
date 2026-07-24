import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MyfieldsCard } from '../../../shared/components/myfields-card/myfields-card.component';
import { FootballFieldService } from '../../../core/services/football-field.service';
import { UserService } from '../../../core/services/user.service';
import { BusinessService } from '../../../core/services/business.service';


@Component({
  selector: 'app-my-fields',
  standalone: true,
  imports: [CommonModule, RouterLink, MyfieldsCard], // MyfieldsCard eklendi
  templateUrl: './my-fields.component.html'
})
export class MyFieldsComponent implements OnInit {
  private fieldService = inject(FootballFieldService);
  private businessService = inject(BusinessService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);

  userService = inject(UserService); 
  user = this.userService.currentUser();

  // Sahte diziyi kaldırdık, yerine boş bir liste koyduk
  myFields: any[] = [];
  isLoading: boolean = true;

  ngOnInit() {
    this.loadMyFields();
  }

  loadMyFields() {
    const bId = this.user?.businessId; 

    if (!bId) {
      console.error("Kullanıcının işletme ID'si bulunamadı!");
      this.isLoading = false;
      return;
    }

    this.businessService.getBusinessesFields(bId).subscribe({
      next: (response: any) => {
        // 1. Veriyi atadın
        this.myFields = response.data; 
        console.log("Yüklenen sahalar:", this.myFields);
        this.isLoading = false;
        
        // 2. 🎯 ANGULAR'A EKRANI ZORLA GÜNCELLEMESİNİ SÖYLÜYORUZ!
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        console.error("Sahalar yüklenirken hata:", err);
        this.isLoading = false;
        this.cdr.detectChanges(); // Hata durumunda da ekranı güncelle
      }
    });
  }
  goToEdit(fieldId: number) {
    console.log("Düzenlenecek Saha ID:", fieldId);
    // Yönlendirme adresimiz: business-panel/my-fields/edit/5 gibi olacak
    this.router.navigate(['/business-panel/my-fields/edit', fieldId]); 
  }
  deleteField(fieldId: number) {
    if (confirm("Bu halı sahayı ve ona bağlı tüm fiyat/saat programlarını silmek istediğinize emin misiniz?")) {
      console.log("Silme isteği atılıyor... Saha ID:", fieldId);
      
      // Servisi çağırıp ID'yi yolluyoruz (servisin adını kendi injeksiyonuna göre düzenle)
      this.fieldService.deleteField(fieldId).subscribe({
        next: (response: any) => {
          console.log("Silme başarılı:", response);
          
          // JİLET GİBİ BİR DOKUNUŞ: Ekranı yenilemeden silinen kartı arayüzden uçuruyoruz
          this.myFields = this.myFields.filter(f => f.id !== fieldId);
          this.cdr.detectChanges(); // Arayüze "güncellen" diyoruz
        },
        error: (err) => {
          console.error("Silinirken hata oluştu:", err);
          alert("Saha silinirken bir hata oluştu.");
        }
      });
    }
  }
}
