using Karandash.Shared.Enums.Auth;

namespace Karandash.Authentication.Business.DTOs.Users;

public class GetAllUsersDto
{
    public Guid Id { get; set; }
    public DateTime InsertedAt { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public bool IsVerified { get; set; }

    public UserRole UserRole { get; set; }

    public bool IsDeleted { get; set; }
}