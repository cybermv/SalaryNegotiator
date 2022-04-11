import {Component, Input, OnInit} from '@angular/core';
import {AbstractControl, FormControl, FormControlStatus} from "@angular/forms";
import {TextInputInfo} from "../text-input/text-input.component";

@Component({
  selector: 'app-number-input',
  templateUrl: './number-input.component.html',
  styleUrls: ['./number-input.component.scss']
})
export class NumberInputComponent implements OnInit {
  @Input() public numberInputInfo: NumberInputInfo | undefined
  constructor() { }

  ngOnInit(): void {

  }


}
export class NumberInputInfo{
  constructor(id: string, name: string, placeholder: string, control: FormControl, status: FormControlStatus){
    this.id = id
    this.name = name
    this.placeholder = placeholder
    this.control = control
    this.status = status
  }
  id:string;
  name:string;
  placeholder:string;
  control:FormControl;
  status: FormControlStatus;
}
