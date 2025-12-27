using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Shared.Attributes;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Utils.Methods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karandash.Authentication.API.Controllers;

[Route("api/[controller]/[action]"), ApiController]
public class AuthenticationController(AuthenticationService authenticationService) : ControllerBase
{
    private readonly AuthenticationService _authenticationService = authenticationService;

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <remarks>
    /// UserRole enum guide:
    /// - <c>Admin</c> = 0
    /// - <c>Moderator</c> = 1 — Şərhləri və forumu idarə edən
    /// - <c>ContentCreator</c> = 2 — Blog kontenti idarə edən
    /// - <c>Student</c> = 3 - Ya məktəb, ya da universitet şagirdləri/tələbələri üçün
    /// - <c>Alumni</c> = 4 — Məzunlar üçün
    /// - <c>Parent</c> = 5 - Valideynlər üçün
    /// - <c>Mentor</c> = 6 — Mentorluq edənlər üçün
    /// - <c>Teacher</c> = 7 — Əsasən, məktəb müəllimləri (ibtidai, orta, lisey) üçün
    /// - <c>TeacherStaff</c> = 8 — Məktəb administrativ və digər personal
    /// - <c>Lecturer</c> = 9 — Universitet müəllimləri üçün
    /// - <c>Professor</c> = 10 — Universitet professorları üçün
    /// - <c>UniversityStaff</c> = 11 — Universitet administrativ və digər personal (rektorluq, dekan, departament rəhbəri) heyəti üçün
    /// - <c>Researcher</c> = 12 — Elm və tədqiqat fəaliyyəti aparanlar üçün
    /// - <c>Other</c> = 13 - Sadalanan role'lardan heç birinə uyğun olmayanlar üçün
    /// </remarks>
    /// <param name="registerDto">Registration data for the new user</param>
    /// <returns>
    /// Returns 200 OK with success message if registration succeeds, 
    /// or 500 Internal Server Error with failure message if registration fails.
    /// </returns>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        (bool result, string message) = await _authenticationService.RegisterAsync(registerDto, isAdminAction: false);

        return !result
            ? StatusCode(StatusCodes.Status500InternalServerError, new { Message = message })
            : Ok(new { Message = message });
    }

    [AuthorizeRole(UserRole.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterByAdmin([FromBody] RegisterDto registerDto)
    {
        (bool result, string message) = await _authenticationService.RegisterAsync(registerDto, isAdminAction: true);

        return !result
            ? StatusCode(StatusCodes.Status500InternalServerError, new { Message = message })
            : Ok(new { Message = message });
    }

    /// <summary>
    /// Authenticates a user and returns a token.
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto) =>
        Ok(await _authenticationService.LoginAsync(loginDto));

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <param name="requestDto"></param>
    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto requestDto) =>
        Ok(await _authenticationService.LoginByRefreshTokenAsync(requestDto.RefreshToken));

    /// <summary>
    /// Generates and sends a password reset token to the specified email.
    /// </summary>
    /// <param name="email">The email of the user</param>
    [HttpPost]
    public async Task<IActionResult> SendResetToken([FromQuery] string email)
    {
        await _authenticationService.GenerateAndSendPasswordResetTokenAsync(email);
        return Accepted(new
        {
            Message = MessageHelper.GetMessage("PasswordReset-EmailMessage")
        });
    }

    /// <summary>
    /// Confirms password reset using the token sent to email.
    /// </summary>
    /// <param name="passwordResetDto">Password reset data including token and new password</param>
    [HttpPost]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] PasswordResetDto passwordResetDto)
    {
        await _authenticationService.ConfirmPasswordResetAsync(passwordResetDto);
        return Ok(new
        {
            Message = MessageHelper.GetMessage("PasswordReset-Success")
        });
    }
}