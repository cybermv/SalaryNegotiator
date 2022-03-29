using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Services.Interfaces;
using DTO = Vele.SalaryNegotiator.Core.Dto;
using GRPC = Vele.SalaryNegotiator.Grpc;

namespace Vele.SalaryNegotiator.Web.Services;

public class AdminServiceImpl : GRPC.AdminService.AdminServiceBase
{
    private readonly IAdminService _adminService;
    private readonly IMapper _mapper;
    private readonly ILogger<NegotiationServiceImpl> _logger;

    public AdminServiceImpl(IAdminService adminService, IMapper mapper, ILogger<NegotiationServiceImpl> logger)
    {
        _adminService = adminService;
        _mapper = mapper;
        _logger = logger;
    }

    public async override Task<GRPC.NegotiationAdminResponseList> View(GRPC.NegotiationAdminViewRequest request, ServerCallContext context)
    {
        IList<DTO.NegotiationAdminResponse> responseDto = await _adminService.View(request.Secret);

        GRPC.NegotiationAdminResponseList response = new GRPC.NegotiationAdminResponseList();
        foreach (DTO.NegotiationAdminResponse item in responseDto)
        {
            GRPC.NegotiationAdminResponse mappedItem = _mapper.Map<DTO.NegotiationAdminResponse, GRPC.NegotiationAdminResponse>(item);
            response.Data.Add(mappedItem);
        }

        return response;
    }

    public async override Task<Empty> Delete(GRPC.NegotiationAdminDeleteRequest request, ServerCallContext context)
    {
        await _adminService.Delete(request.Secret, request.Id);
        return new Empty();
    }
}
