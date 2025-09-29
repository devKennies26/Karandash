namespace Karandash.Shared.Filters.Pagination;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int TotalCount { get; set; }
}