using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Threading.Tasks;
using Vele.SalaryNegotiator.Core.Exceptions;

namespace Vele.SalaryNegotiator.Web.Exceptions;

public class GrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ForbiddenException ex)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
        }
        catch (BaseException ex)
        {
            throw new RpcException(new Status(StatusCode.Unknown, ex.Message));
        }
    }
}
