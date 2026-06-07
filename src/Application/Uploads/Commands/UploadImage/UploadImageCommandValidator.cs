using FluentValidation;

namespace Fashia.Application.Uploads.Commands.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private static readonly string[] AllowedFolders =
    [
        "products",
        "categories",
        "product-variants",
        "brands",
    ];

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.FileStream).NotNull();

        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);

        RuleFor(x => x.ContentType)
            .Must(x => AllowedContentTypes.Contains(x))
            .WithMessage("Only JPEG, PNG and WEBP images are allowed.");

        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(5 * 1024 * 1024)
            .WithMessage("Image size must not exceed 5MB.");

        RuleFor(x => x.Folder)
            .Must(x => AllowedFolders.Contains(x))
            .WithMessage("Invalid upload folder.");
    }
}
