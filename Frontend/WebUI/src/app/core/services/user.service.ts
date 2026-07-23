import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';




export interface UserProfile {
  rowGuid: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  status: boolean;
  hasBusiness?: boolean;
  isBusinessApproved?: boolean;
  businessName?: string;
  businessId: number;
  avatarPath?: string;
  teamAvatarId?: number;
  phoneNumber?: string;
}


 
export interface TeamAvatar {
  id: number;          // Veritabanındaki Id
  teamName: string;    // Örn: Galatasaray
  imagePath: string;   // Örn: /assets/images/avatars/gs.png
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
  avatars = signal<TeamAvatar[]>([]);

   getTeamAvatars(): Observable<DataResult<TeamAvatar[]>> {
    return this.http.get<DataResult<TeamAvatar[]>>(`${this.apiUrl}/TeamAvatars`, {
      withCredentials: true 
    }).pipe(
      tap((response) => {
        // İşlem başarılıysa doğrudan servisteki sinyale basıyoruz
        if (response.success && response.data) {
          this.avatars.set(response.data);
        }
      })
    );
  }

  updateProfile(payload: Partial<UserProfile>): Observable<DataResult<UserProfile>> {
    return this.http.put<DataResult<UserProfile>>(`${this.apiUrl}/updateProfile`, payload, {
      withCredentials: true
    }).pipe(
      tap((response) => {
        // İşlem başarılıysa backend'den dönen yepyeni profili (yeni resim yolu dahil) 
        // doğrudan sinyale basıyoruz. Böylece Angular arayüzü F5 gerektirmeden saniyesinde güncelliyor!
        if (response.success && response.data) {
          this.currentUser.set(response.data);
        }
      })
    );
  }

 
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