using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Dto;

namespace Vele.SalaryNegotiator.Core.Services.Interfaces;

public interface INegotiationService
{
    /// <summary>
    /// Creates a new negotiation session with the first offer. It can be used by either
    /// party, employee or employer.
    /// </summary>
    /// <param name="request">Data about the negotiation and first offer</param>
    /// <returns>The ID of the negotiation and secret for the first offering party</returns>
    Task<NegotiationCreateOrClaimResponse> Create(NegotiationCreateRequest request);

    /// <summary>
    /// Displays the whole negotiation session to one of the parties; it is neccessary to
    /// present a secret in order to view it
    /// </summary>
    /// <param name="request">The negotiation ID and requester party's secret</param>
    /// <returns>The full negotiation session for the requesting party</returns>
    Task<NegotiationResponse> View(NegotiationViewRequest request);

    /// <summary>
    /// Claims the created negotiation session. This endpoint is intended for the second
    /// party, after the first party creates the session.
    /// </summary>
    /// <param name="request">The ID of the negotiation session</param>
    /// <returns>The ID of the negotiation and secret for the claiming party</returns>
    Task<NegotiationCreateOrClaimResponse> Claim(NegotiationClaimRequest request);

    /// <summary>
    /// Makes a new offer in a negotiation session
    /// </summary>
    /// <param name="request">The new offer data</param>
    /// <returns>The ID of the newly made offer</returns>
    Task<NegotiationMakeOfferResponse> MakeOffer(NegotiationMakeOfferRequest request);
}
