import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';


export interface UserProfile {
  rowGuid: string;
  firstName: string;
  lastName: string;
  email: string;
  status: boolean;

  hasBusiness: boolean;
  isBusinessApproved: boolean;
  businessName: string;
  businessId: number;
}
export interface DataResult<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root' 
})
export class UserService {
  
  private http = inject(HttpClient); 
  
  
  private apiUrl = 'https://localhost:7074/api/User'; 

  
  
  currentUser = signal<UserProfile | null>(null);

 
  fetchMyProfile(): Observable<UserProfile> {
    
    return this.http.get<DataResult<UserProfile>>(`${this.apiUrl}/myProfile`, { 
      withCredentials: true 
    }).pipe(
      map((response) => response.data),
      
      tap((profile) => {
        console.log('Paketten çıkarılan saf profil verisi:', profile);
        this.currentUser.set(profile); 
      })
    );
  }

  /**
   * Çıkış yapıldığında hafızayı temizlemek için kullanacağımız yardımcı metot.
   */
  clearUser() {
    this.currentUser.set(null);
  }
}