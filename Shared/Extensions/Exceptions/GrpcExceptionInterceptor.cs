// namespace FoxAI.Shared.Infrastructure.Extensions.Exceptions;
//
// public class GrpcExceptionInterceptor : Interceptor
// {
//     public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
//         TRequest request,
//         ServerCallContext context,
//         UnaryServerMethod<TRequest, TResponse> continuation
//     )
//     {
//         try
//         {
//             return await continuation(request, context);
//         }
//         catch (Exception exception)
//         {
//             throw new RpcException(new Status(StatusCode.Internal, exception.Message));
//         }
//     }
// }
