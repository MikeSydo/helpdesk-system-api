using HelpDesk.API;
using HelpDesk.API.Security;
using HelpDesk.Application.Categories;
using HelpDesk.Application.Tickets;
using HelpDesk.Application.Users;
using HelpDesk.Application.Auth;
using HelpDesk.DataAccess;
using HelpDesk.DataAccess.Repositories;
using HelpDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ApplicationPasswordHasher = HelpDesk.Application.Common.Interfaces.IPasswordHasher;
using AspNetPasswordHasher = Microsoft.AspNetCore.Identity.IPasswordHasher<HelpDesk.Domain.Entities.User>;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString(nameof(AppDbContext)));
});

// Dependency Injection
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketProcessingService, TicketProcessingService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<AspNetPasswordHasher, PasswordHasher<User>>();
builder.Services.AddScoped<ApplicationPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}