namespace Respondr.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Respondr.Application.Identity;
using Respondr.Domain.Identity;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Constants;

public static class IdentitySeedExtensions
{
    public static async Task SeedDevelopmentIdentityUsersAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySeedOptions>>().Value;
        if (!options.SeedDevelopmentUsers)
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();

        var dispatcherRole = await EnsureRoleAsync(dbContext, RoleNames.Dispatcher, cancellationToken);
        var operationsLeadRole = await EnsureRoleAsync(dbContext, RoleNames.OperationsLead, cancellationToken);

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            options.DispatcherEmail,
            options.DispatcherPassword,
            "Development Dispatcher",
            dispatcherRole.Id,
            cancellationToken);

        await EnsureUserAsync(
            dbContext,
            passwordHasher,
            options.OperationsLeadEmail,
            options.OperationsLeadPassword,
            "Development Operations Lead",
            operationsLeadRole.Id,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Role> EnsureRoleAsync(RespondrDbContext dbContext, string roleName, CancellationToken cancellationToken)
    {
        var existingRole = await dbContext.Roles.SingleOrDefaultAsync(role => role.Name == roleName, cancellationToken);
        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = new Role(Guid.NewGuid(), roleName);
        dbContext.Roles.Add(role);
        return role;
    }

    private static async Task EnsureUserAsync(
        RespondrDbContext dbContext,
        IPasswordHasherService passwordHasher,
        string email,
        string password,
        string fullName,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existingUser = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        dbContext.Users.Add(new User(
            Guid.NewGuid(),
            email.Trim(),
            passwordHasher.HashPassword(password),
            fullName,
            roleId));
    }
}
