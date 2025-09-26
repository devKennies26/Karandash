namespace Karandash.Authentication.Core.Entities.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}