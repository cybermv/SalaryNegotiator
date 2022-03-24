using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Serilog;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Generators;
using System;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Data.Entities;

namespace Vele.SalaryNegotiator.TestConsole;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Information()
            .CreateLogger();

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(dispose: true))
            .AddSingleton<ICodeGenerator, WordCodeGenerator>()
            .AddSingleton<ISecretGenerator, GuidSecretGenerator>()
            .AddScoped<INegotiationService, NegotiationService>()
            .AddDbContext<SalaryNegotiatorDbContext>(db =>
            {
                db.UseSqlite("Data source=SalaryNegotiator_Console.db");
                db.EnableSensitiveDataLogging();
                db.EnableDetailedErrors();
            })
            .BuildServiceProvider();

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            SalaryNegotiatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SalaryNegotiatorDbContext>();
            //dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        INegotiationService ns = serviceProvider.GetRequiredService<INegotiationService>();

        NegotiationCreateOrClaimResponse createResponse = await ns.Create(new NegotiationCreateRequest
        {
            NegotiationName = "1545 - blagajnik/ca",
            Name = "Konzum",
            Side = Offer.OfferSide.Employer,
            Type = Offer.OfferType.Range,
            MaxAmount = 5500,
            MinAmount = 4200
        });

        NegotiationCreateOrClaimResponse claimResponse = await ns.Claim(new NegotiationClaimRequest
        {
            Id = createResponse.Id,
            Name = "Marica Blajdić",
            Side = Offer.OfferSide.Employee
        });

        NegotiationResponse viewRes1 = await ns.View(new NegotiationViewRequest
        {
            Id = createResponse.Id,
            Side = Offer.OfferSide.Employee,
            Secret = claimResponse.Secret
        });

        NegotiationMakeOfferResponse offerResponse1 = await ns.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = createResponse.Id,
            Side = Offer.OfferSide.Employee,
            Secret = claimResponse.Secret,
            Type = Offer.OfferType.Minimum,
            MinAmount = 5000,
            NeedsCounterOfferToShow = true
        });

        NegotiationResponse viewRes2 = await ns.View(new NegotiationViewRequest
        {
            Id = createResponse.Id,
            Side = Offer.OfferSide.Employer,
            Secret = createResponse.Secret
        });

        NegotiationMakeOfferResponse offerResponse2 = await ns.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = createResponse.Id,
            Side = Offer.OfferSide.Employer,
            Secret = createResponse.Secret,
            Type = Offer.OfferType.Fixed,
            Amount = 5200,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerResponse1.Id
        });

        NegotiationResponse viewRes3 = await ns.View(new NegotiationViewRequest
        {
            Id = createResponse.Id,
            Side = Offer.OfferSide.Employer,
            Secret = createResponse.Secret
        });

        NegotiationResponse viewRes4 = await ns.View(new NegotiationViewRequest
        {
            Id = createResponse.Id,
            Side = Offer.OfferSide.Employee,
            Secret = claimResponse.Secret
        });

        return 0;
    }
}