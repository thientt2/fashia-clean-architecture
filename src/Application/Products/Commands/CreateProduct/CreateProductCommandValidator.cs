using FluentValidation;

namespace Fashia.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.ImageUrl).MaximumLength(500);

        RuleFor(x => x.CategoryId).GreaterThan(0);

        RuleFor(x => x.BrandId).GreaterThan(0);

        RuleFor(x => x.Variants).NotEmpty();

        RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantDtoValidator());
    }
}

public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
{
    public CreateProductVariantDtoValidator()
    {
        RuleFor(x => x.OriginalPrice).GreaterThanOrEqualTo(0);

        RuleFor(x => x.AttributeValueIds).NotEmpty();
    }
}
