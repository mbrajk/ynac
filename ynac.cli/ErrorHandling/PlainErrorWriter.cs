namespace ynac.ErrorHandling;

public class PlainErrorWriter : IErrorWriter
{
    public void WriteError(string message)
        => Console.Error.WriteLine($"Error: {message}");

    public void WriteAuthError(string message)
        => Console.Error.WriteLine($"Authentication Error: {message}");

    public void WriteApiError(string message)
        => Console.Error.WriteLine($"API Error: {message}");
}
