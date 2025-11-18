using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.DTOs;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly IHateoasLinkService _hateoasLinkService;

    protected ApiControllerBase(IHateoasLinkService hateoasLinkService)
    {
        _hateoasLinkService = hateoasLinkService;
    }

    protected T WithLinks<T>(T resource) where T : ResourceResponse
    {
        return _hateoasLinkService.AddLinks(resource);
    }

    protected PagedResponse<T> WithLinks<T>(PagedResponse<T> response) where T : ResourceResponse
    {
        return _hateoasLinkService.AddLinks(response);
    }

    protected ActionResult<T> OkWithLinks<T>(T resource) where T : ResourceResponse
    {
        var enriched = WithLinks(resource);
        return Ok(enriched);
    }

    protected ActionResult<PagedResponse<T>> OkPaged<T>(PagedResponse<T> response) where T : ResourceResponse
    {
        var enriched = WithLinks(response);
        return Ok(enriched);
    }
}
