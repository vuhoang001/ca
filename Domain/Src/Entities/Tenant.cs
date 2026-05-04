
using Shared;

namespace Domain.Entities;

public sealed class Tenant : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private Tenant()
    {
    }

    public Tenant(string name, string slug)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
    }
}
