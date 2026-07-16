import { Component } from '@angular/core';

@Component({
  selector: 'app-how-it-works',
  standalone: true,
  imports: [],
  // HATA BURADAYDI: Sadece .html yazıyordu, .component.html olarak düzeltiyoruz (veya klasöründeki html dosyasının adı tam olarak neyse onu yazıyoruz)
  templateUrl: './how-it-works.component.html', 
  styleUrl: './how-it-works.component.scss' // Eğer stil dosyan da patlıyorsa onu da .component.scss yapmayı unutma
})
export class HowItWorksComponent {}
