import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DataResult } from './user.service';

// Backend DTO Karşılıkları
export interface PriceScheduleDto {
  fieldPriceScheduleId: number;
  dayId: number;
  dayName: string;
  timeSlotId: number;
  startTime: string; 
  endTime: string;
  price: number;
}

export interface FootballFieldScheduleDto {
  footballFieldId: number;
  footballFieldName: string;
  schedules: PriceScheduleDto[];
}
export interface CreateReservationDto {
  fieldPriceScheduleId: number;
  reservationDate: string;
  finalPrice: number;
  cardNumber: string; // Şimdilik temsili ödeme için
}

export interface UserReservationDetailDto {
  reservationId: number;
  reservationDate: string;
  startTime: string;
  endTime: string;
  footballFieldName: string;
  businessId: number;
  businessName: string;
  cityName: string;
  districtName: string;
  finalPrice: number;
  statusName: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7074/api/Reservations'; 

  getBusinessFieldSchedules(businessId: number, dateStr: string): Observable<DataResult<FootballFieldScheduleDto[]>> {
    return this.http.get<DataResult<FootballFieldScheduleDto[]>>(`${this.apiUrl}/getbusinessfieldschedules?businessId=${businessId}&date=${dateStr}`);
  }


  getBookedScheduleIdsByDate(businessId: number, dateStr: string): Observable<DataResult<number[]>> {
    return this.http.get<DataResult<number[]>>(`${this.apiUrl}/getbookedids?businessId=${businessId}&date=${dateStr}`);
  }

  createReservation(dto: CreateReservationDto): Observable<any> {
    // withCredentials: true ekliyoruz ki eğer token cookie'deyse güvenle backend'e gitsin.
    // Interceptor kullanıyorsan buna gerek kalmayabilir ama zarar vermez.
    return this.http.post(`${this.apiUrl}/create`, dto, { withCredentials: true });
  }

  getUserReservations(): Observable<DataResult<UserReservationDetailDto[]>> {
    // Token ile (Cookie) güvenli bir şekilde gidiyoruz
    return this.http.get<DataResult<UserReservationDetailDto[]>>(`${this.apiUrl}/my-reservations`, { withCredentials: true });
  }
  cancelReservation(reservationId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/cancel/${reservationId}`, {}, { withCredentials: true });
  }
}