using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Data;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Exceptions;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

namespace Vele.SalaryNegotiator.Core.Services;

public class AdminService : IAdminService
{
    private readonly SalaryNegotiatorDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NegotiationService> _logger;

    public AdminService(
        SalaryNegotiatorDbContext dbContext,
        IConfiguration configuration,
        ILogger<NegotiationService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IList<NegotiationAdminResponse>> View(string secret)
    {
        CheckSecret(secret);

        return await _dbContext.Negotiations
            .OrderBy(n => n.CreatedDate)
            .Select(n => new NegotiationAdminResponse
            {
                Id = n.Id,
                Name = n.Name,
                EmployerName = n.EmployerName,
                EmployerSecret = n.EmployerSecret,
                EmployeeName = n.EmployeeName,
                EmployeeSecret = n.EmployeeSecret,
                CreatedDate = n.CreatedDate,
                Offers = n.Offers
                          .OrderBy(o => o.CreatedDate)
                          .Select(o => new OfferAdminResponse
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
                          .ToList()
            })
            .ToListAsync();
    }

    public async Task Delete(string secret, string id)
    {
        CheckSecret(secret);

        throw new System.NotImplementedException();
    }

    private void CheckSecret(string secret)
    {
        string configuredSecret = _configuration["Admin:Secret"] ?? null;

        if (string.IsNullOrEmpty(configuredSecret))
        {
            _logger.LogInformation("Attempted to access Admin service while it is disabled");
            throw new NotFoundException();
        }

        if (!configuredSecret.Equals(secret, System.StringComparison.Ordinal))
        {
            _logger.LogWarning("Attempted to access Admin service with bad secret: '{BadSecret}'", secret);
            throw new ForbiddenException();
        }

    }
}
