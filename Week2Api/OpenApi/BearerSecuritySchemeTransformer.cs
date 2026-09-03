using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Week2Api.Auth;

namespace Week2Api.OpenApi;

/// <summary>
/// Adds a Bearer security scheme to the generated OpenAPI document so the
/// Swagger UI shows an "Authorize" button. Paste one of the demo tokens
/// (admin-token or user-token) to exercise the protected endpoints.
/// </summary>
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            Description = "Enter a demo token: 'admin-token' (Admin) or 'user-token' (User).",
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[TokenAuthenticationHandler.SchemeName] = scheme;

        // Apply the scheme globally so the "Authorize" token is sent on every request.
        var reference = new OpenApiSecuritySchemeReference(TokenAuthenticationHandler.SchemeName, document);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [reference] = new List<string>(),
        });

        return Task.CompletedTask;
    }
}
