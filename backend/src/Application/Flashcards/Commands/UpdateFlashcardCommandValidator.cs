using FluentValidation;

namespace Application.Flashcards.Commands;

public class UpdateFlashcardCommandValidator : AbstractValidator<UpdateFlashcardCommand>
{
    public UpdateFlashcardCommandValidator()
    {
        RuleFor(x => x.FlashcardId)
            .GreaterThan(0).WithMessage("Invalid flashcard ID.");

        RuleFor(x => x.Term)
            .NotEmpty().WithMessage("Term is required.")
            .MaximumLength(255).WithMessage("Term cannot exceed 255 characters.");

        RuleFor(x => x.Definition)
            .NotEmpty().WithMessage("Definition is required.")
            .MaximumLength(500).WithMessage("Definition cannot exceed 500 characters.");
    }
}
