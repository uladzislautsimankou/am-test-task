namespace AM.TestTask.Business.Exceptions;

public sealed class SynchronizationException : Exception
{
    public SynchronizationException(string message)
        : base(message) { }

    public SynchronizationException(string message, Exception innerException)
        : base(message, innerException) { }
}