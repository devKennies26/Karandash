using FluentValidation;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Filters.Pagination;
using Karandash.Shared.Utils.Methods;

namespace Karandash.Authentication.Business.DTOs.Users;

public class GetAllUsersFilterDto : IPaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public UserRole? UserRole { get; set; }

    public bool? IsVerified { get; set; }

    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class GetAllUsersFilterDtoValidator : AbstractValidator<GetAllUsersFilterDto>
{
    public GetAllUsersFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(_ => MessageHelper.GetMessage("InvalidPageNumber"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage(_ => MessageHelper.GetMessage("InvalidPageSize"));
    }
}