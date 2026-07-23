import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MyfieldsCard } from '../../../shared/components/myfields-card/myfields-card.component';
@Component({
  selector: 'app-my-fields',
  standalone: true,
  imports: [CommonModule, RouterLink, MyfieldsCard], // MyfieldsCard eklendi
  templateUrl: './my-fields.component.html'
})
export class MyFieldsComponent {
  // Backend'den gelecek veriyi simüle ediyoruz
  myFields = [
    { id: 1, name: 'Merkez Arena Halı Saha', type: 'Kapalı Suni Çim', capacity: 14, status: 'Aktif' },
    { id: 2, name: 'Vadi Açık Saha', type: 'Açık Suni Çim', capacity: 12, status: 'Aktif' }
  ];
}