export class NegotiationCreateRequest {
  negotiationName: string;
  name: string;
  side:OfferSide;
  type:OfferType;
  amount:number;
  maxAmount:number;
  minAmount:number;
  needsCounterOfferToShow:boolean;
}

export enum OfferSide{
  Employer = 1,
  Employee = 2
}
export enum OfferType{
  Fixed = 1,
  Range = 2,
  Minimum = 3,
  Maximum = 4
}
