import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

export interface BusinessImageDto {
  id: number;
  imagePath: string;
  isCover: boolean;
}

export interface BusinessDetailDto {
  businessId: number; // 🚀 Backend'den artık 'id' değil 'businessId' dönüyor
  name: string;
  city: string;
  district: string;
  fullAddress: string; // 🚀 YENİ
  images: BusinessImageDto[]; // 🚀 YENİ Liste Alanımız
}

@Component({
  selector: 'app-field-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './field-card.component.html',
  styleUrl: './field-card.component.scss'
})
export class FieldCardComponent implements OnInit {
  // 🚀 Input tipini yeni DTO'muza bağladık
  @Input() field!: BusinessDetailDto;

  ngOnInit() {
    // 1. Gelen resimleri sıralayalım: isCover = true olanlar EN BAŞA gelsin.
    if (this.field && this.field.images && this.field.images.length > 0) {
      this.field.images.sort((a, b) => (a.isCover === b.isCover) ? 0 : a.isCover ? -1 : 1);
    }
  }

  // 2. Backend sadece "/uploads/..." dönüyor. Angular'ın resmi bulabilmesi için
  // başına API adresini (https://localhost:7074) ekliyoruz.
  getFullImagePath(path: string): string {
    if (!path) return '';
    // Eğer http ile başlıyorsa zaten tam linktir, değilse API URL'ini ekle
    if (path.startsWith('http')) return path;
    return `https://localhost:7074${path}`; // Kendi API portun neyse orayı kontrol et!
  }
}