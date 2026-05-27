namespace Fashia.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.BrandId)
            .GreaterThan(0);
    }
}
