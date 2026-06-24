using MailAssistant.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MailAssistant.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddMailAssistantAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Authentication:Authority"]
            ?? throw new InvalidOperationException(
                "Authentication authority is not configured.");
        var audience = configuration["Authentication:Audience"]
            ?? throw new InvalidOperationException(
                "Authentication audience is not configured.");
        var validIssuers = configuration
            .GetSection("Authentication:ValidIssuers")
            .Get<string[]>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = configuration.GetValue(
                    "Authentication:RequireHttpsMetadata",
                    true);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "preferred_username",
                };
            });
        services.AddAuthorization();

        return services;
    }
}
