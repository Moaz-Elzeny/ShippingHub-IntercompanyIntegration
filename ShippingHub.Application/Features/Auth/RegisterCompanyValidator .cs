using FluentValidation;

namespace ShippingHub.Application.Features.Auth
{
    public sealed class RegisterCompanyValidator : AbstractValidator<RegisterCompanyCommand>
    {
        public RegisterCompanyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(200);
        }
    }
}
