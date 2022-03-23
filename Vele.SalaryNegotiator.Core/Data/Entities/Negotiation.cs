using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Vele.SalaryNegotiator.Core.Data.Entities;

public class Negotiation
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string EmployerName { get; set; }

    public string EmployeeName { get; set; }

    public DateTime CreatedDate { get; set; }

    public string EmployerSecret { get; set; }

    public string EmployeeSecret { get; set; }

    public virtual ICollection<Offer> Offers { get; set; }
}

public class NegotiationEntityTypeConfiguration : IEntityTypeConfiguration<Negotiation>
{
    public void Configure(EntityTypeBuilder<Negotiation> builder)
    {
        builder
            .HasKey(n => n.Id);
    }
}
