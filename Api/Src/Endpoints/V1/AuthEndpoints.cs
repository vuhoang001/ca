using Application.Common;
using Application.Features.Auth.Commands;
using Infrastructure.Authentication;
using MediatR;
using Shared.Results;

namespace Api.Endpoints.V1;

public class AuthEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Auth");

        auth.MapPost("/register",
                     async (RegisterUserCommand command, ISender sender, CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(await sender.Send(command, cancellationToken))))
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        auth.MapPost("/login",
                     async (LoginCommand command, ISender sender, CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(await sender.Send(command, cancellationToken))))
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        auth.MapPost("/refresh-token",
                     async (RefreshTokenCommand command, ISender sender, CancellationToken cancellationToken) =>
                         Results.Ok(new ApiEnvelope<object>(await sender.Send(command, cancellationToken))))
            .AllowAnonymous()
            .RequireRateLimiting("auth");

        auth.MapPost("/change-password",
                     async (ChangePasswordCommand command, ISender sender, CancellationToken cancellationToken) =>
                     {
                         await sender.Send(command, cancellationToken);
                         return Results.NoContent();
                     })
            .RequireAuthorization()
            .RequireRateLimiting("default");

        auth.MapPost("/revoke",
                     async (RevokeTokenCommand command, ISender sender, CancellationToken cancellationToken) =>
                     {
                         await sender.Send(command, cancellationToken);
                         return Results.NoContent();
                     })
            .RequireAuthorization(policy => policy.RequirePermission(PermissionCodes.TokensRevoke))
            .RequireRateLimiting("default");
    }
}