using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.WebhookDeliveries;

public sealed record WebhookDeliveryListItemDto(
    int Id,
    int WebhookId,
    string EventCode,
    WebhookDeliveryStatus Status,
    int AttemptCount,
    DateTime NextRetryAtUtc,
    DateTime CreatedAtUtc
);

public sealed record WebhookDeliveryDetailsDto(
    int Id,
    int TargetCompanyId,
    int WebhookId,
    string EventCode,
    string PayloadJson,
    Guid CorrelationId,
    WebhookDeliveryStatus Status,
    int AttemptCount,
    DateTime NextRetryAtUtc,
    string? LastError,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public sealed record WebhookDeliverySummaryDto(
    int Pending,
    int Success,
    int Failed
);
