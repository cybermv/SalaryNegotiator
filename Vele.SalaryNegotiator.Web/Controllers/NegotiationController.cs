using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Dto;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using Vele.SalaryNegotiator.Web.Filters;

namespace Vele.SalaryNegotiator.Web.Controllers;

[ApiController, ExceptionFilter]
[Route("negotiation")]
public class NegotiationController : ControllerBase
{
    private readonly INegotiationService _negotiationService;

    public NegotiationController(INegotiationService negotiationService)
    {
        _negotiationService = negotiationService;
    }

    [HttpPost("create")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationCreateOrClaimResponse))]
    public async Task<IActionResult> Create([FromBody] NegotiationCreateRequest request)
    {
        NegotiationCreateOrClaimResponse response = await _negotiationService.Create(request);
        return Ok(response);
    }

    [HttpPost("claim")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationCreateOrClaimResponse))]
    public async Task<IActionResult> Claim([FromBody] NegotiationClaimRequest request)
    {
        NegotiationCreateOrClaimResponse response = await _negotiationService.Claim(request);
        return Ok(response);
    }

    [HttpPost("view")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationResponse))]
    public async Task<IActionResult> View([FromBody] NegotiationViewRequest request)
    {
        NegotiationResponse response = await _negotiationService.View(request);
        return Ok(response);
    }

    [HttpPost("offer")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(NegotiationMakeOfferResponse))]
    public async Task<IActionResult> MakeOffer([FromBody] NegotiationMakeOfferRequest request)
    {
        NegotiationMakeOfferResponse response = await _negotiationService.MakeOffer(request);
        return Ok(response);
    }
}
