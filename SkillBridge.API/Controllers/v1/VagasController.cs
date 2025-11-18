using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Vagas;
using SkillBridge.API.Models;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/vagas")]
public class VagasController : ApiControllerBase
{
    private readonly IVagaService _vagaService;

    public VagasController(IVagaService vagaService, IHateoasLinkService hateoasLinkService)
        : base(hateoasLinkService)
    {
        _vagaService = vagaService;
    }

    [HttpGet(Name = ApiRoutes.Vagas.GetAll)]
    public async Task<ActionResult<PagedResponse<VagaResponse>>> GetAsync([FromQuery] PaginationQuery query)
    {
        var result = await _vagaService.GetAsync(query);
        var response = ToPagedResponse(result);
        return OkPaged(response);
    }

    [HttpGet("{id:guid}", Name = ApiRoutes.Vagas.GetById)]
    public async Task<ActionResult<VagaResponse>> GetByIdAsync(Guid id)
    {
        var vaga = await _vagaService.GetByIdAsync(id);
        if (vaga is null)
        {
            return NotFound();
        }

        return OkWithLinks(vaga);
    }

    [HttpPost(Name = ApiRoutes.Vagas.Create)]
    public async Task<ActionResult<VagaResponse>> CreateAsync([FromBody] CreateVagaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _vagaService.CreateAsync(request);
        return CreatedAtRoute(ApiRoutes.Vagas.GetById, new { id = created.Id, version = "1.0" }, WithLinks(created));
    }

    [HttpPut("{id:guid}", Name = ApiRoutes.Vagas.Update)]
    public async Task<ActionResult<VagaResponse>> UpdateAsync(Guid id, [FromBody] UpdateVagaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _vagaService.UpdateAsync(id, request);
        if (updated is null)
        {
            return NotFound();
        }

        return OkWithLinks(updated);
    }

    [HttpDelete("{id:guid}", Name = ApiRoutes.Vagas.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var deleted = await _vagaService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private PagedResponse<VagaResponse> ToPagedResponse(PagedResult<VagaResponse> result)
    {
        return new PagedResponse<VagaResponse>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        };
    }
}
