using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Week2Api.Auth;
using Week2Api.Data;
using Week2Api.Middleware;
using Week2Api.OpenApi;
using Week2Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
builder.Services.AddControllers();

// EF Core with the in-memory database provider.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("Week2BooksDb"));

builder.Services.AddScoped<IBookService, BookService>();

// Authentication / authorization (produces real 401 and 403 responses).
builder.Services
    .AddAuthentication(TokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>(
        TokenAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure the in-memory database exists and is seeded.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Convert unhandled exceptions into 500 ProblemDetails.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Week2Api v1");
        options.RoutePrefix = "swagger"; // UI served at /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Exposed so the integration test project can use WebApplicationFactory<Program>.
public partial class Program;
