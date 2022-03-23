using FluentValidation;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Dto;

public class NegotiationCreateRequest
{
    public string NegotiationName { get; set; }

    public string Name { get; set; }

    public Offer.OfferSide OfferSide { get; set; }

    public Offer.OfferType OfferType { get; set; }

    public double? Amount { get; set; }

    public double? MaxAmount { get; set; }

    public double? MinAmount { get; set; }

    public bool NeedsConterOfferToShow { get; set; }

    public class Validator : AbstractValidator<NegotiationCreateRequest>
    {
        public Validator()
        {
            RuleFor(r => r.NegotiationName).NotEmpty();
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.OfferSide).IsInEnum();
            RuleFor(r => r.OfferType).IsInEnum();

            RuleFor(r => r.Amount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.OfferType == Offer.OfferType.Fixed);

            RuleFor(r => r.MaxAmount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.OfferType == Offer.OfferType.Maximum || r.OfferType == Offer.OfferType.Range);

            RuleFor(r => r.MinAmount)
                .Must(a => a.HasValue && a.Value > 0)
                    .When(r => r.OfferType == Offer.OfferType.Minimum || r.OfferType == Offer.OfferType.Range);

            RuleFor(r => r)
                .Must(r => r.MinAmount < r.MaxAmount)
                    .When(r => r.OfferType == Offer.OfferType.Range)
                .WithName("AmountRange");
        }
    }
}
