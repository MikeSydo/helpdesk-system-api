using System.ComponentModel.DataAnnotations;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole  Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; }
    public DateTime CreatedAt { get; private set; } =  DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private User(string name, string email, string passwordHash, UserRole role)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Create(string name, string email, string passwordHash,  UserRole role = UserRole.User)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        return new User(name.Trim(), NormalizeEmail(email), passwordHash, role);
    }
    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Touch();
    }
    public void UpdateEmail(string email)
    {
        Email = NormalizeEmail(email);
        Touch();
    }
    public void UpdatePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        Touch();
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
        {
            return;
        }

        EmailConfirmed = true;
        Touch();
    }
    public void ChangeRole(UserRole role)
    {
        Role = role;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        string normalizedEmail = email.Trim();
        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new ArgumentException(
                "A valid email address is required.",
                nameof(email));
        }

        return normalizedEmail;
    }
}