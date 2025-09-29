using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Exceptions;
using Karandash.Shared.Filters.Pagination;
using Karandash.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Users;

public class UserService(
    AuthenticationDbContext dbContext,
    ICurrentUser currentUser,
    AuthenticationService authenticationService,
    PasswordHasher passwordHasher)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly AuthenticationService _authenticationService = authenticationService;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

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

    /* TODO: Şifrə dəyişildikdən sonra uyğun şəkildə mail göndərilməlidir! */
    public async Task UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
    {
        _authenticationService.ValidatePassword(updatePasswordDto.OldPassword);
        _authenticationService.ValidatePassword(updatePasswordDto.NewPassword);

        User? user = await _dbContext.Users
            .IgnoreQueryFilters()
            .Where(u => u.Id == _currentUser.UserGuid).FirstOrDefaultAsync();

        if (user is null)
            throw new UserFriendlyBusinessException("InvalidEmailOrPassword");
        if (user.IsDeleted)
            throw new UserFriendlyBusinessException("AccountDeleted");

        if (!_passwordHasher.Verify(updatePasswordDto.OldPassword, user.PasswordHash, user.PasswordSalt))
            throw new UserFriendlyBusinessException("OldPasswordIncorrect"); /* Köhnə parolu yoxlayırıq */
        if (_passwordHasher.Verify(updatePasswordDto.NewPassword, user.PasswordHash, user.PasswordSalt))
            throw
                new UserFriendlyBusinessException(
                    "NewPasswordCannotBeSameAsOld"); /* Köhnə parol yenisi ilə eyni olarsa, extra database işləmi görməkdənsə istəyi geri çeviririk! */

        byte[] newSalt = _passwordHasher.GenerateSalt();
        string newHashedPassword = _passwordHasher.Hash(updatePasswordDto.NewPassword, newSalt);

        user.PasswordSalt = newSalt;
        user.PasswordHash = newHashedPassword;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<User> ApplyFilters(IQueryable<User> query, GetAllUsersFilterDto filter)
    {
        if (filter.UserRole.HasValue)
            query = query.Where(u => u.UserRole == filter.UserRole.Value);

        if (filter.IsVerified.HasValue)
            query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

        if (!filter.IncludeDeleted)
            query = query.Where(u =>
                !u.IsDeleted); /* Əgər IncludeDeleted false'dursa, yalnız deleted olmayanları gətir */

        if (!string.IsNullOrWhiteSpace(filter.FullName))
            query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(filter.FullName));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        return query;
    }
}