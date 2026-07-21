import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddFieldComponent } from './add-field.component';

describe('AddFieldComponent', () => {
  let component: AddFieldComponent;
  let fixture: ComponentFixture<AddFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AddFieldComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
