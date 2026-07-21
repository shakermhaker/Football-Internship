import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FieldCardComponent } from './field-card.component';

describe('FieldCardComponent', () => {
  let component: FieldCardComponent;
  let fixture: ComponentFixture<FieldCardComponent      >;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FieldCardComponent ],
    }).compileComponents();

    fixture = TestBed.createComponent(FieldCardComponent  );
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
