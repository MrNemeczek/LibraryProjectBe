namespace LibraryProject.Application.Common.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(
        string code,
        string userMessage,
        int statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(userMessage)
    {
        Code = code;
        UserMessage = userMessage;
        StatusCode = statusCode;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public string Code { get; }
    public string UserMessage { get; }
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
