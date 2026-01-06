namespace ynab;

/// <summary>
/// Exception thrown when authentication with the YNAB API fails (401 Unauthorized)
/// </summary>
public class YnabAuthenticationException : Exception
{
    public YnabAuthenticationException() 
        : base("Authentication failed. The provided API token is invalid or has expired.")
    {
    }

    public YnabAuthenticationException(string message) 
        : base(message)
    {
    }

    public YnabAuthenticationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
