using FluentValidation;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Dto;

public class NegotiationMakeOfferRequest
{
    public string NegotiationId { get; set; }

    public string Secret { get; set; }

    public Offer.OfferSide Side { get; set; }

    public Offer.OfferType Type { get; set; }

    public double? Amount { get; set; }

    public double? MaxAmount { get; set; }

    public double? MinAmount { get; set; }

    public bool NeedsCounterOfferToShow { get; set; }

    public int? CounterOfferId { get; set; }

    public class Validator : AbstractValidator<NegotiationMakeOfferRequest>
    {
        public Validator()
        {
            RuleFor(r => r.NegotiationId).NotEmpty();
            RuleFor(r => r.Secret).NotEmpty();
            RuleFor(r => r.Side).IsInEnum();
            RuleFor(r => r.Type).IsInEnum();

            RuleFor(r => r.Amount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.Type == Offer.OfferType.Fixed);

            RuleFor(r => r.MaxAmount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.Type == Offer.OfferType.Maximum || r.Type == Offer.OfferType.Range);

            RuleFor(r => r.MinAmount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.Type == Offer.OfferType.Minimum || r.Type == Offer.OfferType.Range);

            RuleFor(r => r)
                .Must(r => r.MinAmount < r.MaxAmount)
                    .When(r => r.Type == Offer.OfferType.Range)
                .WithName("AmountRange");

            RuleFor(r => r.NeedsCounterOfferToShow)
                .Must(v => v == true)
                .When(r => r.CounterOfferId.HasValue);
        }
    }
}
