using Fashia.Domain.Entities;
using FluentValidation;

namespace Fashia.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

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