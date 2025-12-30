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
[Route("api/[controller]/[action]")]
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
    [HttpGet]
    public async Task<ActionResult<PagedResponse<GetAllUsersDto>>> GetAllUsers([FromQuery] GetAllUsersFilterDto filter)
    {
        PagedResponse<GetAllUsersDto> result = await _userService.GetAllUsersAsync(filter);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto updatePasswordDto)
    {
        (bool result, string message) = await _userService.UpdatePasswordAsync(updatePasswordDto);

        return !result
            ? StatusCode(StatusCodes.Status500InternalServerError, new { Message = message })
            : Ok(new { Message = message });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleDto changeUserRoleDto)
    {
        (bool result, string message) =
            await _userService.ChangeUserRoleAsync(changeUserRoleDto.TargetUserId, changeUserRoleDto.NewRole);

        return !result
            ? StatusCode(StatusCodes.Status500InternalServerError, new { Message = message })
            : Ok(new { Message = message });
    }
}