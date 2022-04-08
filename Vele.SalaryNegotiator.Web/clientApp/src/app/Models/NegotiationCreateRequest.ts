export class NegotiationCreateRequest {
  constructor(
    public negotiationName: string,
    public name: string,
    public side: OfferSide,
    public type: OfferType,
    public amount: number,
    public maxAmount: number,
    public minAmount: number,
    public needsCounterOfferToShow: boolean
  ) { }
}

export enum OfferSide{
  Employer = 1,
  Employee = 2
}
export enum OfferType{
  Fixed = 1,
  Range = 2,
}
export type StandardEnum<T> = {
  [nu: number]: string;
}
