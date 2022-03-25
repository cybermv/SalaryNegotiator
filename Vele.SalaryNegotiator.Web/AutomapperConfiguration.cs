using AutoMapper;
using DTO = Vele.SalaryNegotiator.Core.Dto;
using GRPC = Vele.SalaryNegotiator.Grpc;

namespace Vele.SalaryNegotiator.Web;

public class AutomapperConfiguration : Profile
{
    public AutomapperConfiguration()
    {
        CreateMap<GRPC.NegotiationCreateRequest, DTO.NegotiationCreateRequest>();
        CreateMap<GRPC.NegotiationViewRequest, DTO.NegotiationViewRequest>();
        CreateMap<GRPC.NegotiationClaimRequest, DTO.NegotiationClaimRequest>();
        CreateMap<GRPC.NegotiationMakeOfferRequest, DTO.NegotiationMakeOfferRequest>();

        CreateMap<DTO.NegotiationCreateOrClaimResponse, GRPC.NegotiationCreateOrClaimResponse>();
        CreateMap<DTO.NegotiationMakeOfferResponse, GRPC.NegotiationMakeOfferResponse>();
        CreateMap<DTO.NegotiationResponse, GRPC.NegotiationResponse>();
        CreateMap<DTO.OfferResponse, GRPC.NegotiationResponse.Types.OfferResponse>();
    }
}