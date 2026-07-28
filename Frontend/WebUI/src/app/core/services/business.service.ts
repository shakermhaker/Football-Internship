import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { DataResult, UserService } from './user.service';

export interface MonthlyRevenueDto {
  month: number;
  monthName: string;
  revenue: number;
  reservationCount: number;
}

export interface FieldRevenueDto {
  fieldId: number;
  fieldName: string;
  totalRevenue: number;
  reservationCount: number;
  monthlyRevenues: MonthlyRevenueDto[]; // Sahanın aylık kırılımı
}

export interface BusinessDashboardDto {
  totalRevenueThisYear: number;
  totalRevenueThisMonth: number;
  totalRevenueThisWeek: number;
  totalReservationsThisMonth: number;
  fieldRevenues: FieldRevenueDto[];
  monthlyRevenues: MonthlyRevenueDto[];
}

export interface BusinessImageDto {
  id: number;
  imagePath: string;
  isCover: boolean;
}

export interface BusinessDetailDto {
  businessId: number;
  name: string;
  city: string;
  district: string;
  fullAddress: string;
  images: BusinessImageDto[];
}


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
getBusinessDetails(businessId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/getdetailsbyid?businessId=${businessId}`, {
      withCredentials: true
    });
  }

  // 2. Kapak veya galeri resmi yükleyen metot (FormData ile)
  addBusinessImage(businessId: number, file: File, isCover: boolean): Observable<any> {
    const formData: FormData = new FormData();
    formData.append('businessId', businessId.toString());
    formData.append('file', file, file.name);
    formData.append('isCover', isCover.toString());

    return this.http.post<any>(`${this.apiUrl}/addimage`, formData, {
      withCredentials: true
    });
  }

  updateBusinessDetails(updateData: any): Observable<any> {
    // API URL'in ve controller ismin projedeki yapıya göre (örneğin /Business/update) olmalı
    return this.http.put(`${this.apiUrl}/update`, updateData, {
      withCredentials: true
    });
  }
  
  deleteBusinessImage(imageId: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/deleteimage?imageId=${imageId}`, {
    withCredentials: true
  });
}
  

  getDashboardSummary(businessId: number, year: number): Observable<DataResult<BusinessDashboardDto>> {
    return this.http.get<DataResult<BusinessDashboardDto>>(
      `${this.apiUrl}/dashboard-summary?businessId=${businessId}&year=${year}`,
      { withCredentials: true }
    );
  }
}
