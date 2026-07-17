import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FootballfieldsComponent } from './footballfields.component';

describe('FootballfieldsComponent', () => {
  let component: FootballfieldsComponent;
  let fixture: ComponentFixture<FootballfieldsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FootballfieldsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FootballfieldsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
