using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Auth;

public class PasswordResetDto
{
    private string? _token;

    public string Token
    {
        get => _token!;
        set => _token = value?.Trim();
    }

    private string? _newPassword;

    public string NewPassword
    {
        get => _newPassword!;
        set => _newPassword = value?.Trim();
    }
}

public class PasswordResetDtoValidator : AbstractValidator<PasswordResetDto>
{
    public PasswordResetDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Token")));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-NewPassword")));
    }
}