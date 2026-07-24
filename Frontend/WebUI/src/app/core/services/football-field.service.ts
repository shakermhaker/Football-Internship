import { Injectable, inject, PLATFORM_ID } from '@angular/core'; // PLATFORM_ID eklendi
import { isPlatformBrowser } from '@angular/common'; // isPlatformBrowser eklendi
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap,catchError , of} from 'rxjs';
import { UserService } from './user.service'; // 🎯 Kullanıcı sinyalini tetiklemek için ekledik

@Injectable({
  providedIn: 'root'
})
export class FootballFieldService {
  private http = inject(HttpClient);
  
  // Kendi backend portuna göre burayı düzenle (Örn: https://localhost:7123/api)
  private apiUrl = 'https://localhost:7074/api/Fields'; 
  userService = inject(UserService); // 🎯 Kullanıcı sinyalini tetiklemek için ekledik
  user = this.userService.currentUser(); // 🎯 Kullanıcı sinyalini tetiklemek için ekledik

  // Parçalanmış kusursuz datamızı backend'e fırlatan metot
  addWithSchedules(fieldData: any): Observable<any> {
    
    // İŞTE EKSİK OLAN SİHİRLİ DOKUNUŞ BURADA: { withCredentials: true }
    return this.http.post(`${this.apiUrl}/addwithschedules`, fieldData, {
      withCredentials: true 
    });
  }
  // Sahayı silme isteği
  deleteField(fieldId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deletefield?fieldId=${fieldId}`, {
      withCredentials: true 
    });
  }
  getFieldById(fieldId: number): Observable<any> {
  // DİKKAT: Buradaki adresi getfieldforedit yaptık!
  return this.http.get(`${this.apiUrl}/getfieldforedit?fieldId=${fieldId}`, {
    withCredentials: true
  });
}

  // ==========================================
  // 2. DEĞİŞTİRİLEN SAHAYI BACKEND'E GÜNCELLEMEYE YOLLA
  // ==========================================
  
  updateField(fieldId: number, payload: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/updatefieldwithschedules?fieldId=${fieldId}`, payload, {
    withCredentials: true
  });
}
  
}