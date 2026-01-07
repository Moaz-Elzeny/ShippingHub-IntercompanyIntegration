using FluentValidation;

namespace ShippingHub.Application.Features.Auth;

public sealed class LoginCompanyValidator : AbstractValidator<LoginCompanyCommand>
{
    public LoginCompanyValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0);
        RuleFor(x => x.ApiKey).NotEmpty();
    }
}
