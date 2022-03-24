using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Data.Entities;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Exceptions;
using Vele.SalaryNegotiator.Core.Generators;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

namespace Vele.SalaryNegotiator.Test.Services;

[TestFixture]
public class NegotiationFlowTest
{
    private ILogger _logger;
    private IServiceProvider _serviceProvider;
    private INegotiationService _negotiationService;

    [SetUp]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .MinimumLevel.Information()
            .CreateLogger();

        _serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(_logger, dispose: true))
            .AddSingleton<ICodeGenerator, WordCodeGenerator>()
            .AddSingleton<ISecretGenerator, GuidSecretGenerator>()
            .AddScoped<INegotiationService, NegotiationService>()
            .AddDbContext<SalaryNegotiatorDbContext>(db =>
            {
                db.UseSqlite("Data source=SalaryNegotiator_Test.db");
                db.EnableSensitiveDataLogging();
                db.EnableDetailedErrors();
            })
            .BuildServiceProvider();

        _negotiationService = _serviceProvider.GetRequiredService<INegotiationService>();

        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            SalaryNegotiatorDbContext dbContext = scope.ServiceProvider.GetRequiredService<SalaryNegotiatorDbContext>();
            //dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (_logger != null && _logger is IDisposable disposableLogger)
            disposableLogger.Dispose();
        if (_serviceProvider != null && _serviceProvider is IDisposable disposableServProv)
            disposableServProv.Dispose();
    }

    [Test]
    public async Task Flow_HappyPath()
    {
        // John creates a negotiation with an initial offer - range from 35k to 50k
        NegotiationCreateOrClaimResponse createRes = await _negotiationService.Create(new NegotiationCreateRequest
        {
            NegotiationName = $"test_happy_{DateTime.Now:MMdd-hhmmss)}",
            Name = "John Smith",
            OfferSide = Offer.OfferSide.Employee,
            OfferType = Offer.OfferType.Range,
            MaxAmount = 50000,
            MinAmount = 35000,
            NeedsConterOfferToShow = true
        });

        string negotiationId = createRes.Id;
        string employeeSecret = createRes.Secret;

        Assert.That(negotiationId, Is.Not.Null.And.Not.Empty);
        Assert.That(employeeSecret, Is.Not.Null.And.Not.Empty);

        // John views his negotiation and sends a link to Microsoft
        NegotiationResponse negotiationResEmployee1 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.That(negotiationResEmployee1.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployee1.Offers.Count, Is.EqualTo(1));
        Assert.That(negotiationResEmployee1.Offers[0].Type, Is.EqualTo(Offer.OfferType.Range));

        int offer1Id = negotiationResEmployee1.Offers.Single().Id;

        // Microsoft claims the negotiation
        NegotiationCreateOrClaimResponse claimRes = await _negotiationService.Claim(new NegotiationClaimRequest
        {
            Id = negotiationId,
            Side = Offer.OfferSide.Employer,
            Name = "Microsoft Corp."
        });

        string employerSecret = claimRes.Secret;

        Assert.That(employerSecret, Is.Not.Null.And.Not.Empty);
        Assert.That(claimRes.Id, Is.EqualTo(negotiationId));

        // Microsoft views the negotiation but doesn't see the full offer details
        NegotiationResponse negotiationResEmployer1 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer1.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer1.EmployeeName, Contains.Substring("John"));
        Assert.That(negotiationResEmployer1.EmployerName, Contains.Substring("Microsoft"));
        Assert.That(negotiationResEmployer1.Offers.Count, Is.EqualTo(1));
        Assert.That(negotiationResEmployer1.Offers[0].Type, Is.Null);
        Assert.That(negotiationResEmployer1.Offers[0].Amount, Is.Null);
        Assert.That(negotiationResEmployer1.Offers[0].MinAmount, Is.Null);
        Assert.That(negotiationResEmployer1.Offers[0].MaxAmount, Is.Null);
        Assert.That(negotiationResEmployer1.Offers[0].NeedsConterOfferToShow, Is.True);

        // Microsoft counters the initial offer with a fixed amount of 28k
        NegotiationMakeOfferResponse offerResEmployer1 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer,
            Type = Offer.OfferType.Fixed,
            Amount = 28000,
            CounterOfferId = offer1Id,
            NeedsCounterOfferToShow = true
        });

        int offer2Id = offerResEmployer1.Id;

        // Microsoft views the negotiation and sees both offers in full
        NegotiationResponse negotiationResEmployer2 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer2.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer2.Offers.Count, Is.EqualTo(2));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer1Id).Type, Is.EqualTo(Offer.OfferType.Range));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer1Id).MinAmount, Is.EqualTo(35000));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer1Id).MaxAmount, Is.EqualTo(50000));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer2Id).Type, Is.EqualTo(Offer.OfferType.Fixed));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer2Id).Amount, Is.EqualTo(28000));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer1Id).CounterOfferId, Is.EqualTo(offer2Id));
        Assert.That(negotiationResEmployer2.Offers.Single(o => o.Id == offer2Id).CounterOfferId, Is.EqualTo(offer1Id));

        // John views the negotiation and sees both offers in full
        NegotiationResponse negotiationResEmployee2 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.That(negotiationResEmployee2.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployee2.Offers.Count, Is.EqualTo(2));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer1Id).Type, Is.EqualTo(Offer.OfferType.Range));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer1Id).MinAmount, Is.EqualTo(35000));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer1Id).MaxAmount, Is.EqualTo(50000));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer2Id).Type, Is.EqualTo(Offer.OfferType.Fixed));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer2Id).Amount, Is.EqualTo(28000));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer1Id).CounterOfferId, Is.EqualTo(offer2Id));
        Assert.That(negotiationResEmployee2.Offers.Single(o => o.Id == offer2Id).CounterOfferId, Is.EqualTo(offer1Id));

        // John makes another offer of 40k minimum, without requiring a counteroffer
        NegotiationMakeOfferResponse offerResEmployee2 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee,
            Type = Offer.OfferType.Minimum,
            MinAmount = 40000,
            NeedsCounterOfferToShow = false
        });

        int offer3Id = offerResEmployee2.Id;

        // Microsoft views the negotiation and sees the new offer in full
        NegotiationResponse negotiationResEmployer3 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer3.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer3.Offers.Count, Is.EqualTo(3));
        Assert.That(negotiationResEmployer3.Offers.Single(o => o.Id == offer3Id).Type, Is.EqualTo(Offer.OfferType.Minimum));
        Assert.That(negotiationResEmployer3.Offers.Single(o => o.Id == offer3Id).MinAmount, Is.EqualTo(40000));
        Assert.That(negotiationResEmployer3.Offers.Single(o => o.Id == offer3Id).MaxAmount, Is.Null);
        Assert.That(negotiationResEmployer3.Offers.Single(o => o.Id == offer3Id).NeedsConterOfferToShow, Is.False);
        Assert.That(negotiationResEmployer3.Offers.Single(o => o.Id == offer3Id).CounterOfferId, Is.Null);

        // Microsoft makes another offer of 37k maximum and requires a counter offer
        NegotiationMakeOfferResponse offerResEmployer2 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer,
            Type = Offer.OfferType.Maximum,
            MaxAmount = 37000,
            NeedsCounterOfferToShow = true
        });

        int offer4Id = offerResEmployer2.Id;

        // Microsoft views the negotiation and sees four offers in full
        NegotiationResponse negotiationResEmployer4 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer4.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer4.Offers.Count, Is.EqualTo(4));
        Assert.That(negotiationResEmployer4.Offers.Single(o => o.Id == offer4Id).Type, Is.EqualTo(Offer.OfferType.Maximum));
        Assert.That(negotiationResEmployer4.Offers.Single(o => o.Id == offer4Id).MaxAmount, Is.EqualTo(37000));
        Assert.That(negotiationResEmployer4.Offers.Single(o => o.Id == offer4Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployer4.Offers.Single(o => o.Id == offer4Id).CounterOfferId, Is.Null);

        // John views the negotiation and sees three full offers and one from Microsoft closed
        NegotiationResponse negotiationResEmployee3 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.That(negotiationResEmployee3.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployee3.Offers.Count, Is.EqualTo(4));
        Assert.That(negotiationResEmployee3.Offers.Single(o => o.Id == offer4Id).Type, Is.Null);
        Assert.That(negotiationResEmployee3.Offers.Single(o => o.Id == offer4Id).Amount, Is.Null);
        Assert.That(negotiationResEmployee3.Offers.Single(o => o.Id == offer4Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployee3.Offers.Single(o => o.Id == offer4Id).CounterOfferId, Is.Null);

        // John makes a counterofer to the last Microsoft offer, 39k minimum
        NegotiationMakeOfferResponse offerResEmployee3 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee,
            Type = Offer.OfferType.Minimum,
            MinAmount = 39000,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offer4Id
        });

        int offer5Id = offerResEmployee3.Id;

        // John views the negotiation and sees five full offers
        NegotiationResponse negotiationResEmployee4 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.That(negotiationResEmployee4.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployee4.Offers.Count, Is.EqualTo(5));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer4Id).Type, Is.EqualTo(Offer.OfferType.Maximum));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer4Id).MaxAmount, Is.EqualTo(37000));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer4Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer4Id).CounterOfferId, Is.EqualTo(offer5Id));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer5Id).Type, Is.EqualTo(Offer.OfferType.Minimum));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer5Id).MinAmount, Is.EqualTo(39000));
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer5Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployee4.Offers.Single(o => o.Id == offer5Id).CounterOfferId, Is.EqualTo(offer4Id));

        // Microsoft views the negotiation and sees five full offers
        NegotiationResponse negotiationResEmployer5 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer5.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer5.Offers.Count, Is.EqualTo(5));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer4Id).Type, Is.EqualTo(Offer.OfferType.Maximum));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer4Id).MaxAmount, Is.EqualTo(37000));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer4Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer4Id).CounterOfferId, Is.EqualTo(offer5Id));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer5Id).Type, Is.EqualTo(Offer.OfferType.Minimum));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer5Id).MinAmount, Is.EqualTo(39000));
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer5Id).NeedsConterOfferToShow, Is.True);
        Assert.That(negotiationResEmployer5.Offers.Single(o => o.Id == offer5Id).CounterOfferId, Is.EqualTo(offer4Id));

        // Microsoft makes another offer of 38k fixed
        NegotiationMakeOfferResponse offerResEmployer3 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer,
            Type = Offer.OfferType.Fixed,
            Amount = 38000
        });

        int offer6Id = offerResEmployer3.Id;

        // John views the negotiation and sees six full offers
        NegotiationResponse negotiationResEmployee5 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.That(negotiationResEmployee5.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployee5.Offers.Count, Is.EqualTo(6));
        Assert.That(negotiationResEmployee5.Offers.Single(o => o.Id == offer6Id).Type, Is.EqualTo(Offer.OfferType.Fixed));
        Assert.That(negotiationResEmployee5.Offers.Single(o => o.Id == offer6Id).Amount, Is.EqualTo(38000));
        Assert.That(negotiationResEmployee5.Offers.Single(o => o.Id == offer6Id).NeedsConterOfferToShow, Is.False);
        Assert.That(negotiationResEmployee5.Offers.Single(o => o.Id == offer6Id).CounterOfferId, Is.Null);

        // Microsoft views the negotiation and sees six offers
        NegotiationResponse negotiationResEmployer6 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        Assert.That(negotiationResEmployer6.Id, Is.EqualTo(negotiationId));
        Assert.That(negotiationResEmployer6.Offers.Count, Is.EqualTo(6));
        Assert.That(negotiationResEmployer6.Offers.Single(o => o.Id == offer6Id).Type, Is.EqualTo(Offer.OfferType.Fixed));
        Assert.That(negotiationResEmployer6.Offers.Single(o => o.Id == offer6Id).Amount, Is.EqualTo(38000));
        Assert.That(negotiationResEmployer6.Offers.Single(o => o.Id == offer6Id).NeedsConterOfferToShow, Is.False);
        Assert.That(negotiationResEmployer6.Offers.Single(o => o.Id == offer6Id).CounterOfferId, Is.Null);

        Assert.Pass();
    }

    [Test]
    public void Flow_Failed_BadCreateParams()
    {
        Assert.CatchAsync<ValidationException>(async () => await _negotiationService.Create(new NegotiationCreateRequest
        {
            NegotiationName = "",
            Name = null,
            OfferSide = Offer.OfferSide.Employee,
            OfferType = Offer.OfferType.Range,
            Amount = -100,
            MaxAmount = null,
            MinAmount = 32,
            NeedsConterOfferToShow = false
        }));
    }

    [Test]
    public async Task Flow_Failed_Unauthorized()
    {
        // Mike creates a negotiation with an initial offer - minimum 25k
        NegotiationCreateOrClaimResponse createRes = await _negotiationService.Create(new NegotiationCreateRequest
        {
            NegotiationName = $"test_fail_auth_{DateTime.Now:MMdd-hhmmss)}",
            Name = "Mike Langelo",
            OfferSide = Offer.OfferSide.Employee,
            OfferType = Offer.OfferType.Minimum,
            MinAmount = 25000,
            NeedsConterOfferToShow = true
        });

        string negotiationId = createRes.Id;
        string employeeSecret = createRes.Secret;

        Assert.That(negotiationId, Is.Not.Null.And.Not.Empty);
        Assert.That(employeeSecret, Is.Not.Null.And.Not.Empty);

        // Can't view with bad secret
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret + "-BAD",
            Side = Offer.OfferSide.Employee
        }));

        // Can't view if side is mismatched
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employer
        }));

        // Can't view if everything bad
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret + "-BAD",
            Side = Offer.OfferSide.Employer
        }));

        // Can't claim as side which has secret already
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.Claim(new NegotiationClaimRequest
        {
            Id = negotiationId,
            Name = "Exxon Mobil Bad",
            Side = Offer.OfferSide.Employee
        }));

        // Exxon Mobil claims it's secret
        NegotiationCreateOrClaimResponse claimRes = await _negotiationService.Claim(new NegotiationClaimRequest
        {
            Id = negotiationId,
            Name = "Exxon Mobil",
            Side = Offer.OfferSide.Employer
        });

        string employerSecret = claimRes.Secret;

        Assert.That(employerSecret, Is.Not.Null.And.Not.Empty);

        // Can't claim two times
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.Claim(new NegotiationClaimRequest
        {
            Id = negotiationId,
            Name = "Exxon Mobil Bad Again",
            Side = Offer.OfferSide.Employer
        }));

        // Can't view with secret and side mismatch
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employee
        }));

        // Can't make offer with wrong secret
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret + "-BAD",
            Type = Offer.OfferType.Fixed,
            Amount = 20000
        }));

        // Can't make offer with wrong side
        Assert.CatchAsync<ForbiddenException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employerSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 20000
        }));

        // Exxon views the negotiation finally
        NegotiationResponse viewRes1 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employerSecret,
            Side = Offer.OfferSide.Employer
        });

        int offerId1 = viewRes1.Offers.Single().Id;

        // Exxon makes a counter offer
        NegotiationMakeOfferResponse offerRes1 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 20000,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId1
        });

        Assert.That(offerRes1.Id, Is.GreaterThan(0));

        NegotiationResponse negotiationView = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret
        });

        Assert.That(negotiationView.Offers, Is.Not.Empty.And.Count.EqualTo(2));
    }

    [Test]
    public async Task Flow_Failed_CounterOffers()
    {
        // We need this to clear the change tracker. 
        SalaryNegotiatorDbContext dbContext = _serviceProvider.GetRequiredService<SalaryNegotiatorDbContext>();

        NegotiationCreateOrClaimResponse createRes = await _negotiationService.Create(new NegotiationCreateRequest
        {
            NegotiationName = $"test_fail_counter_{DateTime.Now:MMdd-hhmmss)}",
            Name = "BigCorp",
            OfferSide = Offer.OfferSide.Employer,
            OfferType = Offer.OfferType.Range,
            MaxAmount = 700,
            MinAmount = 500,
            NeedsConterOfferToShow = true
        });

        string negotiationId = createRes.Id;
        string employerSecret = createRes.Secret;

        NegotiationCreateOrClaimResponse claimRes = await _negotiationService.Claim(new NegotiationClaimRequest
        {
            Id = negotiationId,
            Side = Offer.OfferSide.Employee,
            Name = "Joe Average"
        });

        string employeeSecret = claimRes.Secret;

        NegotiationResponse negotiationResYee1 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        int offerId1 = negotiationResYee1.Offers.Single().Id;

        // Can't counter a nonexistent offer
        NotFoundException ex1 = Assert.CatchAsync<NotFoundException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 111,
            NeedsCounterOfferToShow = true,
            CounterOfferId = -50
        }));
        Assert.That(ex1.Message, Contains.Substring("Countered offer not found"));

        // Can't counter an offer without marking your offer as closed
        ValidationException ex2 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 222,
            NeedsCounterOfferToShow = false,
            CounterOfferId = offerId1
        }));
        Assert.That(ex2.ValidationResult, Is.Not.Null);
        Assert.That(ex2.ValidationResult.Errors, Is.Not.Empty);
        Assert.That(ex2.ValidationResult.ToString(), Contains.Substring("'Needs Counter Offer To Show'"));

        dbContext.ChangeTracker.Clear();

        // Can't counter your own offer
        ValidationException ex3 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 333,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId1
        }));
        Assert.That(ex3.Message, Contains.Substring("Cannot counter your own offer"));

        dbContext.ChangeTracker.Clear();

        // Can't create a closed offer when another closed offer exists
        ValidationException ex4 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 444,
            NeedsCounterOfferToShow = true,
            CounterOfferId = null
        }));
        Assert.That(ex4.Message, Contains.Substring("Cannot create closed offer when another closed one exists"));

        dbContext.ChangeTracker.Clear();

        NegotiationMakeOfferResponse makeOfferResYee1 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Minimum,
            MinAmount = 650,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId1
        });

        int offerId2 = makeOfferResYee1.Id;

        // Can't counter an already countered offer (yours)
        ValidationException ex5 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 555,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId2
        }));
        Assert.That(ex5.Message, Contains.Substring("Cannot counter an already countered offer"));

        dbContext.ChangeTracker.Clear();

        // Can't counter an already countered offer (theirs)
        ValidationException ex6 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 666,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId2
        }));
        Assert.That(ex6.Message, Contains.Substring("Cannot counter an already countered offer"));

        dbContext.ChangeTracker.Clear();

        NegotiationMakeOfferResponse makeOfferResYer2 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Maximum,
            MaxAmount = 900,
            NeedsCounterOfferToShow = true
        });

        int offerId3 = makeOfferResYer2.Id;

        // Can't make two unrelated closed offers
        ValidationException ex7 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Maximum,
            MaxAmount = 777,
            NeedsCounterOfferToShow = true
        }));
        Assert.That(ex7.Message, Contains.Substring("Cannot create closed offer when another closed one exists"));

        NegotiationMakeOfferResponse makeOfferResYee2 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Minimum,
            MinAmount = 850,
            NeedsCounterOfferToShow = false
        });

        int offerId4 = makeOfferResYee2.Id;

        NegotiationMakeOfferResponse makeOfferResYer3 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employer,
            Secret = employerSecret,
            Type = Offer.OfferType.Fixed,
            Amount = 851,
            NeedsCounterOfferToShow = false
        });

        int offerId5 = makeOfferResYer3.Id;

        // Can't make offer not needing counter offer but countering an offer
        ValidationException ex8 = Assert.CatchAsync<ValidationException>(async () => await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Minimum,
            MinAmount = 888,
            NeedsCounterOfferToShow = false,
            CounterOfferId = offerId3
        }));
        Assert.That(ex8.ValidationResult, Is.Not.Null);
        Assert.That(ex8.ValidationResult.Errors, Is.Not.Empty);
        Assert.That(ex8.ValidationResult.ToString(), Contains.Substring("'Needs Counter Offer To Show'"));

        NegotiationResponse negotiationResYee3 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        NegotiationMakeOfferResponse makeOfferResYee4 = await _negotiationService.MakeOffer(new NegotiationMakeOfferRequest
        {
            NegotiationId = negotiationId,
            Side = Offer.OfferSide.Employee,
            Secret = employeeSecret,
            Type = Offer.OfferType.Minimum,
            MinAmount = 850,
            NeedsCounterOfferToShow = true,
            CounterOfferId = offerId3
        });

        NegotiationResponse negotiationResYee4 = await _negotiationService.View(new NegotiationViewRequest
        {
            Id = negotiationId,
            Secret = employeeSecret,
            Side = Offer.OfferSide.Employee
        });

        Assert.Pass();
    }
}
