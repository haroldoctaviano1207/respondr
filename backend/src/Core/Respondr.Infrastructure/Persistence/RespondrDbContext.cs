namespace Respondr.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Respondr.Domain.Dispatch;
using Respondr.Domain.Identity;
using Respondr.Domain.Incidents;
using Respondr.Domain.Notifications;
using Respondr.Domain.Resources;

public sealed class RespondrDbContext : DbContext
{
    public RespondrDbContext(DbContextOptions<RespondrDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Incident> Incidents => Set<Incident>();

    public DbSet<IncidentHistory> IncidentHistories => Set<IncidentHistory>();

    public DbSet<IncidentLocation> IncidentLocations => Set<IncidentLocation>();

    public DbSet<ResponderProfile> ResponderProfiles => Set<ResponderProfile>();

    public DbSet<DispatchAssignment> DispatchAssignments => Set<DispatchAssignment>();

    public DbSet<ResourceRequest> ResourceRequests => Set<ResourceRequest>();

    public DbSet<ResourceItem> ResourceItems => Set<ResourceItem>();

    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RespondrDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
