import { Injectable, inject, PLATFORM_ID } from '@angular/core'; // PLATFORM_ID eklendi
import { isPlatformBrowser } from '@angular/common'; // isPlatformBrowser eklendi
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap,catchError , of} from 'rxjs';
import { UserService } from './user.service'; // 🎯 Kullanıcı sinyalini tetiklemek için ekledik
@Injectable({
  providedIn: 'root' // Neden? Uygulama genelinde tek bir hafıza alanı (Singleton) kullansın diye.
})
export class AuthService {
  private userService = inject(UserService);
  // Bağımlılık Enjeksiyonu (DI): Angular'ın HTTP motorunu içeri çağırıyoruz.
  private http = inject(HttpClient); 
  private platformId = inject(PLATFORM_ID); // Tarayıcı mı sunucu mu anlamak için ekledik
  private apiUrl = 'https://localhost:7074/api/Auth'; // Sizin .NET 10 API adresiniz

  // --- NAVBAR İÇİN EKLEDİĞİMİZ REAKTİF STATE ---
  private loggedIn = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.loggedIn.asObservable(); // Navbar bunu dinleyecek

  constructor() {
    
    this.checkInitialAuthStatus();
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
        this.loggedIn.next(true);
        this.userService.fetchMyProfile().subscribe();
      })
    );
  }

  sendVerificationEmail(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resendVerificationEmail`, { email: email });
  }
  private checkInitialAuthStatus() {
    if (isPlatformBrowser(this.platformId)) {
      this.userService.fetchMyProfile().pipe(
        tap(() => {
          // Çerez geçerli, backend profili döndü -> Oturum AÇIK!
          this.loggedIn.next(true);
        }),
        catchError(() => {
          // Çerez yok, süresi bitmiş veya geçersiz -> Oturum KAPALI!
          this.loggedIn.next(false);
          this.userService.clearUser();
          return of(null);
        })
      ).subscribe();
    }
  }

  // --- NAVBAR İÇİN EKLEDİĞİMİZ ÇIKIŞ METODU ---
  logout(): void {
    // Backend'deki çerezi fiziksel olarak imha etmek için logout endpoint'ine istek atıyoruz
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      next: () => this.cleanFrontendSession(),
      error: () => this.cleanFrontendSession() // Backend kapalı olsa bile arayüzü temizle
    });
  }

  private cleanFrontendSession() {
    this.loggedIn.next(false);
    this.userService.clearUser(); // Sinyali (Mahmut Tuncer'i) RAM'den sil
    
    // Eğer önceden kalma eski çöp datalar varsa garanti olsun diye temizliyoruz
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('user_token'); 
      localStorage.removeItem('isLoggedIn');
    }
  }
}