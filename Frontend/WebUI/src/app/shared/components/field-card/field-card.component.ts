import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-field-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './field-card.component.html',
  styleUrl: './field-card.component.scss'
})
export class FieldCardComponent {
  // Veritabanı tablona (Örn: Businesses veya Fields) karşılık gelen model
  @Input() field!: {
    id: number;
    name: string;
    city: string;
    district: string;
    // Resim alanı şimdilik opsiyonel
    imageUrl?: string; 
  };
}