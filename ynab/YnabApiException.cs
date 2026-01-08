namespace ynab;

/// <summary>
/// Exception thrown when the YNAB API returns an error or when network issues occur
/// </summary>
public class YnabApiException : Exception
{
    public YnabApiException() 
        : base("An error occurred while communicating with the YNAB API. Please check your internet connection and try again.")
    {
    }

    public YnabApiException(string message) 
        : base(message)
    {
    }

    public YnabApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
