using System.Security.Claims;
using Karandash.Shared.Enums.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Karandash.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute(params UserRole[] roles) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(meta => meta is AllowAnonymousAttribute);
        if (hasAllowAnonymous) return;

        ClaimsPrincipal user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string? userRoleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

        UserRole enumUserRoleClaim = Enum.Parse<UserRole>(userRoleClaim!);

        if (userRoleClaim != null && roles.Contains(enumUserRoleClaim)) return;
        context.Result = new ForbidResult();
    }
}