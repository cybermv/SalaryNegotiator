import { Component, OnInit } from '@angular/core';
import {NegotiationCreateRequest} from "../../Models/NegotiationCreateRequest";
import {FormGroup} from "@angular/forms";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  form=new FormGroup({});
   negotiationCreateRequest=new NegotiationCreateRequest();
  constructor() { }

  ngOnInit(): void {
  }

}
