using FluentValidation;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Auth;

public class UpdatePasswordDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
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