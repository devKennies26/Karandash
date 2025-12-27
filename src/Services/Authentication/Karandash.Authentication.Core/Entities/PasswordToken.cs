using System.ComponentModel.DataAnnotations.Schema;
using Karandash.Authentication.Core.Entities.Common;

namespace Karandash.Authentication.Core.Entities;

public class PasswordToken : BaseEntity
{
    public string Value { get; set; }
    public DateTime ExpiresDate { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    [NotMapped] public override DateTime? RemovedAt { get; set; }
}