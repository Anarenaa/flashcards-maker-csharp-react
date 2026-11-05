using FluentValidation;

namespace Application.Flashcards.Commands;

public class CreateFlashcardCommandValidator : AbstractValidator<CreateFlashcardCommand>
{
    public CreateFlashcardCommandValidator()
    {
        RuleFor(x => x.Term)
            .NotEmpty().WithMessage("Term is required.")
            .MaximumLength(255).WithMessage("Term cannot exceed 255 characters.");

        RuleFor(x => x.Definition)
            .NotEmpty().WithMessage("Definition is required.")
            .MaximumLength(500).WithMessage("Definition cannot exceed 500 characters.");
    }
}
