using ECommerceApi.DTOs.Catalog;
using FluentValidation;

namespace ECommerceApi.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(280);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.DisplayPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Thumbnail).MaximumLength(500);
    }
}