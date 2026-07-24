import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { DataResult, UserService } from './user.service';


@Injectable({ providedIn: 'root' })
export class BusinessService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7074/api/Business';


   userService = inject(UserService); // 🎯 Kullanıcı sinyalini tetiklemek için ekledik
    user = this.userService.currentUser(); // 🎯 Kullanıcı sinyalini tetiklemek için ekledik

  // İşletme kaydetme isteği
  addBusiness(businessData: any): Observable<any> {
    // Token ile işlem yaptığımız için withCredentials veya interceptor ayarlarına dikkat
    return this.http.post<any>(`${this.apiUrl}/add`, businessData, { withCredentials: true });
  }
  
  getFilteredBusinesses(cityId: number | null, districtId: number | null, search: string): Observable<DataResult<any[]>> {
  let params = new HttpParams();

  // cityId doluysa mutlaka params'a eklenmeli
  if (cityId) {
    params = params.append('cityId', cityId.toString());
  }
  
  if (districtId) {
    params = params.append('districtId', districtId.toString());
  }

  if (search && search.trim() !== '') {
    params = params.append('search', search.trim());
  }

  return this.http.get<DataResult<any[]>>(`${this.apiUrl}/getall`, { params });
}

getBusinessesFields(businessId: number): Observable<any> {
    // URL'in sonuna ?businessId= parametresini dinamik olarak ekliyoruz
    return this.http.get(`${this.apiUrl}/getallfields?businessId=${businessId}`, {
      withCredentials: true 
    });
  }
}
