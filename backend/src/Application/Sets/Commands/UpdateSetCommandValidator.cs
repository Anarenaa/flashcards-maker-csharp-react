using FluentValidation;

namespace Application.Sets.Commands;

public class UpdateSetCommandValidator : AbstractValidator<UpdateSetCommand>
{
    public UpdateSetCommandValidator()
    {
        RuleFor(x => x.SetId)
            .GreaterThan(0).WithMessage("Invalid set ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 255).WithMessage("Name must be between 3 and 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.FromLang)
            .NotEmpty().WithMessage("From language is required.")
            .Length(2).WithMessage("From language must be a 2-character ISO code.");

        RuleFor(x => x.ToLang)
            .NotEmpty().WithMessage("To language is required.")
            .Length(2).WithMessage("To language must be a 2-character ISO code.");
    }
}
