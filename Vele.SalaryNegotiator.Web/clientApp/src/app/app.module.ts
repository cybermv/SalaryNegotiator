import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './components/home/home.component';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import { NewNegotiatorFormComponent } from './components/new-negotiator-form/new-negotiator-form.component';
import { TextInputComponent } from './components/input-components/text-input/text-input.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NewNegotiatorFormComponent,
    TextInputComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
