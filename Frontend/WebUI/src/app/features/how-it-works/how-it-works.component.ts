import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-how-it-works',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './how-it-works.component.html',
  styleUrls: ['./how-it-works.component.scss']
})
export class HowItWorksComponent {
  // Varsayılan olarak sporcular sekmesi açık gelsin
  activeTab: 'athletes' | 'businesses' = 'athletes';

  setTab(tab: 'athletes' | 'businesses') {
    this.activeTab = tab;
  }
}