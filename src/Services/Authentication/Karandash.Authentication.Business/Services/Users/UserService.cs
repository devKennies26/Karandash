using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.Core.Enums;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Filters.Pagination;
using Karandash.Shared.Policies.Role;
using Karandash.Shared.Utils;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Users;

public class UserService(
    AuthenticationDbContext dbContext,
    ICurrentUser currentUser,
    AuthenticationService authenticationService,
    PasswordHasher passwordHasher,
    EmailService emailService,
    TokenHandler tokenHandler)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;
    private readonly TokenHandler _tokenHandler = tokenHandler;

    public async Task<PagedResponse<GetAllUsersDto>> GetAllUsersAsync(GetAllUsersFilterDto filter)
    {
        IQueryable<User> query = _dbContext.Users
            .IgnoreQueryFilters()
            .AsQueryable()
            .AsNoTracking();

        query = ApplyFilters(query, filter);

        int totalCount = await query.CountAsync();
        List<GetAllUsersDto> users = await query
            .OrderBy(u => u.InsertedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new GetAllUsersDto
            {
                Id = u.Id,
                InsertedAt = u.InsertedAt,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                IsVerified = u.IsVerified,
                UserRole = u.UserRole
            })
            .ToListAsync();

        /* NOTE: admin tərəf üçün çağırıldığı üçün default aşağıdaki dəyərləri ilə gəlməsi yetərlidir, lazım olarsa, daha sonradan burası dəyişdirilə bilər. */
        return new PagedResponse<GetAllUsersDto>
        {
            Data = users,
            TotalCount = totalCount
        };
    }

    public async Task<(bool result, string message)> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
    {
        /*_authenticationService.ValidatePassword(updatePasswordDto.OldPassword);*/
        /* NOTE: köhnə şifrənin minimum 8 rəqəmli olduğunu bildikdən sonrası çox da önəmli deyil ki, biz burada onu validate edək. Sadəcə yenisinin formatını yoxladıqdan sonra aşağıda eyniliyini də yoxlayacayıq. */
        _authenticationService.ValidatePassword(updatePasswordDto.NewPassword!);

        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == _currentUser.UserGuid).FirstOrDefaultAsync();

        if (user is null)
            throw new UserFriendlyBusinessException("UserNotFoundForUpdatePassword");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        if (!_passwordHasher.Verify(updatePasswordDto.OldPassword, user.PasswordHash, user.PasswordSalt))
            throw new UserFriendlyBusinessException("OldPasswordIncorrect"); /* Köhnə parolu yoxlayırıq */
        if (_passwordHasher.Verify(updatePasswordDto.NewPassword!, user.PasswordHash, user.PasswordSalt))
            throw
                new UserFriendlyBusinessException(
                    "NewPasswordCannotBeSameAsOld"); /* Köhnə parol yenisi ilə eyni olarsa, extra database işləmi görməkdənsə istəyi geri çeviririk! */

        byte[] newSalt = _passwordHasher.GenerateSalt();
        string newHashedPassword = _passwordHasher.Hash(updatePasswordDto.NewPassword!, newSalt);

        user.PasswordSalt = newSalt;
        user.PasswordHash = newHashedPassword;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);

        bool result = await _dbContext.SaveChangesAsync() > 0;
        if (result)
            _emailService.SendPasswordChangedEmail(
                user.Email,
                $"{user.FirstName} {user.LastName}"
            );

        return result
            ? (true, MessageHelper.GetMessage("PasswordChangedSuccessfully"))
            : (false, MessageHelper.GetMessage("PasswordChangeFailed"));
    }

    public async Task<(bool result, string message)> ChangeUserRoleAsync(
        Guid targetUserId,
        UserRole newRole)
    {
        User actor = await _dbContext.Users
                         .IgnoreQueryFilters()
                         .FirstOrDefaultAsync(u => u.Id == _currentUser.UserGuid)
                     ?? throw new UserFriendlyBusinessException("ActorUserNotFound");

        User target = await _dbContext.Users
                          .IgnoreQueryFilters()
                          .FirstOrDefaultAsync(u => u.Id == targetUserId)
                      ?? throw new UserFriendlyBusinessException("TargetUserNotFound");

        if (target.UserRole == newRole)
            return (true, MessageHelper.GetMessage("RoleChangedSuccessfully"));

        bool isSelfService = actor.Id == target.Id;

        if (!RoleChangePolicy.CanChangeRole(actor.UserRole, target.UserRole, newRole, isSelfService))
            throw new UserFriendlyBusinessException("RoleChangeNotAllowed");

        target.UserRole = newRole;
        target.UpdatedAt = DateTime.UtcNow;

        _dbContext.Users.Update(target);
        bool success = await _dbContext.SaveChangesAsync() > 0;

        string message = success
            ? MessageHelper.GetMessage("RoleChangedSuccessfully")
            : MessageHelper.GetMessage("RoleChangeFailed");

        return (success, message);
    }

    public async Task GenerateAndSendEmailVerificationTokenAsync()
    {
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == _currentUser.UserGuid).FirstOrDefaultAsync();
        if (user is null || user.IsDeleted)
            throw new UserFriendlyBusinessException("Unauthorized");

        if (user.IsVerified)
            throw new UserFriendlyBusinessException("EmailAlreadyVerified");

        UserToken? userToken =
            await _dbContext.UserTokens.FirstOrDefaultAsync(ut =>
                ut.UserId == user.Id && ut.TokenType == TokenType.EmailVerification);

        DateTime expiresDate = DateTime.UtcNow.AddHours(24);
        string token = _tokenHandler.GenerateEmailVerificationToken(user, expiresDate);

        if (userToken is null)
        {
            userToken = new UserToken()
            {
                UserId = user.Id,
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = null, /* NOTE: ilk dəfə şifrəni dəyişmək istədikdə update at dəyəri null olmalıdır! */
                Value = token,
                ExpiresDate = expiresDate,
                TokenType = TokenType.EmailVerification
            };

            await _dbContext.UserTokens.AddAsync(userToken);
        }
        else
        {
            userToken.Value = token;
            userToken.ExpiresDate = expiresDate;
            userToken.UpdatedAt = DateTime.UtcNow;

            _dbContext.UserTokens.Update(userToken);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _emailService.SendEmailVerificationEmail(
            user.Email,
            $"{user.FirstName} {user.LastName}",
            token);
    }

    public async Task ConfirmEmailVerificationAsync(string? token)
    {
        token = token?.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new UserFriendlyBusinessException("SessionExpiredRetryAction");

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwToken = handler.ReadJwtToken(token);

        string? email = jwToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || user.IsDeleted)
            throw new UserFriendlyBusinessException("EmailVerificationNotAllowed");

        UserToken? userToken = await _dbContext.UserTokens.FirstOrDefaultAsync(ut =>
                                   ut.Value == token &&
                                   ut.UserId == user.Id &&
                                   ut.TokenType == TokenType.EmailVerification &&
                                   ut.ExpiresDate >= DateTime.UtcNow)
                               ?? throw new UserFriendlyBusinessException("EmailVerificationNotAllowed");

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.UserTokens.Remove(userToken);
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<User> ApplyFilters(IQueryable<User> query, GetAllUsersFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.FullName))
            query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(filter.FullName));
        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        if (filter.UserRole.HasValue)
            query = query.Where(u => u.UserRole == filter.UserRole.Value);

        if (!filter.IncludeSystemRoles)
            query = query.Where(u => !SystemRoles.Contains(u.UserRole));

        if (!filter.IncludeDeleted)
            query = query.Where(u =>
                !u.IsDeleted); /* Əgər IncludeDeleted false'dursa, yalnız deleted olmayanları gətir */

        if (filter.IsVerified.HasValue)
            query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

        return query;
    }

    private static readonly UserRole[] SystemRoles =
    [
        UserRole.Admin,
        UserRole.Moderator,
        UserRole.ContentCreator
    ];
}