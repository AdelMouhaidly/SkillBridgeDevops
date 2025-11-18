namespace SkillBridge.API.DTOs;

public class PagedResponse<T> where T : ResourceResponse
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public long TotalCount { get; init; }
    public IList<LinkDto> Links { get; init; } = new List<LinkDto>();
}

