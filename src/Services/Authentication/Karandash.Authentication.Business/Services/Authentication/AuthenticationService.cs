using Karandash.Authentication.Business.DTOs.Register;
using Karandash.Authentication.Business.Exceptions;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Authentication;

public class AuthenticationService(AuthenticationDbContext dbContext, PasswordHasher passwordHasher)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    public async Task<(bool result, string message)> RegisterAsync(RegisterDto registerDto)
    {
        if (!registerDto.HasAcceptedPolicy)
            throw new PolicyException();

        await CheckEmailExistsAsync(registerDto.Email.Trim());
        ValidatePassword(registerDto.Password);

        User user = new User()
        {
            Id = Guid.NewGuid(),
            InsertedAt = DateTime.UtcNow,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Password = _passwordHasher.Hash(registerDto.Password),
            Email = registerDto.Email,
            IsVerified = false,
            UserRole = registerDto.UserRole
        };

        await _dbContext.Users.AddAsync(user);

        /* TODO: Aşağıdaki şərt yoxlamasına görə lazım olarsa, mail göndərmək və event göndərmək gərəkəcək! */
        return await _dbContext.SaveChangesAsync() > 0
            ? (true, MessageHelper.GetMessage("UserRegisteredSuccessfully"))
            : (false, MessageHelper.GetMessage("UserRegistrationFailed"));
    }

    private async Task CheckEmailExistsAsync(string email)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            throw new UserFriendlyBusinessException("EmailAlreadyExists");
    }

    private void ValidatePassword(string password)
    {
        if (password.Length < 8 ||
            password.Length > 64 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsNumber) ||
            !password.Any(char.IsPunctuation))
            throw new UserFriendlyBusinessException("PasswordInvalid");
    }
}