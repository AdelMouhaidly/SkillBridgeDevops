using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.Models;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers.v2;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/usuarios")]
public class UsuariosController : ApiControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService, IHateoasLinkService hateoasLinkService)
        : base(hateoasLinkService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet(Name = ApiRoutes.Usuarios.Summary)]
    public async Task<ActionResult<PagedResponse<UsuarioSummaryResponse>>> GetResumoAsync([FromQuery] PaginationQuery query)
    {
        var result = await _usuarioService.GetSummariesAsync(query);
        var response = new PagedResponse<UsuarioSummaryResponse>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        };

        return OkPaged(response);
    }
}
