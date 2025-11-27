namespace AM.TestTask.Business.Exceptions;

public sealed class DataRetrievalException : Exception
{
    public DataRetrievalException(string message)
        : base(message) { }

    public DataRetrievalException(string message, Exception innerException)
        : base(message, innerException) { }
}