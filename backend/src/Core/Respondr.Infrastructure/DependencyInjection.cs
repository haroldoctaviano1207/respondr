namespace Respondr.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respondr.Application.Identity;
using Respondr.Application.Realtime;
using Respondr.Infrastructure.Identity;
using Respondr.Infrastructure.Persistence;
using Respondr.Infrastructure.Realtime;
using Respondr.Shared.Time;

public static class DependencyInjection
{
    public static IServiceCollection AddRespondrInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<JwtOptions>? configureJwt = null,
        Action<IdentitySeedOptions>? configureIdentitySeed = null,
        Action<RealtimeApiOptions>? configureRealtime = null)
    {
        services.AddDbContext<RespondrDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(RespondrDbContext).Assembly.FullName);
            }));

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<IRealtimePublisher, HttpRealtimePublisher>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RealtimeApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

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

        if (configureRealtime is not null)
        {
            services.Configure(configureRealtime);
        }
        else
        {
            services.AddOptions<RealtimeApiOptions>();
        }

        return services;
    }
}
