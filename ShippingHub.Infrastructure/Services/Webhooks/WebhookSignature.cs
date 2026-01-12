using System.Security.Cryptography;
using System.Text;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public static class WebhookSignature
{
    public static string Sign(string secret, string timestamp, string rawBody)
    {
        var payload = $"{timestamp}.{rawBody}";
        using var hmac = new HMACSHA256(Convert.FromBase64String(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
