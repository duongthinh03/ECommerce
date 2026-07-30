using ECommerceApi.DTOs.Order;
using FluentValidation;


namespace ECommerceApi.Validators
{
    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.ShipRecipient).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ShipPhone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.ShipProvince).NotEmpty();
            RuleFor(x => x.ShipDistrict).NotEmpty();
            RuleFor(x => x.ShipWard).NotEmpty();
            RuleFor(x => x.ShipAddressLine).NotEmpty().MaximumLength(255);
        }
    }
}
