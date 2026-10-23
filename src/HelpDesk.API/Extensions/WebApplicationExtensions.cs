using HelpDesk.Application.Common.Interfaces;
using HelpDesk.DataAccess;
using HelpDesk.DataAccess.Seed;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.API.Extensions;

public static class WebApplicationExtensions
{
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);

    public static async Task ApplyDatabaseInitializationAsync(
        this WebApplication application,
        CancellationToken cancellationToken = default)
    {
        await InitializationLock.WaitAsync(cancellationToken);
        try
        {
            await using AsyncServiceScope scope = application.Services.CreateAsyncScope();
            AppDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IPasswordHasher passwordHasher =
                scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            await dbContext.Database.MigrateAsync(cancellationToken);

            bool shouldSeed = application.Environment.IsDevelopment() &&
                application.Configuration.GetValue<bool>("DatabaseInitialization:Seed");

            if (shouldSeed)
            {
                await DbSeeder.SeedAsync(dbContext, passwordHasher, cancellationToken);
            }
        }
        finally
        {
            InitializationLock.Release();
        }
    }
}
