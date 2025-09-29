using Karandash.Authentication.Business.DTOs.Users;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared.Filters.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.Business.Services.Users;

public class UserService(AuthenticationDbContext dbContext)
{
    private readonly AuthenticationDbContext _dbContext = dbContext;

    public async Task<PagedResponse<GetAllUsersDto>> GetAllUsersAsync(GetAllUsersFilterDto filter)
    {
        IQueryable<User> query = _dbContext.Users
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
    
    private IQueryable<User> ApplyFilters(IQueryable<User> query, GetAllUsersFilterDto filter)
    {
        if (filter.UserRole.HasValue)
            query = query.Where(u => u.UserRole == filter.UserRole.Value);

        if (filter.IsVerified.HasValue)
            query = query.Where(u => u.IsVerified == filter.IsVerified.Value);

        if (!string.IsNullOrWhiteSpace(filter.FullName))
            query = query.Where(u => (u.FirstName + " " + u.LastName).Contains(filter.FullName));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        return query;
    }
}