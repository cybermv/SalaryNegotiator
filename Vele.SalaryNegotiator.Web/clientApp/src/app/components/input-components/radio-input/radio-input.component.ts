import {Component, Input, OnInit} from '@angular/core';
import {FormControl, FormControlStatus, FormGroup} from "@angular/forms";
import {StandardEnum} from "../../../Models/NegotiationCreateRequest";

@Component({
  selector: 'app-radio-input',
  templateUrl: './radio-input.component.html',
  styleUrls: ['./radio-input.component.scss']
})
export class RadioInputComponent<T extends StandardEnum<unknown>> implements OnInit {
  @Input() public radioInputInfo: RadioInputInfo<T>|undefined;
  constructor() {


  }
  ngOnInit(): void {
    var a=this.radioInputInfo;
  }

}
export class RadioInputInfo<T extends StandardEnum<unknown>>{
  constructor(id: string, name: string, placeholder: string,
              formGroup: FormGroup, status: FormControlStatus,
                options: T){
    this.id = id;
    this.name = name;
    this.placeholder = placeholder;
    this.formGroup = formGroup;
    this.status = status;
    this.options = options;
  }
  id:string;
  name:string;
  placeholder:string;
  formGroup:FormGroup;
  status: FormControlStatus;
  options: T;
}
