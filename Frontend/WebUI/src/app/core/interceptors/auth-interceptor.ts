import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core'; // PLATFORM_ID eklendi
import { isPlatformBrowser } from '@angular/common'; // Kontrol için eklendi
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID); // Hangi platformdayız?
  
  let token: string | null = null;

  // SADECE TARAYICIDAYSAK localStorage'a eriş
  if (isPlatformBrowser(platformId)) {
    token = localStorage.getItem('token');
  }

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((err) => {
      // Yine burayı da sadece tarayıcıda çalıştır
      if (isPlatformBrowser(platformId) && (err.status === 401 || err.status === 400)) {
        localStorage.removeItem('token');
        router.navigate(['/login']);
      }
      return throwError(() => err);
    })
  );
};