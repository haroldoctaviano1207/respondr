namespace Respondr.Api.Tests.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Respondr.Infrastructure.Persistence;

public sealed class DbContextModelTests
{
    [Fact]
    public void Respondr_db_context_builds_model_with_expected_schemas()
    {
        var options = new DbContextOptionsBuilder<RespondrDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=RespondrDb_ModelTests;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(RespondrDbContext).Assembly.FullName))
            .Options;

        using var context = new RespondrDbContext(options);

        var schemas = context.Model
            .GetEntityTypes()
            .Select(static entityType => entityType.GetSchema())
            .Where(static schema => !string.IsNullOrWhiteSpace(schema))
            .Distinct()
            .ToArray();

        Assert.Contains("identity", schemas);
        Assert.Contains("incidents", schemas);
        Assert.Contains("dispatch", schemas);
        Assert.Contains("resources", schemas);
        Assert.Contains("notifications", schemas);
    }

    [Fact]
    public void Respondr_db_context_exposes_initial_migration()
    {
        var options = new DbContextOptionsBuilder<RespondrDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=RespondrDb_ModelTests;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                sql => sql.MigrationsAssembly(typeof(RespondrDbContext).Assembly.FullName))
            .Options;

        using var context = new RespondrDbContext(options);

        var migrations = context.Database.GetMigrations().ToArray();

        Assert.Contains(migrations, static migration => migration.EndsWith("InitialCore"));
    }
}
