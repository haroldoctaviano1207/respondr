namespace Respondr.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeRespondrDatabaseAsync(
        this IServiceProvider services,
        bool applyMigrations = true,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Respondr.DatabaseInitialization");
        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();

        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (applyMigrations)
                {
                    await dbContext.Database.MigrateAsync(cancellationToken);
                }
                else if (!await dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    throw new InvalidOperationException("The API could not connect to RespondrDb.");
                }

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database migration attempt {Attempt} of {MaxAttempts} failed. Retrying.",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        if (applyMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            return;
        }

        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException("The API could not connect to RespondrDb.");
        }
    }
}
