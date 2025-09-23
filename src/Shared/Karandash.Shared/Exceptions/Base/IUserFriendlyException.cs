namespace Karandash.Shared.Exceptions.Base;

public interface IUserFriendlyException
{
    int StatusCode { get; }

    string Message { get; }
}