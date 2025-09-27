using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Business.Exceptions;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Enums.Auth;
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

        if (registerDto.UserRole is < UserRole.Guest or > UserRole.Other)
            return (false, MessageHelper.GetMessage("UserRoleAreNotAllowed"));

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

    public async Task GenerateAndSendPasswordResetTokenAsync(string email)
    {
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new UserFriendlyBusinessException("UserNotFoundForPasswordReset");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        PasswordToken? existsPasswordToken =
            await _dbContext.PasswordTokens.FirstOrDefaultAsync(pt => pt.UserId == user.Id);
        if (existsPasswordToken is not null)
        {
            _dbContext.PasswordTokens.Remove(existsPasswordToken);
            await _dbContext.SaveChangesAsync();
        }

        DateTime expiresDate = DateTime.UtcNow.AddHours(1);
        string token = _tokenHandler.GeneratePasswordResetToken(user, expiresDate);
        PasswordToken passwordToken = new PasswordToken()
        {
            Value = token,
            UserId = user.Id,
            InsertedAt = DateTime.UtcNow,
            ExpiresDate = expiresDate
        };

        await _dbContext.PasswordTokens.AddAsync(passwordToken);
        await _dbContext.SaveChangesAsync();

        _emailService.SendPasswordResetEmail(user.Email, $"{user.FirstName} {user.LastName}", token);
    }

    public async Task ConfirmPasswordResetAsync(PasswordResetDto passwordResetDto)
    {
        ValidatePassword(passwordResetDto.NewPassword);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        var jwToken = handler.ReadJwtToken(passwordResetDto.Token);

        string? email = jwToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new UserFriendlyBusinessException("UserNotFoundForPasswordReset");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        PasswordToken? passwordToken = await _dbContext.PasswordTokens.FirstOrDefaultAsync(pt =>
                                           pt.Value == passwordResetDto.Token && pt.UserId == user.Id &&
                                           pt.ExpiresDate >= DateTime.UtcNow)
                                       ?? throw new UserFriendlyBusinessException("InvalidPasswordToken");

        byte[] salt = _passwordHasher.GenerateSalt();

        user.PasswordHash = _passwordHasher.Hash(passwordResetDto.NewPassword, salt);
        user.PasswordSalt = salt;
        _dbContext.PasswordTokens.Remove(passwordToken);
        await _dbContext.SaveChangesAsync();
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