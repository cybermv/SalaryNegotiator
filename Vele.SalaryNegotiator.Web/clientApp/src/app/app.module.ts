import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './components/home/home.component';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import { NewNegotiatorFormComponent } from './components/new-negotiator-form/new-negotiator-form.component';
import { TextInputComponent } from './components/input-components/text-input/text-input.component';
import { RadioInputComponent } from './components/input-components/radio-input/radio-input.component';
import { CheckboxInputComponent } from './components/input-components/checkbox-input/checkbox-input.component';
import {HttpClientModule} from "@angular/common/http";
import {NegotiationService} from "./services/negotiation.service";

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NewNegotiatorFormComponent,
    TextInputComponent,
    RadioInputComponent,
    CheckboxInputComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule
  ],
  providers: [
    NegotiationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
