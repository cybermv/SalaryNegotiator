import { Injectable } from '@angular/core';
import {environment} from "../../environments/environment";
import {HttpClient} from "@angular/common/http";
import {NegotiationCreateRequest} from "../Models/NegotiationCreateRequest";

@Injectable({
  providedIn: 'root'
})
export class NegotiationService {
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {

  }

  createNegotiation(negotiation: NegotiationCreateRequest) {
    return this.http.post(this.baseUrl + '/negotiation/create', negotiation);
  }
}
