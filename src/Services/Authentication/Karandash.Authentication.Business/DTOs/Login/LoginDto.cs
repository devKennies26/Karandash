using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Login;

public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
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