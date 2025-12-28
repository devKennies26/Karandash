using Karandash.Shared.Enums.Auth;

namespace Karandash.Authentication.Business.DTOs.Users;

public class ChangeUserRoleDto
{
    public Guid TargetUserId { get; set; }
    public UserRole NewRole { get; set; }
}