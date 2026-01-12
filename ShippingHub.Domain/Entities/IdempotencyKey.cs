namespace ShippingHub.Domain.Entities;

public class IdempotencyKey
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public string Key { get; set; } = "";

    public string RequestHash { get; set; } = "";
    public string ResponseBody { get; set; } = "";

    public int StatusCode { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
