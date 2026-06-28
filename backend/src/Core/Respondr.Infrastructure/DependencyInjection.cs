namespace Respondr.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respondr.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddRespondrInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<RespondrDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(RespondrDbContext).Assembly.FullName);
            }));

        return services;
    }
}
