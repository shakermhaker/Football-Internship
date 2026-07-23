import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyfieldsCard } from './myfields-card.component';

describe('MyfieldsCard', () => {
  let component: MyfieldsCard;
  let fixture: ComponentFixture<MyfieldsCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyfieldsCard],
    }).compileComponents();

    fixture = TestBed.createComponent(MyfieldsCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
