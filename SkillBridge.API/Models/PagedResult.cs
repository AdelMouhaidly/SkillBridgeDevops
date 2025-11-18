namespace SkillBridge.API.Models;

public class PagedResult<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required long TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
