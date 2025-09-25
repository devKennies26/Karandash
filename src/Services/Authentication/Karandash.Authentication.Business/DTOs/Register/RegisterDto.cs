using FluentValidation;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Register;

public class RegisterDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

    public UserRole UserRole { get; set; }

    public bool HasAcceptedPolicy { get; set; }
}

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Name")))
            .MaximumLength(100)
            .WithMessage(MessageHelper.GetMessage("FieldMaxLength", MessageHelper.GetMessage("FieldName-Name"), 100));

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Surname")))
            .MaximumLength(100)
            .WithMessage(MessageHelper.GetMessage("FieldMaxLength", MessageHelper.GetMessage("FieldName-Surname"),
                100));

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

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(MessageHelper.GetMessage("FieldRequired",
                MessageHelper.GetMessage("FieldName-ConfirmPassword")))
            .Equal(x => x.Password).WithMessage(MessageHelper.GetMessage("PasswordsDoNotMatch"));

        RuleFor(x => x.UserRole)
            .IsInEnum()
            .WithMessage(MessageHelper.GetMessage("InvalidUserRole"));

        RuleFor(x => x.HasAcceptedPolicy)
            .Equal(true)
            .WithMessage(MessageHelper.GetMessage("TermsAndPolicyNotAccepted"));
    }
}