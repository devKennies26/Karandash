using Karandash.Shared.Exceptions.Base;
using Karandash.Shared.Utils.Methods;
using Microsoft.AspNetCore.Http;

namespace Karandash.Shared.Exceptions;

public class UserFriendlyBusinessException : Exception, IUserFriendlyException
{
    public int StatusCode { get; } = StatusCodes.Status400BadRequest;
    public string Message { get; }

    public UserFriendlyBusinessException(string key)
        : base(MessageHelper.GetMessage(key))
    {
        Message = MessageHelper.GetMessage(key);
    }

    public UserFriendlyBusinessException(string key, object param)
        : base(MessageHelper.GetMessage(key, param))
    {
        Message = MessageHelper.GetMessage(key, param);
    }
}