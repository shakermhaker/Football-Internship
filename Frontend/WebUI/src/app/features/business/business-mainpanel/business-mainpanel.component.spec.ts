import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BusinessMainpanelComponent } from './business-mainpanel.component';

describe('BusinessMainpanelComponent', () => {
  let component: BusinessMainpanelComponent;
  let fixture: ComponentFixture<BusinessMainpanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BusinessMainpanelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BusinessMainpanelComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
