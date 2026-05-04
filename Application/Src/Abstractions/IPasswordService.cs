using Domain.Entities;

namespace Api.Application;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string passwordHash, string providedPassword);
}