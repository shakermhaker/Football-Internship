import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BusinessReservationsComponent } from './business-reservations.component';

describe('BusinessReservationsComponent', () => {
  let component: BusinessReservationsComponent;
  let fixture: ComponentFixture<BusinessReservationsComponent >;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BusinessReservationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BusinessReservationsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
