using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Auth.Shared.Extensions.EndPoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public interface IEndpoint<TResult> : IEndpoint
{
    Task<TResult> HandlerAsync();
}

public interface IEndpoint<TResult, in TRequest> : IEndpoint
{
    Ok<string> HandlerAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IEndpoint<TResult, in TRequest1, in TRequest2> : IEndpoint
{
    Task<TResult> HandleAsync(
        TRequest1 cmd,
        TRequest2 sender,
        CancellationToken cancellationToken = default
    );
}

public interface IEndpoint<TResult, in TRequest1, in TRequest2, in TRequest3> : IEndpoint
{
    Task<TResult> HandleAsync(
        TRequest1 request1,
        TRequest2 request2,
        TRequest3 request3,
        CancellationToken cancellationToken = default
    );
}