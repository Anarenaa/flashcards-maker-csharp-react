using Application.Sets.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class SetErrorFactory
{
    public static ObjectResult ToObjectResult(this SetException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                SetNotFoundException => StatusCodes.Status404NotFound,
                SetAlreadyExistsException => StatusCodes.Status409Conflict,
                SetUnhandledException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Set error handler is not implemented.")
            }
        };
    }
}