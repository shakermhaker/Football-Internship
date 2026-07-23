import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-myfield-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './myfields-card.component.html',
  styleUrl: './myfields-card.component.scss'
})
export class MyfieldsCard { 
  @Input() field: any;
  
  // 🎯 Ana bileşene fırlatacağımız sinyaller (Sahanın ID'sini yollayacağız)
  @Output() onEdit = new EventEmitter<number>();
  @Output() onDelete = new EventEmitter<number>();

  editClick() {
    this.onEdit.emit(this.field.id);
  }

  deleteClick() {
    this.onDelete.emit(this.field.id);
  }
}