using FluentValidation;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Dto;

public class NegotiationClaimRequest
{
    public string Id { get; set; }

    public Offer.OfferSide Side { get; set; }

    public string Name { get; set; }

    public class Validator : AbstractValidator<NegotiationClaimRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Side).IsInEnum();
            RuleFor(r => r.Name).NotEmpty();
        }
    }
}
