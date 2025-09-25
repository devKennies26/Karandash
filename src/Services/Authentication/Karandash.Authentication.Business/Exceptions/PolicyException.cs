using Karandash.Shared.Exceptions.Base;
using Karandash.Shared.Utils.Methods;
using Microsoft.AspNetCore.Http;

namespace Karandash.Authentication.Business.Exceptions;

public class PolicyException : Exception, IUserFriendlyException
{
    public int StatusCode => StatusCodes.Status403Forbidden;
    public string Message { get; } = MessageHelper.GetMessage("TermsAndPolicyNotAccepted");
}