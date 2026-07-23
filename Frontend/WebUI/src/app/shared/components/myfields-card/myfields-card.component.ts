import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-myfield-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './myfields-card.component.html',
  styleUrl: './myfields-card.component.scss' // CSS DOSYASININ BURAYA BAĞLI OLDUĞUNDAN EMİN OL
})
export class MyfieldsCard { // Sınıf adın farklıysa (örn: MyfieldsCard) burayı ona göre bırak
  @Input() field: any;
}