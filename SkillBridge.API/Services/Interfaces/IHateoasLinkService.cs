using SkillBridge.API.DTOs;

namespace SkillBridge.API.Services.Interfaces;

public interface IHateoasLinkService
{
    T AddLinks<T>(T resource) where T : ResourceResponse;
    PagedResponse<T> AddLinks<T>(PagedResponse<T> pagedResponse) where T : ResourceResponse;
}
