import { Component, OnInit } from '@angular/core';
import {FormControl, FormControlStatus, FormGroup, Validators} from "@angular/forms";

@Component({
  selector: 'app-new-negotiator-form',
  templateUrl: './new-negotiator-form.component.html',
  styleUrls: ['./new-negotiator-form.component.scss']
})
export class NewNegotiatorFormComponent implements OnInit {
  form=new FormGroup({
    negotiationName: new FormControl('', [Validators.required]),
    name: new FormControl('', [Validators.required]),
    side: new FormControl('1', [Validators.required]),
    type: new FormControl('1', [Validators.required]),
    amount: new FormControl('0', [Validators.required]),
    maxAmount: new FormControl('0', [Validators.required]),
    minAmount: new FormControl('0', [Validators.required]),
    needsCounterOfferToShow: new FormControl('', [Validators.required]),
  });

  get negotiationName() {
    return this.form.get('negotiationName') as FormControl;
  }
  constructor() { }

  ngOnInit(): void {
  }

}
