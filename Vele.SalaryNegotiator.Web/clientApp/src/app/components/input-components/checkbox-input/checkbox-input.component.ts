import {Component, Input, OnInit} from '@angular/core';
import {FormControl, FormControlStatus, FormGroup} from "@angular/forms";
import {TextInputInfo} from "../text-input/text-input.component";

@Component({
  selector: 'app-checkbox-input',
  templateUrl: './checkbox-input.component.html',
  styleUrls: ['./checkbox-input.component.scss']
})
export class CheckboxInputComponent implements OnInit {
  @Input() public checkboxInputInfo: CheckBoxInputInfo | undefined

  constructor() { }

  ngOnInit(): void {
  }

}

export class CheckBoxInputInfo{
  constructor(id: string, name: string, placeholder: string, control: FormGroup, status: FormControlStatus){
    this.id = id
    this.name = name
    this.placeholder = placeholder
    this.control = control
    this.status = status
  }
  id:string;
  name:string;
  placeholder:string;
  control:FormGroup;
  status: FormControlStatus;
}
