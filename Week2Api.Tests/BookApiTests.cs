using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Week2Api.Models;
using Week2Api.Services;

namespace Week2Api.Tests;

public class BookApiTests
{
    private const string AdminToken = "admin-token";
    private const string UserToken = "user-token";

    // Each test gets its own factory => its own isolated, freshly-seeded in-memory database.
    private static WebApplicationFactory<Program> CreateFactory(Action<IServiceCollection>? configureServices = null)
        => new CustomFactory(configureServices);

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, string? token = null)
    {
        var client = factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static Book ValidBook() => new()
    {
        Title = "Refactoring",
        Author = "Martin Fowler",
        Year = 1999,
        Isbn = "978-0201485677",
    };

    // ---------- 200 OK ----------

    [Fact]
    public async Task GetAll_Returns200_WithSeededBooks()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory);

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        Assert.NotNull(books);
        Assert.True(books!.Count >= 2);
    }

    [Fact]
    public async Task GetById_ExistingBook_Returns200()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory);

        var response = await client.GetAsync("/api/books/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(book);
        Assert.Equal(1, book!.Id);
    }

    // ---------- 201 Created ----------

    [Fact]
    public async Task Create_AsAdmin_WithValidBody_Returns201_AndLocationHeader()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, AdminToken);

        var response = await client.PostAsJsonAsync("/api/books", ValidBook());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Refactoring", created.Title);
    }

    // ---------- 204 No Content ----------

    [Fact]
    public async Task Update_AsAdmin_ExistingBook_Returns204()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, AdminToken);

        var updated = ValidBook();
        updated.Title = "Clean Code (2nd ed.)";

        var response = await client.PutAsJsonAsync("/api/books/2", updated);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAdmin_ExistingBook_Returns204()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, AdminToken);

        var response = await client.DeleteAsync("/api/books/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ---------- 400 Bad Request ----------

    [Fact]
    public async Task Create_AsAdmin_WithInvalidBody_Returns400()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, AdminToken);

        var invalid = new Book { Title = "", Author = "", Year = 0 }; // fails validation attributes

        var response = await client.PostAsJsonAsync("/api/books", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- 401 Unauthorized ----------

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory); // no Authorization header

        var response = await client.PostAsJsonAsync("/api/books", ValidBook());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- 403 Forbidden ----------

    [Fact]
    public async Task Create_AsNonAdmin_Returns403()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, UserToken); // authenticated but role "User"

        var response = await client.PostAsJsonAsync("/api/books", ValidBook());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- 404 Not Found ----------

    [Fact]
    public async Task GetById_MissingBook_Returns404()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory);

        var response = await client.GetAsync("/api/books/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAdmin_MissingBook_Returns404()
    {
        using var factory = CreateFactory();
        var client = CreateClient(factory, AdminToken);

        var response = await client.DeleteAsync("/api/books/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- 500 Internal Server Error ----------

    [Fact]
    public async Task GetAll_WhenServiceThrows_Returns500()
    {
        using var factory = CreateFactory(services =>
        {
            // Replace the real service with one that always throws.
            services.RemoveAll<IBookService>();
            services.AddScoped<IBookService, ThrowingBookService>();
        });
        var client = CreateClient(factory);

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private sealed class ThrowingBookService : IBookService
    {
        public Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<bool> UpdateAsync(int id, Book book, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }

    private sealed class CustomFactory(Action<IServiceCollection>? configureServices)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (configureServices is not null)
                builder.ConfigureTestServices(configureServices);
        }
    }
}
