using Fashia.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateProductCommandValidator(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;

        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .MustAsync(CategoryExistsAsync)
            .WithMessage("Category not found.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .MustAsync(BrandExistsAsync)
            .WithMessage("Brand not found.");

        // Chỉ validate nếu có ảnh mới
        When(
            x => x.NewUploadedImageIds.Count > 0,
            () =>
            {
                RuleFor(x => x)
                    .MustAsync(NewImagesValidAsync)
                    .WithMessage(
                        "Some images are invalid, already in use, or do not belong to you."
                    )
                    .OverridePropertyName("NewUploadedImageIds");
            }
        );
    }

    private Task<bool> CategoryExistsAsync(int id, CancellationToken ct) =>
        _context.Categories.AnyAsync(x => x.Id == id, ct);

    private Task<bool> BrandExistsAsync(int id, CancellationToken ct) =>
        _context.Brands.AnyAsync(x => x.Id == id, ct);

    private async Task<bool> NewImagesValidAsync(UpdateProductCommand command, CancellationToken ct)
    {
        var images = await _context
            .UploadedFiles.Where(x => command.NewUploadedImageIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.IsUsed,
                x.CreatedBy,
            })
            .ToListAsync(ct);

        if (images.Count != command.NewUploadedImageIds.Count)
            return false;
        if (images.Any(x => x.IsUsed))
            return false;
        if (images.Any(x => x.CreatedBy != _user.Id))
            return false;

        return true;
    }
}
