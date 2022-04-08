import {Component, Input, OnInit} from '@angular/core';
import {AbstractControl, FormControl, FormControlStatus} from "@angular/forms";

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.scss']
})
export class TextInputComponent implements OnInit {
  @Input() public textInputInfo: TextInputInfo | undefined
  constructor() { }

  ngOnInit(): void {

  }


}

export class TextInputInfo{
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
