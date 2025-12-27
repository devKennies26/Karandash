using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Auth;

public class LoginDto
{
    private string? _email;

    public string Email
    {
        get => _email!;
        set => _email = value?.Trim();
    }

    private string? _password;

    public string Password
    {
        get => _password!;
        set => _password = value?.Trim();
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Email")))
            .MaximumLength(255)
            .WithMessage(MessageHelper.GetMessage("FieldMaxLength", MessageHelper.GetMessage("FieldName-Email"), 255))
            .EmailAddress()
            .WithMessage(MessageHelper.GetMessage("InvalidEmailFormat"));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Password")))
            .MinimumLength(8)
            .WithMessage(MessageHelper.GetMessage("FieldMinLength", MessageHelper.GetMessage("FieldName-Password"), 8));
    }
}