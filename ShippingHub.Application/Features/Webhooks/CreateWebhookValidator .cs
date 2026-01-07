using FluentValidation;

namespace ShippingHub.Application.Features.Webhooks;

public sealed class CreateWebhookValidator : AbstractValidator<CreateWebhookCommand>
{
    public CreateWebhookValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0);
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .WithMessage("Invalid URL.");
    }
}
