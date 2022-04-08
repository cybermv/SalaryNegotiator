import { Component, OnInit } from '@angular/core';
import {FormControl, FormControlStatus, FormGroup, Validators} from "@angular/forms";
import {TextInputInfo} from "../input-components/text-input/text-input.component";
import {RadioInputInfo,} from "../input-components/radio-input/radio-input.component";
import {NegotiationCreateRequest, OfferSide, OfferType} from "../../Models/NegotiationCreateRequest";
import {CheckBoxInputInfo} from "../input-components/checkbox-input/checkbox-input.component";
import {NegotiationService} from "../../services/negotiation.service";

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
    amount: new FormControl('', [Validators.required]),
    maxAmount: new FormControl('', [Validators.required]),
    minAmount: new FormControl('', [Validators.required]),
    needsCounterOfferToShow: new FormControl(false, ),
  });
  negotiationNameInfo=new TextInputInfo('negotiationName','negotiationName','Negotiation Name',this.negotiationName,'VALID');
  nameInfo=new TextInputInfo('name','name','Name',this.name,'VALID');
  amountInfo=new TextInputInfo('amount','amount','Amount',this.amount,'VALID');
  minAmountInfo=new TextInputInfo('minAmount','minAmount','Min Amount',this.minAmount,'VALID');
  maxAmountInfo=new TextInputInfo('maxAmount','maxAmount','Max Amount',this.maxAmount,'VALID');
  typeInfo=new RadioInputInfo('type','type','Type',this.form,'VALID',OfferType);
  sideInfo=new RadioInputInfo('side','side','Side',this.form,'VALID',OfferSide);
  needsCounterOfferToShowInfo=new CheckBoxInputInfo('needsCounterOfferToShow','needsCounterOfferToShow','Needs Counter Offer To Show',this.form,'VALID');

  offerType=OfferType;


  get negotiationName() {
    return this.form.get('negotiationName') as FormControl;
  }
  get name() {
    return this.form.get('name') as FormControl;
  }
  get amount() {
    var controller=(this.form.get('amount') as FormControl);
    if(this.type.value==OfferType.Range){
       controller.setValidators([Validators.required]);
    }
    else{
       controller.setValidators([]);
    }
    return controller;
  }
  get maxAmount() {
    return this.form.get('maxAmount') as FormControl;
  }
  get minAmount() {
    return this.form.get('minAmount') as FormControl;
  }
  get side() {
    return this.form.get('side') as FormControl;
  }
  get type() {
    return this.form.get('type') as FormControl;
  }
  get needsCounterOfferToShow() {
    return this.form.get('needsCounterOfferToShow') as FormControl;
  }
  constructor(public negotiationService:NegotiationService) {
  }

  ngOnInit(): void {
  }

  onSubmit(){
    this.form.markAllAsTouched()
    console.log(this.form.value);
    if(this.form.valid){
      var negotiationCreateRequest=new NegotiationCreateRequest(
        this.negotiationName.value,
        this.name.value,
        +this.side.value,
        +this.type.value,
        +this.amount.value,
        +this.minAmount.value,
        +this.maxAmount.value,
        this.needsCounterOfferToShow.value
      );
      this.negotiationService.createNegotiation(negotiationCreateRequest).subscribe(
        (response)=>{
          console.log(response);
        },
        (error)=>{
          console.log(error);
        }
      );
    }
  }
}
