using Application.Flashcards.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class FlashcardErrorFactory
{
    public static ObjectResult ToObjectResult(this FlashcardException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                FlashcardNotFoundException => StatusCodes.Status404NotFound,
                FlashcardSetNotFoundException => StatusCodes.Status404NotFound,
                FlashcardUnhandledException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Flashcard error handler is not implemented.")
            }
        };
    }
}
