using Fashia.Domain.Entities;
using FluentValidation;

namespace Fashia.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(Category.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(Category.MaxDescriptionLength);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(Category.MaxImageUrlLength);

        RuleFor(x => x.ParentId)
            .GreaterThan(0)
            .When(x => x.ParentId.HasValue);
    }
}