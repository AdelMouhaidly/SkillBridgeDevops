using Microsoft.Extensions.Logging;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Vagas;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;
using SkillBridge.API.Services.Interfaces;
using SkillBridge.API.Logging;

namespace SkillBridge.API.Services;

public class VagaService : IVagaService
{
    private readonly IVagaRepository _vagaRepository;
    private readonly ILogger<VagaService> _logger;

    public VagaService(IVagaRepository vagaRepository, ILogger<VagaService> logger)
    {
        _vagaRepository = vagaRepository;
        _logger = logger;
    }

    public async Task<PagedResult<VagaResponse>> GetAsync(PaginationQuery query)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("VagaService.GetAsync");
        activity?.SetTag("pageNumber", query.PageNumber);
        activity?.SetTag("pageSize", query.PageSize);
        var (items, totalCount) = await _vagaRepository.GetPagedAsync(query);
        var responses = items.Select(MapToResponse).ToList();

        return new PagedResult<VagaResponse>
        {
            Items = responses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<VagaResponse?> GetByIdAsync(Guid id)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("VagaService.GetByIdAsync");
        activity?.SetTag("id", id);
        var vaga = await _vagaRepository.GetByIdAsync(id);
        return vaga is null ? null : MapToResponse(vaga);
    }

    public async Task<VagaResponse> CreateAsync(CreateVagaRequest request)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("VagaService.CreateAsync");
        activity?.SetTag("titulo", request.Titulo);
        activity?.SetTag("empresa", request.Empresa);
        var vaga = new Vaga
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            Empresa = request.Empresa.Trim(),
            Requisitos = request.Requisitos.Trim(),
            Salario = request.Salario,
            TipoContrato = request.TipoContrato.Trim()
        };

        await _vagaRepository.AddAsync(vaga);
        _logger.LogInformation("Vaga criada: {VagaId}", vaga.Id);
        return MapToResponse(vaga);
    }

    public async Task<VagaResponse?> UpdateAsync(Guid id, UpdateVagaRequest request)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("VagaService.UpdateAsync");
        activity?.SetTag("id", id);
        activity?.SetTag("titulo", request.Titulo);
        var vaga = await _vagaRepository.GetByIdAsync(id);
        if (vaga is null)
        {
            return null;
        }

        vaga.Titulo = request.Titulo.Trim();
        vaga.Empresa = request.Empresa.Trim();
        vaga.Requisitos = request.Requisitos.Trim();
        vaga.Salario = request.Salario;
        vaga.TipoContrato = request.TipoContrato.Trim();

        await _vagaRepository.UpdateAsync(vaga);
        _logger.LogInformation("Vaga atualizada: {VagaId}", vaga.Id);
        return MapToResponse(vaga);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("VagaService.DeleteAsync");
        activity?.SetTag("id", id);
        var vaga = await _vagaRepository.GetByIdAsync(id);
        if (vaga is null)
        {
            return false;
        }

        await _vagaRepository.DeleteAsync(vaga);
        _logger.LogInformation("Vaga removida: {VagaId}", vaga.Id);
        return true;
    }

    private static VagaResponse MapToResponse(Vaga vaga)
    {
        return new VagaResponse
        {
            Id = vaga.Id,
            Titulo = vaga.Titulo,
            Empresa = vaga.Empresa,
            Requisitos = vaga.Requisitos,
            Salario = vaga.Salario,
            TipoContrato = vaga.TipoContrato,
            Links = new List<LinkDto>()
        };
    }
}
