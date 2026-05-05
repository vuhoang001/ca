using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string passwordHash, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}