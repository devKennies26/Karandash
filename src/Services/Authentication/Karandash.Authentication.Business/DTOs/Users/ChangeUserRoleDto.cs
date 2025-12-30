using FluentValidation;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Users;

public class ChangeUserRoleDto
{
    public Guid TargetUserId { get; set; }
    public UserRole NewRole { get; set; }
}

public class ChangeUserRoleDtoValidator : AbstractValidator<ChangeUserRoleDto>
{
    public ChangeUserRoleDtoValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEqual(Guid.Empty)
            .WithMessage(MessageHelper.GetMessage("InvalidTargetUserId"));
        
        RuleFor(x => x.NewRole)
            .IsInEnum()
            .WithMessage(MessageHelper.GetMessage("InvalidUserRole"));
    }
}