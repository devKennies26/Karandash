using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Business.Exceptions;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Utils;
using Karandash.Shared.Utils.Infrastructure;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Authentication;

public class AuthenticationService(
    AuthenticationDbContext dbContext,
    PasswordHasher passwordHasher,
    EmailService emailService,
    TokenHandler tokenHandler,
    ICurrentUser currentUser)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;
    private readonly TokenHandler _tokenHandler = tokenHandler;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<(bool result, string message)> RegisterAsync(RegisterDto registerDto, bool isAdminAction)
    {
        if (!registerDto.HasAcceptedPolicy)
            throw new PolicyException();

        switch (isAdminAction)
        {
            case false when IsSystemSideRole(registerDto.UserRole):
                return (false, MessageHelper.GetMessage("UserRoleAreNotAllowed"));
            case true when registerDto.UserRole == UserRole.Admin:
                return (false, MessageHelper.GetMessage("AdminCannotBeRegistered"));
        }

        await CheckEmailExistsAsync(registerDto.Email);
        ValidatePassword(registerDto.Password);

        byte[] salt = _passwordHasher.GenerateSalt();

        User user = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            /*PendingEmail = null,*/
            IsVerified =
                isAdminAction, /* NOTE: [isAdminAction] Əgər yuxarıdakilar nəzərə alınarsa, o zaman buradaki şərt də admin side role olub-olmaması ilə üst-üstə düşür!  */
            /*RefreshToken = null,*/
            /*RefreshTokenExpireDate = null,*/
            UserRole = registerDto.UserRole,
            InsertedAt = DateTime.UtcNow,
            /*UpdatedAt = null,*/
            /*RemovedAt = null,*/
            /*IsDeleted = false,*/
            PasswordHash = _passwordHasher.Hash(registerDto.Password, salt),
            PasswordSalt = salt,
        };
        await _dbContext.Users.AddAsync(user);

        await AddOutboxEventAsync(user);

        bool result = await _dbContext.SaveChangesAsync() > 0;
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

        user.UpdatedAt = DateTime.UtcNow;
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

    public async Task<TokenResponseDto> LoginByRefreshTokenAsync(string? refreshToken)
    {
        refreshToken = refreshToken?.Trim();
        if (string.IsNullOrEmpty(refreshToken))
            throw new UserFriendlyBusinessException("InvalidRefreshToken");

        User user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                    ?? throw new UserFriendlyBusinessException("InvalidRefreshToken");

        if (user.RefreshTokenExpireDate < DateTime.UtcNow)
            throw new UserFriendlyBusinessException("RefreshTokenExpired");

        string accessToken = _tokenHandler.GenerateAccessToken(user, expireMinutes: 60);
        RefreshToken
            newRefreshToken = _tokenHandler.GenerateRefreshToken(accessToken, minutes: 10080); // 7 gün (tövsiyə olunur)

        user.UpdatedAt = DateTime.UtcNow;
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

    public async Task GenerateAndSendPasswordResetTokenAsync(string? email)
    {
        email = email?.Trim();
        if (string.IsNullOrEmpty(email))
            throw new UserFriendlyBusinessException("InvalidEmail", MessageHelper.GetMessage("FieldName-Email"));

        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new UserFriendlyBusinessException("UserNotFoundForPasswordReset");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        PasswordToken? passwordToken =
            await _dbContext.PasswordTokens.FirstOrDefaultAsync(pt => pt.UserId == user.Id);

        DateTime expiresDate = DateTime.UtcNow.AddHours(1);
        string token = _tokenHandler.GeneratePasswordResetToken(user, expiresDate);

        if (passwordToken is null)
        {
            passwordToken = new PasswordToken
            {
                UserId = user.Id,
                Value = token,
                ExpiresDate = expiresDate,
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = null /* NOTE: ilk dəfə şifrəni dəyişmək istədikdə update at dəyəri null olmalıdır! */
            };

            await _dbContext.PasswordTokens.AddAsync(passwordToken);
        }
        else
        {
            passwordToken.Value = token;
            passwordToken.ExpiresDate = expiresDate;
            passwordToken.UpdatedAt = DateTime.UtcNow;

            _dbContext.PasswordTokens.Update(passwordToken);
        }

        user.UpdatedAt = DateTime.UtcNow;
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
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.PasswordTokens.Remove(passwordToken);
        await _dbContext.SaveChangesAsync();
    }

    private static bool IsSystemSideRole(UserRole role) =>
        role is UserRole.Admin or UserRole.Moderator or UserRole.ContentCreator;

    private async Task CheckEmailExistsAsync(string email)
    {
        User? existingUser = await _dbContext.Users
            .IgnoreQueryFilters() /* NOTE: Ola bilsin ki, soft delete işləmi üçün sistemdə sonraki zamanlarda query filter yazılsın, və həmən səbəbdən onları ignore'a atırıq ki, son istifadəçiyə daha da user-friendly xəta mesajı verə bilək. */
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser is null)
            return;

        throw existingUser.IsDeleted switch
        {
            false => new UserFriendlyBusinessException("EmailAlreadyExists"),
            true => new UserFriendlyBusinessException("EmailBelongsToDeletedUser")
        };
    }

    public void ValidatePassword(string password)
    {
        if (password.Length < 8 ||
            password.Length > 64 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsNumber) ||
            !password.Any(char.IsPunctuation))
            throw new UserFriendlyBusinessException("PasswordInvalid");
    }

    private async Task AddOutboxEventAsync(User user)
    {
        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Type = "UserRegisteredEvent",
            Payload = JsonSerializer.Serialize(new
            {
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}"
            }),
            RetryCount = 0
        };

        await _dbContext.OutboxEvents.AddAsync(outboxEvent);
    }
}