using System.Net;
using System.Net.Http.Json;
using HelpDesk.DataAccess;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.Integration;

public class TicketsApiTests(CustomWebApplicationFactory factory) : 
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ShouldReturnSuccessStatusCode()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithFiltersAndPagination_ShouldReturnPagedResponse()
    {
        Guid categoryId = Guid.NewGuid();

        await using (AsyncServiceScope scope = factory.Services.CreateAsyncScope())
        {
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Category category = Category.Create($"Filter category {Guid.NewGuid():N}");
            User user = User.Create(
                "Filter test user",
                $"filter-{Guid.NewGuid():N}@example.com",
                "hash");
            dbContext.Categories.Add(category);
            dbContext.Users.Add(user);
            dbContext.Tickets.AddRange(
                Ticket.Create(category.Id, user.Id, "High open 1", "Description", TicketPriority.High),
                Ticket.Create(category.Id, user.Id, "High open 2", "Description", TicketPriority.High),
                Ticket.Create(category.Id, user.Id, "Low open", "Description", TicketPriority.Low));
            await dbContext.SaveChangesAsync();
            categoryId = category.Id;
        }

        HttpResponseMessage response = await _client.GetAsync(
            $"/api/Tickets?status=Open&priority=High&categoryId={categoryId}&page=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TicketPageResponse? result = await response.Content.ReadFromJsonAsync<TicketPageResponse>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(categoryId, result.Items[0].CategoryId);
        Assert.Equal(TicketPriority.High, result.Items[0].Priority);
    }

    [Fact]
    public async Task GetAll_WithInvalidPage_ShouldReturnBadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Tickets?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenTicketDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();

        HttpResponseMessage response = await _client.GetAsync($"/api/Tickets/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenTicketDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/Tickets/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_WhenTicketDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();
        ChangeTicketStatusRequest request = new(TicketStatus.InProgress);

        HttpResponseMessage response = await _client.PatchAsJsonAsync(
            $"/api/Tickets/{id}/status",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidCategory_ShouldReturnNotFound()
    {
        CreateTicketRequest request = new(
            "VPN issue",
            "Cannot connect to VPN",
            Guid.NewGuid(),
            TicketPriority.High);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/Tickets",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}