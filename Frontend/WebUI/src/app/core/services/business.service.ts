import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BusinessService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7074/api/Business';

  // İşletme kaydetme isteği
  addBusiness(businessData: any): Observable<any> {
    // Token ile işlem yaptığımız için withCredentials veya interceptor ayarlarına dikkat
    return this.http.post<any>(`${this.apiUrl}/add`, businessData, { withCredentials: true });
  }
}