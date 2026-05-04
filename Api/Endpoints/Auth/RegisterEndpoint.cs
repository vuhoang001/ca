using Application.Auth.Commands.Register;
using Shared.Extensions.EndPoints;
using Shared.Shared.Core;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Endpoints.Auth;

public class RegisterEndpoint : IEndpoint<Ok<Guid>, RegisterCommand, ISender>
{
    public async Task<Ok<Guid>> HandleAsync(RegisterCommand cmd, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(cmd, cancellationToken);
        return TypedResults.Ok(result);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/register",
                    async ([FromBody] RegisterCommand command, [FromServices] ISender sender, CancellationToken cancellationToken) =>
                    await HandleAsync(command, sender, cancellationToken)
            )
            .ProducesPostWithoutLocation<Guid>()
            .WithTags(nameof(Domain.Entities.Auth))
            .WithName(nameof(Domain.Entities.Auth))
            .WithSummary("Register")
            .WithDescription("Register a new user with email and password.")
            .MapToApiVersion(Versions.V1);
    }
}