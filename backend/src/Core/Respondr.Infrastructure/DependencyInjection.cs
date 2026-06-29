namespace Respondr.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Respondr.Application.Identity;
using Respondr.Infrastructure.Identity;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Time;

public static class DependencyInjection
{
    public static IServiceCollection AddRespondrInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<JwtOptions>? configureJwt = null,
        Action<IdentitySeedOptions>? configureIdentitySeed = null)
    {
        services.AddDbContext<RespondrDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(RespondrDbContext).Assembly.FullName);
            }));

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        if (configureJwt is not null)
        {
            services.Configure(configureJwt);
        }
        else
        {
            services.AddOptions<JwtOptions>();
        }

        if (configureIdentitySeed is not null)
        {
            services.Configure(configureIdentitySeed);
        }
        else
        {
            services.AddOptions<IdentitySeedOptions>();
        }

        return services;
    }
}
