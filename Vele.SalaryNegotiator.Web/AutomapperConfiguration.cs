using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using System;
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
        CreateMap<DTO.NegotiationResponse, GRPC.NegotiationResponse>()
            .ForMember(dest => dest.CreatedDate, act => act.MapFrom(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc))));
        CreateMap<DTO.OfferResponse, GRPC.NegotiationResponse.Types.OfferResponse>()
            .ForMember(dest => dest.CreatedDate, act => act.MapFrom(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src.CreatedDate, DateTimeKind.Utc))))
            .ForMember(dest => dest.Type, act => act.MapFrom(src => src.Type.HasValue ? new GRPC.OfferTypeValue { Value = (GRPC.OfferType)src.Type } : null));
    }
}