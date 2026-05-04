using Domain.Entities;

namespace Application.Abstractions;

public interface IClientAppRepository
{
    Task<ClientApp?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default);
}