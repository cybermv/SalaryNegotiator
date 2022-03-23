using Microsoft.EntityFrameworkCore;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.Core.Data;

public class SalaryNegotiatorDbContext : DbContext
{
    public DbSet<Negotiation> Negotiations { get; set; }

    public DbSet<Offer> Offers { get; set; }

    public SalaryNegotiatorDbContext(DbContextOptions<SalaryNegotiatorDbContext> dbContextOptions) : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalaryNegotiatorDbContext).Assembly);
    }
}
