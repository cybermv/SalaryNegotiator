using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Vele.SalaryNegotiator.Web.Exceptions;

namespace Vele.SalaryNegotiator.Web.Controllers;

[ApiController, ExceptionFilter]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("view")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationAdminResponse))]
    public async Task<IActionResult> View([FromQuery] string secret)
    {
        IList<NegotiationAdminResponse> response = await _adminService.View(secret);
        return Ok(response);
    }

    [HttpDelete("delete")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationMakeOfferResponse))]
    public async Task<IActionResult> MakeOffer([FromQuery] string secret, [FromQuery] string id)
    {
        await _adminService.Delete(secret, id);
        return NoContent();
    }
}
