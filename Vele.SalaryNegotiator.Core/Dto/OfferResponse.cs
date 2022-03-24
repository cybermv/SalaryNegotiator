using System;
using System.Text;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Dto;

public class OfferResponse
{
    public int Id { get; set; }

    public Offer.OfferSide Side { get; set; }

    public Offer.OfferType? Type { get; set; }

    public double? Amount { get; set; }

    public double? MaxAmount { get; set; }

    public double? MinAmount { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool NeedsCounterOfferToShow { get; set; }

    public int? CounterOfferId { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"#{Id}-{CreatedDate:yyyyMMdd-HHmmss.fff} - {Side} - ");

        switch (Type)
        {
            case Offer.OfferType.Fixed:
                sb.Append($"fixed {Amount}");
                break;
            case Offer.OfferType.Range:
                sb.Append($"range {MinAmount}:{MaxAmount}");
                break;
            case Offer.OfferType.Minimum:
                sb.Append($"minimum {MinAmount}");
                break;
            case Offer.OfferType.Maximum:
                sb.Append($"maximum {MaxAmount}");
                break;
            default:
                sb.Append($"hidden");
                break;
        }

        if (NeedsCounterOfferToShow)
        {
            if (CounterOfferId.HasValue)
            {
                sb.Append($" - closed, counters {CounterOfferId.Value}");
            }
            else
            {
                sb.Append($" - closed, needs counter offer");
            }
        }
        else
        {
            sb.Append($" - open");
        }

        return sb.ToString();
    }
}