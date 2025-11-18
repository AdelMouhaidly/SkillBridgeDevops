using SkillBridge.API.Models;

namespace SkillBridge.API.Repositories.Interfaces;

public interface IAplicacaoRepository : IGenericRepository<Aplicacao>
{
    Task<Aplicacao?> GetByUsuarioEVagaAsync(Guid usuarioId, Guid vagaId);
    Task<IReadOnlyCollection<Aplicacao>> GetByUsuarioAsync(Guid usuarioId);
    Task<IReadOnlyCollection<Aplicacao>> GetByVagaAsync(Guid vagaId);
}
