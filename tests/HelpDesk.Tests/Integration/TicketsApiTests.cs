using System.Net;
using System.Net.Http.Json;
using HelpDesk.Application.Tickets.Dtos;
using HelpDesk.Domain.Enums;

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
    public async Task Post_WithInvalidCategory_ShouldReturnServerError_ForNow()
    {
        CreateTicketRequest request = new(
            "VPN issue",
            "Cannot connect to VPN",
            Guid.NewGuid(),
            TicketPriority.High);

        Guid currentUserId = Guid.NewGuid();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            $"/api/Tickets?currentUserId={currentUserId}",
            request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}