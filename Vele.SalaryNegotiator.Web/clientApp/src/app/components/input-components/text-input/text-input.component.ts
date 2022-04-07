import {Component, Input, OnInit} from '@angular/core';
import {AbstractControl, FormControl, FormControlStatus} from "@angular/forms";

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.scss']
})
export class TextInputComponent implements OnInit {
  @Input() public control: FormControl | undefined
  constructor() { }

  ngOnInit(): void {

  }

}
