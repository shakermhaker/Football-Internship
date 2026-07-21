import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyFields } from './my-fields.component';

describe('MyFields', () => {
  let component: MyFields;
  let fixture: ComponentFixture<MyFields>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyFields],
    }).compileComponents();

    fixture = TestBed.createComponent(MyFields);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
