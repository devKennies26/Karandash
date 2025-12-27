using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Users;

public class UpdatePasswordDto
{
    private string? _oldPassword;

    public string OldPassword
    {
        get => _oldPassword!;
        set => _oldPassword = value?.Trim();
    }

    private string? _newPassword;

    public string? NewPassword
    {
        get => _newPassword!;
        set => _newPassword = value?.Trim();
    }
}

public class UpdatePasswordDtoValidator : AbstractValidator<UpdatePasswordDto>
{
    public UpdatePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Password")))
            .MinimumLength(8)
            .WithMessage(MessageHelper.GetMessage("FieldMinLength", MessageHelper.GetMessage("FieldName-Password"), 8));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-NewPassword")))
            .MinimumLength(8)
            .WithMessage(MessageHelper.GetMessage("FieldMinLength", MessageHelper.GetMessage("FieldName-NewPassword"),
                8));
    }
}