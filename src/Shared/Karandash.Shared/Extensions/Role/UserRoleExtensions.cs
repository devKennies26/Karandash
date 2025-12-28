using Karandash.Shared.Enums.Auth;

namespace Karandash.Shared.Extensions.Role;

public static class UserRoleExtensions
{
    public static bool IsSystemRole(this UserRole role) =>
        role is UserRole.Admin or UserRole.Moderator or UserRole.ContentCreator;

    public static bool IsRegularRole(this UserRole role) =>
        !role.IsSystemRole();
    
    public static bool IsOneOf(this UserRole role, params UserRole[] roles) =>
        roles.Contains(role);
}