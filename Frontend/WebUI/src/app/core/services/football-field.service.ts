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

  // Parçalanmış kusursuz datamızı backend'e fırlatan metot
  addWithSchedules(fieldData: any): Observable<any> {
    
    // İŞTE EKSİK OLAN SİHİRLİ DOKUNUŞ BURADA: { withCredentials: true }
    return this.http.post(`${this.apiUrl}/addwithschedules`, fieldData, {
      withCredentials: true 
    });
    
  }
}