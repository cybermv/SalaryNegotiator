import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewNegotiatorFormComponent } from './new-negotiator-form.component';

describe('NewNegotiatorFormComponent', () => {
  let component: NewNegotiatorFormComponent;
  let fixture: ComponentFixture<NewNegotiatorFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NewNegotiatorFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewNegotiatorFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
