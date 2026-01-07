using Microsoft.EntityFrameworkCore;
using ShippingHub.Domain.Entities;

namespace ShippingHub.Infrastructure.Persistence;

public static class WebhookEventsSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        // لو موجودين خلاص متزرعيش تاني
        if (await db.WebhookEvents.AnyAsync(ct))
            return;

        db.WebhookEvents.AddRange(
            new WebhookEvent { EventCode = "SHIPMENT_CREATED", Description = "Triggered when a shipment is created." },
            new WebhookEvent { EventCode = "SHIPMENT_UPDATED", Description = "Triggered when a shipment is updated." },
            new WebhookEvent { EventCode = "SHIPMENT_STATUS_CHANGED", Description = "Triggered when shipment status changes." }
        );

        await db.SaveChangesAsync(ct);
    }
}
