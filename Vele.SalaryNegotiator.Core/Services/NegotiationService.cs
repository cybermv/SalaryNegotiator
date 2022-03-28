using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Data.Entities;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Exceptions;
using Vele.SalaryNegotiator.Core.Generators.Interfaces;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

namespace Vele.SalaryNegotiator.Core.Services;

public class NegotiationService : INegotiationService
{
    private readonly SalaryNegotiatorDbContext _dbContext;
    private readonly ICodeGenerator _codeGenerator;
    private readonly ISecretGenerator _secretGenerator;
    private readonly ILogger<NegotiationService> _logger;

    public NegotiationService(
        SalaryNegotiatorDbContext dbContext,
        ICodeGenerator codeGenerator,
        ISecretGenerator secretGenerator,
        ILogger<NegotiationService> logger)
    {
        _dbContext = dbContext;
        _codeGenerator = codeGenerator;
        _secretGenerator = secretGenerator;
        _logger = logger;
    }

    public async Task<NegotiationCreateOrClaimResponse> Create(NegotiationCreateRequest request)
    {
        _logger.LogInformation("Creating new negotiation with data {@NegotiationCreateRequest}", request);

        DateTime now = DateTime.UtcNow;
        NegotiationCreateRequest.Validator validator = new NegotiationCreateRequest.Validator();
        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new ValidationException(result);

        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            string secret = null;
            if (request.Side == Offer.OfferSide.Employer) secret = _secretGenerator.GenerateForEmployer();
            else if (request.Side == Offer.OfferSide.Employee) secret = _secretGenerator.GenerateForEmployee();

            Negotiation negotiation = new Negotiation
            {
                Id = _codeGenerator.Generate(),
                Name = request.NegotiationName,
                EmployerName = request.Side == Offer.OfferSide.Employer ? request.Name : null,
                EmployeeName = request.Side == Offer.OfferSide.Employee ? request.Name : null,
                CreatedDate = now,
                EmployerSecret = request.Side == Offer.OfferSide.Employer ? secret : null,
                EmployeeSecret = request.Side == Offer.OfferSide.Employee ? secret : null
            };

            _logger.LogInformation("Persisting Negotiation entity {@Negotiation}", negotiation);
            _dbContext.Negotiations.Add(negotiation);
            await _dbContext.SaveChangesAsync();

            Offer offer = new Offer
            {
                Side = request.Side,
                Type = request.Type,
                Amount = request.Amount,
                MaxAmount = request.MaxAmount,
                MinAmount = request.MinAmount,
                CreatedDate = now,
                NeedsConterOfferToShow = request.NeedsCounterOfferToShow,
                NegotiationId = negotiation.Id
            };

            _logger.LogInformation("Persisting Offer entity {@Offer}", offer);
            _dbContext.Offers.Add(offer);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("Created new negotiation with ID {NegotiationId}", negotiation.Id);
            return new NegotiationCreateOrClaimResponse
            {
                Id = negotiation.Id,
                Secret = secret
            };
        }
    }

    public async Task<NegotiationResponse> View(NegotiationViewRequest request)
    {
        _logger.LogInformation("Viewing negotiation with data {@NegotiationViewRequest}", request);

        NegotiationViewRequest.Validator validator = new NegotiationViewRequest.Validator();
        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new ValidationException(result);

        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            _logger.LogInformation("Loading Negotiation with ID {NegotiationId}", request.Id);
            Negotiation negotiation = await _dbContext.Negotiations
            .FindAsync(request.Id);

            if (negotiation == null)
                throw new NotFoundException();

            if (request.Side == Offer.OfferSide.Employer &&
                negotiation.EmployerSecret != request.Secret)
                throw new ForbiddenException();

            if (request.Side == Offer.OfferSide.Employee &&
                negotiation.EmployeeSecret != request.Secret)
                throw new ForbiddenException();

            _logger.LogInformation("Loading Offers for negotiation with ID {NegotiationId}", negotiation.Id);
            List<OfferResponse> offers = await _dbContext.Offers
                .AsNoTracking()
                .OrderBy(o => o.CreatedDate)
                .Where(o => o.NegotiationId == negotiation.Id)
                .Select(o => new OfferResponse
                {
                    Id = o.Id,
                    Side = o.Side,
                    Type = o.Type,
                    Amount = o.Amount,
                    MaxAmount = o.MaxAmount,
                    MinAmount = o.MinAmount,
                    CreatedDate = o.CreatedDate,
                    NeedsCounterOfferToShow = o.NeedsConterOfferToShow,
                    CounterOfferId = o.CounterOfferId
                })
                .ToListAsync();

            foreach (OfferResponse offer in offers)
            {
                if (offer.Side != request.Side &&
                    offer.NeedsCounterOfferToShow &&
                    offer.CounterOfferId == null)
                {
                    _logger.LogInformation("Censoring offer with ID {OfferId}", offer.Id);
                    offer.Type = null;
                    offer.Amount = null;
                    offer.MaxAmount = null;
                    offer.MinAmount = null;
                }
            }

            _logger.LogInformation("Loaded negotiation with ID {NegotiationId} for side {NegotiationViewSide}", negotiation.Id, request.Side);
            return new NegotiationResponse
            {
                Id = negotiation.Id,
                Name = negotiation.Name,
                EmployerName = negotiation.EmployerName,
                EmployeeName = negotiation.EmployeeName,
                CreatedDate = negotiation.CreatedDate,
                Offers = offers
            };
        }
    }

    public async Task<NegotiationCreateOrClaimResponse> Claim(NegotiationClaimRequest request)
    {
        _logger.LogInformation("Claiming negotiation with data {@NegotiationClaimRequest}", request);

        NegotiationClaimRequest.Validator validator = new NegotiationClaimRequest.Validator();
        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new ValidationException(result);

        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            _logger.LogInformation("Loading Negotiation with ID {NegotiationId}", request.Id);
            Negotiation negotiation = await _dbContext.Negotiations
                .FindAsync(request.Id);

            if (negotiation == null)
                throw new NotFoundException();

            NegotiationCreateOrClaimResponse response = new NegotiationCreateOrClaimResponse
            {
                Id = negotiation.Id
            };

            if (request.Side == Offer.OfferSide.Employer)
            {
                if (negotiation.EmployerSecret != null)
                    throw new ForbiddenException();

                negotiation.EmployerName = request.Name;
                negotiation.EmployerSecret = _secretGenerator.GenerateForEmployer();
                response.Secret = negotiation.EmployerSecret;
                _dbContext.Update(negotiation);
                await _dbContext.SaveChangesAsync();
            }
            else if (request.Side == Offer.OfferSide.Employee)
            {
                if (negotiation.EmployeeSecret != null)
                    throw new ForbiddenException();

                negotiation.EmployeeName = request.Name;
                negotiation.EmployeeSecret = _secretGenerator.GenerateForEmployee();
                response.Secret = negotiation.EmployeeSecret;
                _dbContext.Update(negotiation);
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Claimed negotiation with ID {NegotiationId} for side {NegotiationClaimSide}", negotiation.Id, request.Side);
            await transaction.CommitAsync();
            return response;
        }
    }

    public async Task<NegotiationMakeOfferResponse> MakeOffer(NegotiationMakeOfferRequest request)
    {
        _logger.LogInformation("Making new offer with data {@NegotiationMakeOfferRequest}", request);

        NegotiationMakeOfferRequest.Validator validator = new NegotiationMakeOfferRequest.Validator();
        ValidationResult result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new ValidationException(result);

        using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            _logger.LogInformation("Loading Negotiation with ID {NegotiationId}", request.NegotiationId);
            Negotiation negotiation = await _dbContext.Negotiations
                .FindAsync(request.NegotiationId);

            if (negotiation == null)
                throw new NotFoundException();

            if (request.Side == Offer.OfferSide.Employer &&
                negotiation.EmployerSecret != request.Secret)
                throw new ForbiddenException();

            if (request.Side == Offer.OfferSide.Employee &&
                negotiation.EmployeeSecret != request.Secret)
                throw new ForbiddenException();

            _logger.LogInformation("Loading Offers for negotiation with ID {NegotiationId}", negotiation.Id);
            List<Offer> offers = await _dbContext.Offers
                .OrderBy(o => o.CreatedDate)
                .Where(o => o.NegotiationId == negotiation.Id)
                .ToListAsync();

            if (request.NeedsCounterOfferToShow && !request.CounterOfferId.HasValue &&
                offers.Any(o => o.NeedsConterOfferToShow &&
                                o.CounterOfferId == null))
            {
                throw new ValidationException("Cannot create closed offer when another closed one exists");
            }

            Offer newOffer = new Offer
            {
                Side = request.Side,
                Type = request.Type,
                Amount = request.Amount,
                MaxAmount = request.MaxAmount,
                MinAmount = request.MinAmount,
                CreatedDate = DateTime.UtcNow,
                NegotiationId = negotiation.Id,
                NeedsConterOfferToShow = request.NeedsCounterOfferToShow
            };

            _logger.LogInformation("Persisting Offer entity {@Offer}", newOffer);
            _dbContext.Offers.Add(newOffer);
            await _dbContext.SaveChangesAsync();

            if (request.CounterOfferId.HasValue)
            {
                Offer counteredOffer = offers.SingleOrDefault(o => o.Id == request.CounterOfferId.Value);
                if (counteredOffer == null)
                    throw new NotFoundException("Countered offer not found");
                if (!counteredOffer.NeedsConterOfferToShow)
                    throw new ValidationException("The countered offer is not counterable");
                if (counteredOffer.CounterOfferId.HasValue)
                    throw new ValidationException("Cannot counter an already countered offer");
                if (counteredOffer.Side == newOffer.Side)
                    throw new ValidationException("Cannot counter your own offer");

                newOffer.CounterOfferId = counteredOffer.Id;
                counteredOffer.CounterOfferId = newOffer.Id;
                _logger.LogInformation("Linking existing offer ID {ExistingOfferId} with new offer ID {NewOfferId}", counteredOffer.Id, newOffer.Id);
                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new NegotiationMakeOfferResponse { Id = newOffer.Id };
        }
    }
}
