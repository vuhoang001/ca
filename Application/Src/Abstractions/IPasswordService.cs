using Domain.Entities;

namespace Application.Abstractions;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string passwordHash, string providedPassword);
}