using Microsoft.EntityFrameworkCore;
using ShippingHub.Domain.Entities;

namespace ShippingHub.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<CollaborationRequest> CollaborationRequests { get; }
    DbSet<WebhookEvent> WebhookEvents { get; }
    DbSet<Webhook> Webhooks { get; }
    DbSet<Shipment> Shipments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
