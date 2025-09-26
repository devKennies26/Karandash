using Karandash.Authentication.Business.DTOs.Login;
using Karandash.Authentication.Business.DTOs.Register;
using Karandash.Authentication.Business.DTOs.Token;
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
    EmailService emailService,
    TokenHandler tokenHandler)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;
    private readonly TokenHandler _tokenHandler = tokenHandler;

    public async Task<(bool result, string message)> RegisterAsync(RegisterDto registerDto)
    {
        if (!registerDto.HasAcceptedPolicy)
            throw new PolicyException();

        await CheckEmailExistsAsync(registerDto.Email.Trim());
        ValidatePassword(registerDto.Password);

        byte[] salt = _passwordHasher.GenerateSalt();

        User user = new User()
        {
            Id = Guid.NewGuid(),
            InsertedAt = DateTime.UtcNow,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.Hash(registerDto.Password, salt),
            PasswordSalt = salt,
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

    public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
    {
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user is null)
            throw new UserFriendlyBusinessException("InvalidEmailOrPassword");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        if (!_passwordHasher.Verify(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            throw new UserFriendlyBusinessException("InvalidEmailOrPassword");

        string accessToken = _tokenHandler.GenerateAccessToken(user, expireMinutes: 60);
        RefreshToken
            refreshToken = _tokenHandler.GenerateRefreshToken(accessToken, minutes: 10080); // 7 gün (tövsiyə olunur)

        user.RefreshToken = refreshToken.TokenValue;
        user.RefreshTokenExpireDate = refreshToken.ExpiresAt;
        await _dbContext.SaveChangesAsync();

        return new TokenResponseDto()
        {
            UserId = user.Id.ToString(),
            FullName = $"{user.FirstName} {user.LastName}",
            RoleId = (byte)user.UserRole,
            AccessToken = accessToken,
            RefreshToken = refreshToken.TokenValue,
            ExpiresDate = refreshToken.ExpiresAt
        };
    }

    public async Task<TokenResponseDto> LoginByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new UserFriendlyBusinessException("InvalidRefreshToken");

        User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                    ?? throw new UserFriendlyBusinessException("InvalidRefreshToken");

        if (user.RefreshTokenExpireDate < DateTime.UtcNow)
            throw new UserFriendlyBusinessException("RefreshTokenExpired");

        string accessToken = _tokenHandler.GenerateAccessToken(user, expireMinutes: 60);
        RefreshToken
            newRefreshToken = _tokenHandler.GenerateRefreshToken(accessToken, minutes: 10080); // 7 gün (tövsiyə olunur)

        user.RefreshToken = newRefreshToken.TokenValue;
        user.RefreshTokenExpireDate = newRefreshToken.ExpiresAt;
        await _dbContext.SaveChangesAsync();

        return new TokenResponseDto()
        {
            UserId = user.Id.ToString(),
            FullName = $"{user.FirstName} {user.LastName}",
            RoleId = (byte)user.UserRole,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.TokenValue,
            ExpiresDate = newRefreshToken.ExpiresAt
        };
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