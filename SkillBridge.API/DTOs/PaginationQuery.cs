namespace SkillBridge.API.DTOs;

public record PaginationQuery(int PageNumber = 1, int PageSize = 10, string? OrderBy = null, string SortDirection = "asc")
{
    public int PageNumber { get; init; } = PageNumber < 1 ? 1 : PageNumber;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 10 : PageSize;
    public string? OrderBy { get; init; } = string.IsNullOrWhiteSpace(OrderBy) ? null : OrderBy;
    public string SortDirection { get; init; } = string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
        ? "desc"
        : "asc";
}

