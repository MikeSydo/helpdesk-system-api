using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Tests.Domain;

public class UserTests
{
    [Fact]
    public void CreateUser_WhenCalled_ShouldCreateUser()
    {
        string userName = Guid.NewGuid().ToString();
        string userEmail = Guid.NewGuid().ToString();
        string userPasswordHash = Guid.NewGuid().ToString();
        UserRole userRole = UserRole.User;
        
        User user = User.Create(userName, userEmail, userPasswordHash, userRole);
        
        Assert.NotEqual(user.Id, Guid.Empty);
        Assert.Equal(userName, user.Name);
        Assert.Equal(userEmail, user.Email);
        Assert.Equal(userPasswordHash, user.PasswordHash);
        Assert.Equal(userRole, user.Role);
        Assert.True(user.IsActive);
        Assert.NotEqual(default, user.CreatedAt);
        Assert.NotEqual(default, user.UpdatedAt);
    }

    [Fact]
    public void ChangeRole_WhenCalled_ShouldChangeRole()
    {
        string userName = Guid.NewGuid().ToString();
        string userEmail = Guid.NewGuid().ToString();
        string userPasswordHash = Guid.NewGuid().ToString();
        UserRole userRole = UserRole.User;
        
        User user = User.Create(userName, userEmail, userPasswordHash, userRole);
        user.ChangeRole(UserRole.Admin);

        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public void Deactivate_WhenCalled_ShouldDeactivateUser()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        DateTime previousUpdatedAt = user.UpdatedAt;

        Thread.Sleep(1);
        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.True(user.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenCalledTwice_ShouldKeepUserDeactivatedAndNotUpdateTimestamp()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        user.Deactivate();
        DateTime updatedAtAfterFirstDeactivation = user.UpdatedAt;

        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.Equal(updatedAtAfterFirstDeactivation, user.UpdatedAt);
    }

    [Fact]
    public void UpdateName_WhenCalled_ShouldTrimNameAndUpdateTimestamp()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        DateTime previousUpdatedAt = user.UpdatedAt;

        Thread.Sleep(1);
        user.UpdateName("  Updated name  ");

        Assert.Equal("Updated name", user.Name);
        Assert.True(user.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WhenCalled_ShouldTrimEmailAndUpdateTimestamp()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        DateTime previousUpdatedAt = user.UpdatedAt;

        Thread.Sleep(1);
        user.UpdateEmail("  updated@example.com  ");

        Assert.Equal("updated@example.com", user.Email);
        Assert.True(user.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void UpdatePasswordHash_WhenCalled_ShouldUpdateHashAndTimestamp()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        DateTime previousUpdatedAt = user.UpdatedAt;

        Thread.Sleep(1);
        user.UpdatePasswordHash("updated-hash");

        Assert.Equal("updated-hash", user.PasswordHash);
        Assert.True(user.UpdatedAt > previousUpdatedAt);
    }

    [Fact]
    public void ChangeRole_WhenCalled_ShouldUpdateTimestamp()
    {
        User user = User.Create("Name", "email@example.com", "hash");
        DateTime previousUpdatedAt = user.UpdatedAt;

        Thread.Sleep(1);
        user.ChangeRole(UserRole.Admin);

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.UpdatedAt > previousUpdatedAt);
    }
}