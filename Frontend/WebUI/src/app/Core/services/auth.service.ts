import { Injectable, inject, PLATFORM_ID } from '@angular/core'; // PLATFORM_ID eklendi
import { isPlatformBrowser } from '@angular/common'; // isPlatformBrowser eklendi
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root' // Neden? Uygulama genelinde tek bir hafıza alanı (Singleton) kullansın diye.
})
export class AuthService {
  // Bağımlılık Enjeksiyonu (DI): Angular'ın HTTP motorunu içeri çağırıyoruz.
  private http = inject(HttpClient); 
  private platformId = inject(PLATFORM_ID); // Tarayıcı mı sunucu mu anlamak için ekledik
  private apiUrl = 'https://localhost:7074/api/Auth'; // Sizin .NET 10 API adresiniz

  // --- NAVBAR İÇİN EKLEDİĞİMİZ REAKTİF STATE ---
  private loggedIn = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.loggedIn.asObservable(); // Navbar bunu dinleyecek

  constructor() {
    // Sayfa yenilendiğinde eğer tarayıcıdaysak ve önceden token varsa giriş yapılı başlasın
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('user_token');
      if (token) {
        this.loggedIn.next(true);
      }
    }
  }

  register(payload: any): Observable<any> {
    // Neden Observable ve subscribe? Veri akışını boru hattıyla dinlemek için RxJS yapısı kullanıyoruz.
    return this.http.post(`${this.apiUrl}/register`, payload);
  }

  login(data: any): Observable<any> {
    // Login işlemi için backend'e POST isteği atıyoruz
    return this.http.post(`${this.apiUrl}/login`, data, { 
      withCredentials: true // Cookie'yi tarayıcıya kaydetmek için 
    }).pipe(
      tap(() => {
        // Giriş başarılı olunca navbar'a "açıl susam açıl" sinyalini gönderiyoruz
        this.loggedIn.next(true);
        if (isPlatformBrowser(this.platformId)) {
          localStorage.setItem('user_token', 'true');
        }
      })
    );
  }

  sendVerificationEmail(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resendVerificationEmail`, { email: email });
  }

  // --- NAVBAR İÇİN EKLEDİĞİMİZ ÇIKIŞ METODU ---
  logout() {
    this.loggedIn.next(false); // Navbar'a "Giriş Yap butonunu geri getir" sinyali gidiyor
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('user_token');
    }
  }
}