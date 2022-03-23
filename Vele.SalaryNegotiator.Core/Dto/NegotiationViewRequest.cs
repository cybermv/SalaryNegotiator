using FluentValidation;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Dto;

public class NegotiationViewRequest
{
    public string Id { get; set; }

    public string Secret { get; set; }

    public Offer.OfferSide Side { get; set; }

    public class Validator : AbstractValidator<NegotiationViewRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Secret).NotEmpty();
            RuleFor(r => r.Side).IsInEnum();
        }
    }
}
