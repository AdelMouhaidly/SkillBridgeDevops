using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Aplicacoes;
using SkillBridge.API.Models;

namespace SkillBridge.API.Services.Interfaces;

public interface IAplicacaoService
{
    Task<PagedResult<AplicacaoResponse>> GetAsync(PaginationQuery query);
    Task<AplicacaoResponse?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<AplicacaoResponse>> GetByUsuarioAsync(Guid usuarioId);
    Task<IReadOnlyCollection<AplicacaoResponse>> GetByVagaAsync(Guid vagaId);
    Task<AplicacaoResponse> CreateAsync(CreateAplicacaoRequest request);
    Task<AplicacaoResponse?> UpdateAsync(Guid id, UpdateAplicacaoRequest request);
    Task<bool> DeleteAsync(Guid id);
}
