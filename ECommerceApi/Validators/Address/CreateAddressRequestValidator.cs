using ECommerceApi.DTOs.Address;
using FluentValidation;

namespace ECommerceApi.Validators;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(255);
    }
}
