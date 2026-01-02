using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Business.Services.Users;
using Karandash.Shared.Attributes;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Filters.Pagination;
using Karandash.Shared.Utils.Methods;
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

    /// <summary>
    /// Generates and sends email verification token to current user.
    /// </summary>
    [HttpPost("send-email-verification")]
    public async Task<IActionResult> SendEmailVerificationToken()
    {
        await _userService.GenerateAndSendEmailVerificationTokenAsync();
        return Accepted(new
        {
            Message = MessageHelper.GetMessage("EmailVerification-EmailMessage")
        });
    }

    /// <summary>
    /// Confirms email verification using the token from email link.
    /// </summary>
    /// <param name="token">Token from verification email link</param>
    [HttpPost("confirm-email-verification")]
    public async Task<IActionResult> ConfirmEmailVerification([FromQuery] string token)
    {
        await _userService.ConfirmEmailVerificationAsync(token);
        return Ok(new
        {
            Message = MessageHelper.GetMessage("EmailVerification-Success")
        });
    }
}