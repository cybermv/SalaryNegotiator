using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Vele.SalaryNegotiator.Web.Exceptions;

namespace Vele.SalaryNegotiator.Web.Controllers;

[ApiController, ApiExplorerSettings(IgnoreApi = true)]
[Route("")]
public class IndexController : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return await Task.FromResult(Ok("Hej! This is the Salary Negotiator API. Head on to /swagger to see what can be done."));
    }
}
