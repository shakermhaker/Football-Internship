import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root' // Neden? Uygulama genelinde tek bir hafıza alanı (Singleton) kullansın diye.
})
export class AuthService {
  // Bağımlılık Enjeksiyonu (DI): Angular'ın HTTP motorunu içeri çağırıyoruz.
  private http = inject(HttpClient); 
  private apiUrl = 'https://localhost:7074/api/Auth'; // Sizin .NET 10 API adresiniz

  register(payload: any): Observable<any> {
    // Neden Observable ve subscribe? Veri akışını boru hattıyla dinlemek için RxJS yapısı kullanıyoruz.
    return this.http.post(`${this.apiUrl}/register`, payload);
  }
}