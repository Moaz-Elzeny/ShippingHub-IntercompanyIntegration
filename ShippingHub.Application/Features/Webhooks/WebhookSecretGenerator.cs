using System.Security.Cryptography;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public static class WebhookSecretGenerator
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32); // 256-bit
        return Convert.ToBase64String(bytes);
    }
}
