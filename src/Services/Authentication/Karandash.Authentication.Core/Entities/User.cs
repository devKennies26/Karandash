using Karandash.Authentication.Core.Entities.Common;
using Karandash.Shared.Enums.Auth;

namespace Karandash.Authentication.Core.Entities;

/*
 * TODO: Login və register əməliyyatları üçün log tutmaq lazımdır
 * TODO: Şifrə bərpası əməliyyatı üçün lazımi field'lər əlavə olunmalıdır
 */
public class User : BaseEntity, ISoftDeletable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public string Email { get; set; } /* Bu mail login zamanı istifadə olunacaq mail'dir və prioritet təşkil edir */

    public string?
        PendingEmail
    {
        get;
        set;
    } /* Bu mail isə istifadəçi mail'ini dəyişmək istədiyi zaman istifadə olunacaq mail'dir */

    public bool IsVerified { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpireDate { get; set; }

    public UserRole UserRole { get; set; }

    public PasswordToken? PasswordToken { get; set; }

    public bool IsDeleted { get; set; }
}