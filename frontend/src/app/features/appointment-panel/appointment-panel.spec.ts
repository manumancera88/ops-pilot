import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentPanel } from './appointment-panel';

describe('AppointmentPanel', () => {
  let component: AppointmentPanel;
  let fixture: ComponentFixture<AppointmentPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentPanel],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentPanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
