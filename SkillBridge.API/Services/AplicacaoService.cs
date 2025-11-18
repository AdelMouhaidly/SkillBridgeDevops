using Microsoft.Extensions.Logging;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Aplicacoes;
using SkillBridge.API.DTOs.Match;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;
using SkillBridge.API.Services.Interfaces;
using SkillBridge.API.Logging;

namespace SkillBridge.API.Services;

public class AplicacaoService : IAplicacaoService
{
    private readonly IAplicacaoRepository _aplicacaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IMatchService _matchService;
    private readonly ILogger<AplicacaoService> _logger;

    public AplicacaoService(
        IAplicacaoRepository aplicacaoRepository,
        IUsuarioRepository usuarioRepository,
        IVagaRepository vagaRepository,
        IMatchService matchService,
        ILogger<AplicacaoService> logger)
    {
        _aplicacaoRepository = aplicacaoRepository;
        _usuarioRepository = usuarioRepository;
        _vagaRepository = vagaRepository;
        _matchService = matchService;
        _logger = logger;
    }

    public async Task<PagedResult<AplicacaoResponse>> GetAsync(PaginationQuery query)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.GetAsync");
        activity?.SetTag("pageNumber", query.PageNumber);
        activity?.SetTag("pageSize", query.PageSize);
        var (items, totalCount) = await _aplicacaoRepository.GetPagedAsync(query);
        var responses = items.Select(MapToResponse).ToList();

        return new PagedResult<AplicacaoResponse>
        {
            Items = responses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AplicacaoResponse?> GetByIdAsync(Guid id)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.GetByIdAsync");
        activity?.SetTag("id", id);
        var aplicacao = await _aplicacaoRepository.GetByIdAsync(id);
        return aplicacao is null ? null : MapToResponse(aplicacao);
    }

    public async Task<IReadOnlyCollection<AplicacaoResponse>> GetByUsuarioAsync(Guid usuarioId)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.GetByUsuarioAsync");
        activity?.SetTag("usuarioId", usuarioId);
        var aplicacoes = await _aplicacaoRepository.GetByUsuarioAsync(usuarioId);
        return aplicacoes.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyCollection<AplicacaoResponse>> GetByVagaAsync(Guid vagaId)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.GetByVagaAsync");
        activity?.SetTag("vagaId", vagaId);
        var aplicacoes = await _aplicacaoRepository.GetByVagaAsync(vagaId);
        return aplicacoes.Select(MapToResponse).ToList();
    }

    public async Task<AplicacaoResponse> CreateAsync(CreateAplicacaoRequest request)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.CreateAsync");
        activity?.SetTag("usuarioId", request.UsuarioId);
        activity?.SetTag("vagaId", request.VagaId);
        var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId)
                      ?? throw new KeyNotFoundException("Usuário não encontrado.");
        var vaga = await _vagaRepository.GetByIdAsync(request.VagaId)
                    ?? throw new KeyNotFoundException("Vaga não encontrada.");

        var existente = await _aplicacaoRepository.GetByUsuarioEVagaAsync(request.UsuarioId, request.VagaId);
        if (existente is not null)
        {
            throw new InvalidOperationException("O usuário já se candidatou a esta vaga.");
        }

        var match = await _matchService.CalculateScoreAsync(new MatchRequest
        {
            CompetenciasUsuario = usuario.Competencias,
            RequisitosVaga = vaga.Requisitos
        });

        var aplicacao = new Aplicacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            VagaId = vaga.Id,
            DataAplicacao = DateTime.UtcNow,
            PontuacaoCompatibilidade = match.Pontuacao
        };

        await _aplicacaoRepository.AddAsync(aplicacao);
        _logger.LogInformation("Aplicação criada: {AplicacaoId}", aplicacao.Id);
        aplicacao.Usuario = usuario;
        aplicacao.Vaga = vaga;
        return MapToResponse(aplicacao);
    }

    public async Task<AplicacaoResponse?> UpdateAsync(Guid id, UpdateAplicacaoRequest request)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.UpdateAsync");
        activity?.SetTag("id", id);
        var aplicacao = await _aplicacaoRepository.GetByIdAsync(id);
        if (aplicacao is null)
        {
            return null;
        }

        aplicacao.PontuacaoCompatibilidade = request.PontuacaoCompatibilidade;
        await _aplicacaoRepository.UpdateAsync(aplicacao);
        _logger.LogInformation("Aplicação atualizada: {AplicacaoId}", aplicacao.Id);
        return MapToResponse(aplicacao);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("AplicacaoService.DeleteAsync");
        activity?.SetTag("id", id);
        var aplicacao = await _aplicacaoRepository.GetByIdAsync(id);
        if (aplicacao is null)
        {
            return false;
        }

        await _aplicacaoRepository.DeleteAsync(aplicacao);
        _logger.LogInformation("Aplicação removida: {AplicacaoId}", aplicacao.Id);
        return true;
    }

    private static AplicacaoResponse MapToResponse(Aplicacao aplicacao)
    {
        return new AplicacaoResponse
        {
            Id = aplicacao.Id,
            UsuarioId = aplicacao.UsuarioId,
            VagaId = aplicacao.VagaId,
            DataAplicacao = aplicacao.DataAplicacao,
            PontuacaoCompatibilidade = aplicacao.PontuacaoCompatibilidade,
            UsuarioNome = aplicacao.Usuario?.Nome ?? string.Empty,
            VagaTitulo = aplicacao.Vaga?.Titulo ?? string.Empty,
            Links = new List<LinkDto>()
        };
    }
}
