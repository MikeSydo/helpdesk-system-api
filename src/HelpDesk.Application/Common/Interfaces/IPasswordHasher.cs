using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password, string passwordHash);
}