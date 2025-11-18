namespace SkillBridge.API.DTOs;

public abstract record ResourceResponse
{
    public IList<LinkDto> Links { get; init; } = new List<LinkDto>();
}

