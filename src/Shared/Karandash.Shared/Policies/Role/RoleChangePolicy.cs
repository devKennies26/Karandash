using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Extensions.Role;

namespace Karandash.Shared.Policies.Role;

public static class RoleChangePolicy
{
    public static bool CanChangeRole(
        UserRole actorRole,
        UserRole targetCurrentRole,
        UserRole newRole,
        bool isSelfService)
    {
        if (targetCurrentRole == UserRole.Admin || newRole == UserRole.Admin)
            return false;

        if (isSelfService)
            return actorRole.IsRegularRole()
                   && targetCurrentRole.IsRegularRole()
                   && newRole.IsRegularRole();

        if (actorRole == UserRole.ContentCreator)
            return false;

        if (actorRole == UserRole.Moderator)
        {
            bool targetIsAllowed = targetCurrentRole.IsRegularRole() || targetCurrentRole == UserRole.ContentCreator;
            bool newRoleIsAllowed = newRole.IsRegularRole() || newRole == UserRole.ContentCreator;

            return targetIsAllowed && newRoleIsAllowed;
        }

        if (actorRole == UserRole.Admin)
            return true;

        return false;
    }
}