namespace DawaAddress;

public class DawaEmptyResultException : Exception
{
    public DawaEmptyResultException()
    {
    }

    public DawaEmptyResultException(string? message) : base(message)
    {
    }

    public DawaEmptyResultException(
        string? message,
        Exception? innerException) : base(message, innerException)
    {
    }
}
