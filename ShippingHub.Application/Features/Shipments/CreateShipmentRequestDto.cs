using ShippingHub.Domain.Enums;

public sealed class CreateShipmentRequestDto
{
    // التعاون
    public int ReceiverCompanyId { get; set; }

    // بيانات العميل
    public string ClientName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string AddressDescription { get; set; } = default!;

    // مالية
    public decimal CashOnDelivery { get; set; }

    // حالة الشحنة (اختياري – الافتراضي CREATED)
    public ShipmentStatus? Status { get; set; }

    // Payload مرن للتكامل
    public object? Payload { get; set; }
}
