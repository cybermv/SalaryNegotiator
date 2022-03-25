using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

using DTO = Vele.SalaryNegotiator.Core.Dto;
using GRPC = Vele.SalaryNegotiator.Grpc;

namespace Vele.SalaryNegotiator.Web.Services
{
    public class NegotiationServiceImpl : GRPC.NegotiationService.NegotiationServiceBase
    {
        private readonly INegotiationService _negotiationService;
        private readonly IMapper _mapper;
        private readonly ILogger<NegotiationServiceImpl> _logger;

        public NegotiationServiceImpl(
            INegotiationService negotiationService,
            IMapper mapper,
            ILogger<NegotiationServiceImpl> logger)
        {
            _negotiationService = negotiationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async override Task<GRPC.NegotiationCreateOrClaimResponse> Create(GRPC.NegotiationCreateRequest request, ServerCallContext context)
        {
            DTO.NegotiationCreateRequest requestDto = _mapper.Map<GRPC.NegotiationCreateRequest, DTO.NegotiationCreateRequest>(request);

            DTO.NegotiationCreateOrClaimResponse responseDto = await _negotiationService.Create(requestDto);

            GRPC.NegotiationCreateOrClaimResponse response = _mapper.Map<DTO.NegotiationCreateOrClaimResponse, GRPC.NegotiationCreateOrClaimResponse>(responseDto);

            return response;
        }

        public async override Task<GRPC.NegotiationResponse> View(GRPC.NegotiationViewRequest request, ServerCallContext context)
        {
            DTO.NegotiationViewRequest requestDto = _mapper.Map<GRPC.NegotiationViewRequest, DTO.NegotiationViewRequest>(request);

            DTO.NegotiationResponse responseDto = await _negotiationService.View(requestDto);

            GRPC.NegotiationResponse response = _mapper.Map<DTO.NegotiationResponse, GRPC.NegotiationResponse>(responseDto);

            return response;
        }

        public async override Task<GRPC.NegotiationCreateOrClaimResponse> Claim(GRPC.NegotiationClaimRequest request, ServerCallContext context)
        {
            DTO.NegotiationClaimRequest requestDto = _mapper.Map<GRPC.NegotiationClaimRequest, DTO.NegotiationClaimRequest>(request);

            DTO.NegotiationCreateOrClaimResponse responseDto = await _negotiationService.Claim(requestDto);

            GRPC.NegotiationCreateOrClaimResponse response = _mapper.Map<DTO.NegotiationCreateOrClaimResponse, GRPC.NegotiationCreateOrClaimResponse>(responseDto);

            return response;
        }

        public async override Task<GRPC.NegotiationMakeOfferResponse> MakeOffer(GRPC.NegotiationMakeOfferRequest request, ServerCallContext context)
        {
            DTO.NegotiationMakeOfferRequest requestDto = _mapper.Map<GRPC.NegotiationMakeOfferRequest, DTO.NegotiationMakeOfferRequest>(request);

            DTO.NegotiationMakeOfferResponse responseDto = await _negotiationService.MakeOffer(requestDto);

            GRPC.NegotiationMakeOfferResponse response = _mapper.Map<DTO.NegotiationMakeOfferResponse, GRPC.NegotiationMakeOfferResponse>(responseDto);

            return response;
        }
    }
}
