using Domain.Sets;

namespace Application.Sets.Exceptions;

public abstract class SetException(SetId setId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public SetId SetId { get; } = setId;
}

public class SetNotFoundException(SetId setId)
    : SetException(setId, $"Set not found under id {setId}");

public class SetAlreadyExistsException(SetId setId)
    : SetException(setId, $"Set already exists under id {setId}");

public class SetUnhandledException(SetId setId, Exception? innerException = null)
    : SetException(setId, $"Unhandled set exception under id {setId}", innerException);
