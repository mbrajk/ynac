namespace ynac.ErrorHandling;

public interface IErrorWriter
{
    void WriteError(string message);
    void WriteAuthError(string message);
    void WriteApiError(string message);
}
