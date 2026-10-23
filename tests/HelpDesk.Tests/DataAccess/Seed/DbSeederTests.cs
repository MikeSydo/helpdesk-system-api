using HelpDesk.DataAccess;
using HelpDesk.DataAccess.Seed;
using HelpDesk.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.DataAccess.Seed;

public sealed class DbSeederTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task StartupSeed_ShouldCreateReferenceUsersCategoriesAndTickets()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(
            user => user.Email == DbSeeder.AdminEmail));
        Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(
            user => user.Email == DbSeeder.SupportEmail));
        Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(
            user => user.Email == DbSeeder.UserEmail));
        Assert.True(await dbContext.Categories.CountAsync() >= 3);
        Assert.True(await dbContext.Tickets.CountAsync() >= 3);
    }

    [Fact]
    public async Task SeedAsync_ShouldBeIdempotent()
    {
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<HelpDesk.Application.Common.Interfaces.IPasswordHasher>();

        await DbSeeder.SeedAsync(dbContext, passwordHasher);

        Assert.Equal(
            1,
            await dbContext.Users.CountAsync(user =>
                user.Email == DbSeeder.AdminEmail));
        Assert.Equal(
            1,
            await dbContext.Users.CountAsync(user =>
                user.Email == DbSeeder.SupportEmail));
        Assert.Equal(
            1,
            await dbContext.Users.CountAsync(user =>
                user.Email == DbSeeder.UserEmail));

        foreach (string categoryName in new[] { "Hardware", "Software", "Network" })
        {
            Assert.Equal(
                1,
                await dbContext.Categories.CountAsync(category =>
                    category.Name == categoryName));
        }

        foreach (string title in new[]
                 {
                     "Laptop does not start",
                     "Application installation request",
                     "VPN access issue"
                 })
        {
            Assert.Equal(
                1,
                await dbContext.Tickets.CountAsync(ticket =>
                    ticket.Title == title));
        }
    }
}
