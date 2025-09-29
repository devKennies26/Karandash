namespace Karandash.Shared.Filters.Pagination;

public interface IPaginationFilter
{
    int PageNumber { get; set; }
    int PageSize { get; set; }
}