using Microsoft.Extensions.Logging;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(IUsuarioRepository usuarioRepository, ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<PagedResult<UsuarioResponse>> GetAsync(PaginationQuery query)
    {
        var (items, totalCount) = await _usuarioRepository.GetPagedAsync(query);
        var responses = items.Select(MapToResponse).ToList();

        return new PagedResult<UsuarioResponse>
        {
            Items = responses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<UsuarioSummaryResponse>> GetSummariesAsync(PaginationQuery query)
    {
        var (items, totalCount) = await _usuarioRepository.GetPagedWithApplicationsAsync(query);
        var summaries = items.Select(usuario => new UsuarioSummaryResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            TotalAplicacoes = usuario.Aplicacoes.Count,
            MediaPontuacao = usuario.Aplicacoes.Any() ? usuario.Aplicacoes.Average(a => a.PontuacaoCompatibilidade) : 0,
            Links = new List<LinkDto>()
        }).ToList();

        return new PagedResult<UsuarioSummaryResponse>
        {
            Items = summaries,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UsuarioResponse?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        return usuario is null ? null : MapToResponse(usuario);
    }

    public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request)
    {
        var existing = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
        {
            _logger.LogWarning("Tentativa de criação de usuário com email já existente: {Email}", request.Email);
            throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");
        }

        var entity = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Competencias = request.Competencias.Trim(),
            DataCadastro = DateTime.UtcNow
        };

        await _usuarioRepository.AddAsync(entity);
        _logger.LogInformation("Usuário criado com sucesso: {UsuarioId}", entity.Id);
        return MapToResponse(entity);
    }

    public async Task<UsuarioResponse?> UpdateAsync(Guid id, UpdateUsuarioRequest request)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            return null;
        }

        if (!string.Equals(usuario.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _usuarioRepository.GetByEmailAsync(request.Email);
            if (existing is not null && existing.Id != id)
            {
                _logger.LogWarning("Tentativa de atualização para email já utilizado: {Email}", request.Email);
                throw new InvalidOperationException("Já existe um usuário cadastrado com este e-mail.");
            }
        }

        usuario.Nome = request.Nome.Trim();
        usuario.Email = request.Email.Trim().ToLowerInvariant();
        usuario.Competencias = request.Competencias.Trim();

        await _usuarioRepository.UpdateAsync(usuario);
        _logger.LogInformation("Usuário atualizado: {UsuarioId}", usuario.Id);
        return MapToResponse(usuario);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario is null)
        {
            return false;
        }

        await _usuarioRepository.DeleteAsync(usuario);
        _logger.LogInformation("Usuário removido: {UsuarioId}", usuario.Id);
        return true;
    }

    private static UsuarioResponse MapToResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Competencias = usuario.Competencias,
            DataCadastro = usuario.DataCadastro,
            Links = new List<LinkDto>()
        };
    }
}
