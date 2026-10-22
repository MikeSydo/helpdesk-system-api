using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.API.Security;

public sealed class PasswordHasher(IPasswordHasher<User> passwordHasher) : IPasswordHasher
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);
    public bool Verify(User user, string password, string passwordHash) => 
        passwordHasher.VerifyHashedPassword(user, passwordHash, password)
            is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
}