using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.Models;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/usuarios")]
public class UsuariosController : ApiControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService, IHateoasLinkService hateoasLinkService)
        : base(hateoasLinkService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet(Name = ApiRoutes.Usuarios.GetAll)]
    public async Task<ActionResult<PagedResponse<UsuarioResponse>>> GetAsync([FromQuery] PaginationQuery query)
    {
        var result = await _usuarioService.GetAsync(query);
        var response = ToPagedResponse(result);
        return OkPaged(response);
    }

    [HttpGet("{id:guid}", Name = ApiRoutes.Usuarios.GetById)]
    public async Task<ActionResult<UsuarioResponse>> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        return OkWithLinks(usuario);
    }

    [HttpPost(Name = ApiRoutes.Usuarios.Create)]
    public async Task<ActionResult<UsuarioResponse>> CreateAsync([FromBody] CreateUsuarioRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _usuarioService.CreateAsync(request);
            return CreatedAtRoute(ApiRoutes.Usuarios.GetById, new { id = created.Id, version = "1.0" }, WithLinks(created));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}", Name = ApiRoutes.Usuarios.Update)]
    public async Task<ActionResult<UsuarioResponse>> UpdateAsync(Guid id, [FromBody] UpdateUsuarioRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updated = await _usuarioService.UpdateAsync(id, request);
            if (updated is null)
            {
                return NotFound();
            }

            return OkWithLinks(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}", Name = ApiRoutes.Usuarios.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var deleted = await _usuarioService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private PagedResponse<UsuarioResponse> ToPagedResponse(PagedResult<UsuarioResponse> result)
    {
        return new PagedResponse<UsuarioResponse>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        };
    }
}
