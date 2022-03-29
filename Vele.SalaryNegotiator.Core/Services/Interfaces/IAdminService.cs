using System.Collections.Generic;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Dto;

namespace Vele.SalaryNegotiator.Core.Services.Interfaces;

public interface IAdminService
{
    Task<IList<NegotiationAdminResponse>> View(string secret);

    Task Delete(string secret, string id);
}
