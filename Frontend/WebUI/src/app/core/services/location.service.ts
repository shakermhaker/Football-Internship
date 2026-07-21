import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators'; // map operatörünü ekledik

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7074/api/Locations';

  getCities(): Observable<any[]> {
    // Backend'den gelen IDataResult içindeki 'data' kısmını map ile ayıklıyoruz
    return this.http.get<any>(`${this.apiUrl}/cities`).pipe(
      map(response => response.data)
    );
  }

  getDistrictsByCityId(cityId: number): Observable<any[]> {
    return this.http.get<any>(`${this.apiUrl}/cities/${cityId}/districts`).pipe(
      map(response => response.data)
    );
  }
}