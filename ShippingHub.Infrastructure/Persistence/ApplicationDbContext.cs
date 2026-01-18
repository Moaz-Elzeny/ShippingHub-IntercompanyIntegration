using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;

namespace ShippingHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CollaborationRequest> CollaborationRequests => Set<CollaborationRequest>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ApiKey).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Name).IsUnique(false);
        });

        modelBuilder.Entity<WebhookEvent>(e =>
        {
            e.Property(x => x.EventCode).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.EventCode).IsUnique();
        });

        modelBuilder.Entity<Webhook>(e =>
        {
            e.Property(x => x.Url).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Secret).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.CompanyId, x.EventId }).IsUnique();
        });

        modelBuilder.Entity<CollaborationRequest>(e =>
        {
            e.HasIndex(x => new { x.SenderCompanyId, x.ReceiverCompanyId }).IsUnique();
        });
        modelBuilder.Entity<WebhookDelivery>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventCode).HasMaxLength(100);
            e.HasIndex(x => new { x.Status, x.NextRetryAtUtc });
            e.HasIndex(x => x.CorrelationId);
        });
        modelBuilder.Entity<IdempotencyKey>(e =>
        {
            e.HasIndex(x => new { x.CompanyId, x.Key }).IsUnique();
            e.Property(x => x.Key).HasMaxLength(200);
        });
        modelBuilder.Entity<Shipment>(e =>
        {
            e.Property(x => x.ClientName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.AddressDescription).HasMaxLength(1000).IsRequired();
        });

    }
}
