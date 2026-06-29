using ECommerceApi.DTOs.Catalog;
using FluentValidation;

namespace ECommerceApi.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(170);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}