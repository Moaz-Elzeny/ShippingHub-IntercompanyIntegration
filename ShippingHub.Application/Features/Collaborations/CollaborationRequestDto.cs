using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Collaborations;

public sealed record CollaborationRequestDto(
    int Id,
    int SenderCompanyId,
    string SenderCompanyName,
    int ReceiverCompanyId,
    string ReceiverCompanyName,
    CollaborationRequestStatus Status,
    DateTime CreatedAtUtc
);

public sealed record SendCollaborationRequestRequest(int ReceiverCompanyId);
