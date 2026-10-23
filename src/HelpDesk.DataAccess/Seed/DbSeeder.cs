using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.DataAccess.Seed;

public static class DbSeeder
{
    public const string AdminEmail = "admin@helpdesk.local";
    public const string SupportEmail = "support@helpdesk.local";
    public const string UserEmail = "user@helpdesk.local";
    private const string DefaultPassword = "HelpDesk123!";

    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        User unused = await GetOrCreateUserAsync(
            dbContext,
            passwordHasher,
            "HelpDesk Admin",
            AdminEmail,
            UserRole.Admin,
            cancellationToken);
        User support = await GetOrCreateUserAsync(
            dbContext,
            passwordHasher,
            "HelpDesk Support",
            SupportEmail,
            UserRole.Support,
            cancellationToken);
        User user = await GetOrCreateUserAsync(
            dbContext,
            passwordHasher,
            "HelpDesk User",
            UserEmail,
            UserRole.User,
            cancellationToken);

        string[] categoryNames = ["Hardware", "Software", "Network"];
        List<Category> categories = [];
        foreach (string categoryName in categoryNames)
        {
            Category? category = await dbContext.Categories
                .FirstOrDefaultAsync(
                    candidate => candidate.Name == categoryName,
                    cancellationToken);

            if (category is null)
            {
                category = Category.Create(
                    categoryName,
                    categoryName == "Hardware"
                        ? "Computer and device issues"
                        : categoryName == "Software"
                            ? "Applications and operating systems"
                            : "Connectivity and access issues");
                await dbContext.Categories.AddAsync(category, cancellationToken);
            }

            categories.Add(category);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await AddSeedTicketIfMissingAsync(
            dbContext,
            categories[0].Id,
            user.Id,
            support.Id,
            "Laptop does not start",
            "The laptop shows no sign of power.",
            TicketPriority.High,
            cancellationToken);
        await AddSeedTicketIfMissingAsync(
            dbContext,
            categories[1].Id,
            user.Id,
            support.Id,
            "Application installation request",
            "Install the approved accounting application.",
            TicketPriority.Medium,
            cancellationToken,
            TicketStatus.InProgress);
        await AddSeedTicketIfMissingAsync(
            dbContext,
            categories[2].Id,
            user.Id,
            support.Id,
            "VPN access issue",
            "VPN access was unavailable from the office network.",
            TicketPriority.Critical,
            cancellationToken,
            TicketStatus.Closed);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task AddSeedTicketIfMissingAsync(
        AppDbContext dbContext,
        Guid categoryId,
        Guid creatorId,
        Guid supportId,
        string title,
        string description,
        TicketPriority priority,
        CancellationToken cancellationToken,
        TicketStatus? status = null)
    {
        if (await dbContext.Tickets.AnyAsync(
                ticket => ticket.Title == title,
                cancellationToken))
        {
            return;
        }

        Ticket ticket = Ticket.Create(
            categoryId,
            creatorId,
            title,
            description,
            priority);

        if (status is TicketStatus.InProgress)
        {
            ticket.ChangeStatus(TicketStatus.InProgress);
            ticket.AssignTo(supportId);
        }
        else if (status is TicketStatus.Closed)
        {
            ticket.ChangeStatus(TicketStatus.InProgress);
            ticket.ChangeStatus(TicketStatus.Resolved);
            ticket.ChangeStatus(TicketStatus.Closed);
            ticket.AssignTo(supportId);
        }

        await dbContext.Tickets.AddAsync(ticket, cancellationToken);
    }

    private static async Task<User> GetOrCreateUserAsync(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        string name,
        string email,
        UserRole role,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .FirstOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        user = User.Create(name, email, "seed-password", role);
        user.UpdatePasswordHash(passwordHasher.Hash(user, DefaultPassword));
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }
}
