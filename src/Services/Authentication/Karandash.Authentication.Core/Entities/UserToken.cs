using System.ComponentModel.DataAnnotations.Schema;
using Karandash.Authentication.Core.Entities.Common;
using Karandash.Authentication.Core.Enums;

namespace Karandash.Authentication.Core.Entities;

public class UserToken : BaseEntity
{
    public string Value { get; set; }
    public DateTime ExpiresDate { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public TokenType TokenType { get; set; }

    [NotMapped] public override DateTime? RemovedAt { get; set; }
}