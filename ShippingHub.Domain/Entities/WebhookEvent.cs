namespace ShippingHub.Domain.Entities
{
    public class WebhookEvent
    {
        public int Id { get; set; }
        public string EventCode { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
