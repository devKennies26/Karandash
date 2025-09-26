using Karandash.Authentication.Business.DTOs.Register;
using Karandash.Authentication.Business.Exceptions;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Authentication;

public class AuthenticationService(
    AuthenticationDbContext dbContext,
    PasswordHasher passwordHasher,
    EmailService emailService)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;

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

        /* TODO: mail göndərildi, uyğun olan event'i göndərmək lazımdır (lazımlı məlumatlarla) */
        bool result = await _dbContext.SaveChangesAsync() > 0;
        if (result)
            _emailService.SendRegistrationEmail(
                user.Email,
                $"{user.FirstName} {user.LastName}"
            );

        return result
            ? (true, MessageHelper.GetMessage("UserRegisteredSuccessfully"))
            : (false, MessageHelper.GetMessage("UserRegistrationFailed"));
    }

    private async Task CheckEmailExistsAsync(string email)
    {
        User? existingUser = await _dbContext.Users
            .IgnoreQueryFilters() /* NOTE: ola bilsin ki, soft delete işləmi üçün sistemdə sonraki zamanlarda query filter yazılsın, həmən səbəbdən onları ignore'a atırıq. */
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser is null)
            return;

        throw existingUser.IsDeleted switch
        {
            false => new UserFriendlyBusinessException("EmailAlreadyExists"),
            true => new UserFriendlyBusinessException("EmailBelongsToDeletedUser")
        };
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