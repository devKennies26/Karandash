using Karandash.Shared.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Karandash.Authentication.API.Controllers;

[Route("api/[controller]"), ApiController, Obsolete]
public class CurrentUserController(ICurrentUser currentUser) : ControllerBase
{
    private readonly ICurrentUser _currentUser = currentUser;

    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            Id = _currentUser.UserId,
            Guid = _currentUser.UserGuid,
            FullName = _currentUser.UserFullName,
            BaseUrl = _currentUser.BaseUrl,
            Role = _currentUser.UserRole,
            Language = _currentUser.LanguageCode.ToString()
        });
    }
}