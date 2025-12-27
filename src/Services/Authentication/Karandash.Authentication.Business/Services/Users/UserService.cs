using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Filters.Pagination;
using Karandash.Shared.Utils;
using Karandash.Shared.Utils.Methods;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Users;

public class UserService(
    AuthenticationDbContext dbContext,
    ICurrentUser currentUser,
    AuthenticationService authenticationService,
    PasswordHasher passwordHasher,
    EmailService emailService)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly EmailService _emailService = emailService;

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
            throw new UserFriendlyBusinessException("UserNotFound");
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

    public async Task<(bool result, string message)> DeactivateAccountAsync()
    {
        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == _currentUser.UserGuid).Include(user => user.PasswordToken).FirstOrDefaultAsync();

        if (user is null)
            throw
                new UserFriendlyBusinessException(
                    "UserNotFound");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        if (user.UserRole is UserRole.Admin or UserRole.Moderator or UserRole.ContentCreator)
            throw new UserFriendlyBusinessException("SystemRoleDeactivationNotAllowed");

        user.IsDeleted = true;

        /* NOTE: İstifadəçi deaktiv olandan sonra artıq refresh token dəyərindən istifadə edə bilməyəcək. */
        user.RefreshToken = null;
        user.RefreshTokenExpireDate = null;

        if (user.PasswordToken is not null)
        {
            _dbContext.PasswordTokens.Remove(user.PasswordToken);
            user.PasswordToken = null;
        }

        user.UpdatedAt = DateTime.UtcNow;
        user.RemovedAt = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        bool success = await _dbContext.SaveChangesAsync() > 0;

        if (!success) return (false, MessageHelper.GetMessage("AccountDeactivationFailed"));

        _emailService.SendAccountDeactivationEmail(
            user.Email,
            $"{user.FirstName} {user.LastName}"
        );
        return (true, MessageHelper.GetMessage("AccountDeactivatedSuccessfully"));
    }

    private static readonly UserRole[] SystemRoles =
    [
        UserRole.Admin,
        UserRole.Moderator,
        UserRole.ContentCreator
    ];

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
}