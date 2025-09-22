namespace Karandash.Authentication.Core.Entities.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    /* Lazımi field'lər gərəkən zaman üçün override və [NotMapped] edə bilərik! */
    public virtual DateTime InsertedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual DateTime? RemovedAt { get; set; }
}