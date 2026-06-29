namespace Respondr.Infrastructure.Authentication;

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Respondr.Infrastructure.Identity;
using Respondr.Shared.Constants;

public static class ApiAuthenticationExtensions
{
    public static IServiceCollection AddRespondrApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, currentConfiguration) =>
            {
                var jwtOptions = currentConfiguration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrWhiteSpace(accessToken) &&
                            path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.DispatcherOnly, policy => policy.RequireRole(RoleNames.Dispatcher));
            options.AddPolicy(PolicyNames.OperationsLeadOnly, policy => policy.RequireRole(RoleNames.OperationsLead));
            options.AddPolicy(
                PolicyNames.DispatcherOrOperationsLead,
                policy => policy.RequireRole(RoleNames.Dispatcher, RoleNames.OperationsLead));
        });

        return services;
    }
}
