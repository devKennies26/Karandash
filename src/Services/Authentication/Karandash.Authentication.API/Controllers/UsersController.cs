using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Business.Services.Users;
using Karandash.Shared.Attributes;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Filters.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karandash.Authentication.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    /// <summary>
    /// Retrieves a paginated list of users with optional filtering.
    /// </summary>
    /// This endpoint allows administrators to retrieve users from the system.
    /// Filters are optional and can be combined:
    /// - <c>UserRole</c>: Filter by specific user role.
    /// - <c>IsVerified</c>: Filter by verification status.
    /// - <c>FullName</c>: Search by full name (first + last name).
    /// - <c>Email</c>: Search by email.
    [AuthorizeRole(UserRole.Admin, UserRole.Moderator)]
    [HttpGet("[action]")]
    public async Task<ActionResult<PagedResponse<GetAllUsersDto>>> GetAllUsers([FromQuery] GetAllUsersFilterDto filter)
    {
        PagedResponse<GetAllUsersDto> result = await _userService.GetAllUsersAsync(filter);
        return Ok(result);
    }
}