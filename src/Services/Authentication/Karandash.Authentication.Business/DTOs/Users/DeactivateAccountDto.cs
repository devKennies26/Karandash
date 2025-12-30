using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Users;

public class DeactivateAccountDto
{
    private string? _password;

    public string Password
    {
        get => _password!;
        set => _password = value?.Trim();
    }
}

public class DeactivateAccountDtoValidator : AbstractValidator<DeactivateAccountDto>
{
    public DeactivateAccountDtoValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(MessageHelper.GetMessage("FieldRequired", MessageHelper.GetMessage("FieldName-Password")))
            .MinimumLength(8)
            .WithMessage(MessageHelper.GetMessage("FieldMinLength", MessageHelper.GetMessage("FieldName-Password"), 8));
    }
}