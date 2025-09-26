using Karandash.Authentication.Business.DTOs.Login;
using Karandash.Authentication.Business.DTOs.Register;
using Karandash.Authentication.Business.Services.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Karandash.Authentication.API.Controllers;

[Route("api/[controller]"), ApiController]
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
    /// - <c>Guest</c> = 3
    /// - <c>Student</c> = 4
    /// - <c>Alumni</c> = 5 — Məzunlar üçün
    /// - <c>Parent</c> = 6
    /// - <c>Mentor</c> = 7 — Mentorluq edənlər üçün
    /// - <c>Teacher</c> = 8 — Məktəb müəllimləri (ibtidai, orta, lisey)
    /// - <c>TeacherStaff</c> = 9 — Məktəb administrativ və digər personal
    /// - <c>Lecturer</c> = 10 — Universitet müəllimləri üçün
    /// - <c>Professor</c> = 11 — Universitet professorları üçün
    /// - <c>UniversityStaff</c> = 12 — Universitet administrativ və digər personal
    /// - <c>Researcher</c> = 13 — Elm və tədqiqat fəaliyyəti aparanlar üçün
    /// - <c>Other</c> = 14
    /// </remarks>
    /// <param name="registerDto">Registration data for the new user</param>
    /// <returns>
    /// Returns 200 OK with success message if registration succeeds, 
    /// or 500 Internal Server Error with failure message if registration fails.
    /// </returns>
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        (bool result, string message) = await _authenticationService.RegisterAsync(registerDto);

        return !result
            ? StatusCode(StatusCodes.Status500InternalServerError, new { Message = message })
            : Ok(new { Message = message });
    }

    /// <summary>
    /// Authenticates a user and returns a token.
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        return Ok(await _authenticationService.LoginAsync(loginDto));
    }
}