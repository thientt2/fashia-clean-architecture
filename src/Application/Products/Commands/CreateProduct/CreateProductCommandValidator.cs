using Fashia.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateProductCommandValidator(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.UploadedImageIds)
            .NotNull()
            .WithMessage("Product images are required.")
            .NotEmpty()
            .WithMessage("At least one image is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .MustAsync(CategoryExistsAsync)
            .WithMessage("Category not found.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .MustAsync(BrandExistsAsync)
            .WithMessage("Brand not found.");

        RuleFor(x => x.Variants)
            .NotNull()
            .WithMessage("Variants are required.")
            .NotEmpty()
            .WithMessage("At least one variant is required.")
            .MustAsync(AttributeValuesExistAsync)
            .WithMessage("One or more attribute values do not exist.");

        RuleForEach(x => x.Variants).NotNull().SetValidator(new CreateProductVariantDtoValidator());

        RuleFor(x => x)
            .MustAsync(AllImagesValidAsync)
            .WithMessage("Some images are invalid, already in use, or do not belong to you.")
            .OverridePropertyName("UploadedImageIds");
    }

    private Task<bool> CategoryExistsAsync(int id, CancellationToken ct) =>
        _context.Categories.AnyAsync(x => x.Id == id, ct);

    private Task<bool> BrandExistsAsync(int id, CancellationToken ct) =>
        _context.Brands.AnyAsync(x => x.Id == id, ct);

    private async Task<bool> AttributeValuesExistAsync(
        List<CreateProductVariantDto> variants,
        CancellationToken ct
    )
    {
        if (variants is null)
            return false;

        var ids = variants
            .Where(x => x is not null)
            .SelectMany(x => x.AttributeValueIds ?? [])
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return true;

        var count = await _context.AttributeValues.CountAsync(x => ids.Contains(x.Id), ct);

        return count == ids.Count;
    }

    private async Task<bool> AllImagesValidAsync(CreateProductCommand command, CancellationToken ct)
    {
        if (command.UploadedImageIds is null || command.Variants is null)
            return false;

        if (string.IsNullOrWhiteSpace(_user.Id))
            return false;

        var allImageIds = command
            .UploadedImageIds.Concat(command.Variants.SelectMany(v => v?.UploadedImageIds ?? []))
            .Distinct()
            .ToList();

        if (allImageIds.Count == 0)
            return true;

        var images = await _context
            .UploadedFiles.Where(x => allImageIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.IsUsed,
                x.CreatedBy,
            })
            .ToListAsync(ct);

        if (images.Count != allImageIds.Count)
            return false;
        if (images.Any(x => x.IsUsed))
            return false;
        if (images.Any(x => x.CreatedBy != _user.Id))
            return false;

        return true;
    }
}

public sealed class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
{
    public CreateProductVariantDtoValidator()
    {
        RuleFor(x => x.OriginalPrice)
            .GreaterThan(0)
            .WithMessage("Original price must be greater than zero.");

        RuleFor(x => x.UploadedImageIds)
            .NotNull()
            .WithMessage("Variant images are required.")
            .NotEmpty()
            .WithMessage("Variant must have at least one image.");

        RuleFor(x => x.AttributeValueIds)
            .NotNull()
            .WithMessage("Variant attribute values are required.")
            .NotEmpty()
            .WithMessage("Variant must have at least one attribute value.");
    }
}
