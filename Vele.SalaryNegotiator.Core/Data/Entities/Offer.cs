using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Vele.SalaryNegotiator.Core.Data.Entities;

public class Offer
{
    public int Id { get; set; }

    public OfferType Type { get; set; }

    public OfferSide Side { get; set; }

    public double? Amount { get; set; }

    public double? MaxAmount { get; set; }

    public double? MinAmount { get; set; }

    public DateTime OfferedDate { get; set; }

    public bool NeedsConterOfferToShow { get; set; }

    public int? CounterOfferId { get; set; }

    public string NegotiationId { get; set; }

    public virtual Offer CounterOffer { get; set; }

    public virtual Negotiation Negotiation { get; set; }

    public enum OfferType
    {
        Fixed = 1,
        Range = 2,
        Minimum = 3,
        Maximum = 4
    }

    public enum OfferSide
    {
        Employer = 1,
        Employee = 2
    }
}

public class OfferEntityTypeConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder
            .HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder
            .HasOne(o => o.Negotiation)
            .WithMany(n => n.Offers)
            .HasForeignKey(o => o.NegotiationId);

        builder
            .HasOne(o => o.CounterOffer)
            .WithOne()
            .HasForeignKey<Offer>(o => o.CounterOfferId);
    }
}