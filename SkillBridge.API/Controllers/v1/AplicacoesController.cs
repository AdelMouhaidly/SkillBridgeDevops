using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Aplicacoes;
using SkillBridge.API.Models;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/aplicacoes")]
public class AplicacoesController : ApiControllerBase
{
    private readonly IAplicacaoService _aplicacaoService;

    public AplicacoesController(IAplicacaoService aplicacaoService, IHateoasLinkService hateoasLinkService)
        : base(hateoasLinkService)
    {
        _aplicacaoService = aplicacaoService;
    }

    [HttpGet(Name = ApiRoutes.Aplicacoes.GetAll)]
    public async Task<ActionResult<PagedResponse<AplicacaoResponse>>> GetAsync([FromQuery] PaginationQuery query)
    {
        var result = await _aplicacaoService.GetAsync(query);
        var response = ToPagedResponse(result);
        return OkPaged(response);
    }

    [HttpGet("{id:guid}", Name = ApiRoutes.Aplicacoes.GetById)]
    public async Task<ActionResult<AplicacaoResponse>> GetByIdAsync(Guid id)
    {
        var aplicacao = await _aplicacaoService.GetByIdAsync(id);
        if (aplicacao is null)
        {
            return NotFound();
        }

        return OkWithLinks(aplicacao);
    }

    [HttpGet("usuario/{usuarioId:guid}", Name = ApiRoutes.Aplicacoes.GetByUsuario)]
    public async Task<ActionResult<IEnumerable<AplicacaoResponse>>> GetByUsuarioAsync(Guid usuarioId)
    {
        var aplicacoes = await _aplicacaoService.GetByUsuarioAsync(usuarioId);
        var enriched = aplicacoes.Select(WithLinks).ToList();
        return Ok(enriched);
    }

    [HttpGet("vaga/{vagaId:guid}", Name = ApiRoutes.Aplicacoes.GetByVaga)]
    public async Task<ActionResult<IEnumerable<AplicacaoResponse>>> GetByVagaAsync(Guid vagaId)
    {
        var aplicacoes = await _aplicacaoService.GetByVagaAsync(vagaId);
        var enriched = aplicacoes.Select(WithLinks).ToList();
        return Ok(enriched);
    }

    [HttpPost(Name = ApiRoutes.Aplicacoes.Create)]
    public async Task<ActionResult<AplicacaoResponse>> CreateAsync([FromBody] CreateAplicacaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _aplicacaoService.CreateAsync(request);
            return CreatedAtRoute(ApiRoutes.Aplicacoes.GetById, new { id = created.Id, version = "1.0" }, WithLinks(created));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}", Name = ApiRoutes.Aplicacoes.Update)]
    public async Task<ActionResult<AplicacaoResponse>> UpdateAsync(Guid id, [FromBody] UpdateAplicacaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _aplicacaoService.UpdateAsync(id, request);
        if (updated is null)
        {
            return NotFound();
        }

        return OkWithLinks(updated);
    }

    [HttpDelete("{id:guid}", Name = ApiRoutes.Aplicacoes.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var deleted = await _aplicacaoService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private PagedResponse<AplicacaoResponse> ToPagedResponse(PagedResult<AplicacaoResponse> result)
    {
        return new PagedResponse<AplicacaoResponse>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        };
    }
}
