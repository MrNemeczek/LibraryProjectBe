namespace LibraryProject.Domain.Common;

public abstract class DomainException : Exception
{
    protected DomainException(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public string Code { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
