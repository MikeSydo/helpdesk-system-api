using System.Net;
using System.Net.Http.Json;
using HelpDesk.Application.Categories.Dtos;

namespace HelpDesk.Tests.Integration;

public class CategoriesApiTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();

        HttpResponseMessage response = await _client.GetAsync($"/api/Categories/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();

        HttpResponseMessage response = await _client.DeleteAsync($"/api/Categories/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithEmptyName_ShouldReturnBadRequest()
    {
        CreateCategoryRequest request = new("", "Some description");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Categories", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WhenCategoryDoesNotExist_ShouldReturnNotFound()
    {
        Guid id = Guid.NewGuid();
        UpdateCategoryRequest request = new("New Name", null);

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/Categories/{id}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
