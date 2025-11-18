using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Data;
using SkillBridge.API.DTOs;
using SkillBridge.API.Extensions;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;

namespace SkillBridge.API.Repositories.Implementations;

public class AplicacaoRepository : GenericRepository<Aplicacao>, IAplicacaoRepository
{
    public AplicacaoRepository(SkillBridgeContext context) : base(context)
    {
    }

    public override async Task<Aplicacao?> GetByIdAsync(Guid id)
    {
        return await Context.Aplicacoes
            .Include(a => a.Usuario)
            .Include(a => a.Vaga)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public Task<Aplicacao?> GetByUsuarioEVagaAsync(Guid usuarioId, Guid vagaId)
    {
        return Context.Aplicacoes
            .Include(a => a.Usuario)
            .Include(a => a.Vaga)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.VagaId == vagaId);
    }

    public async Task<IReadOnlyCollection<Aplicacao>> GetByUsuarioAsync(Guid usuarioId)
    {
        return await Context.Aplicacoes
            .Include(a => a.Vaga)
            .Include(a => a.Usuario)
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<Aplicacao>> GetByVagaAsync(Guid vagaId)
    {
        return await Context.Aplicacoes
            .Include(a => a.Usuario)
            .Include(a => a.Vaga)
            .AsNoTracking()
            .Where(a => a.VagaId == vagaId)
            .ToListAsync();
    }

    public override async Task<(IReadOnlyCollection<Aplicacao> Items, long TotalCount)> GetPagedAsync(PaginationQuery query)
    {
        var baseQuery = Context.Aplicacoes
            .Include(a => a.Usuario)
            .Include(a => a.Vaga)
            .AsNoTracking()
            .ApplyOrdering(query.OrderBy, query.SortDirection);

        var totalCount = await baseQuery.LongCountAsync();
        var items = await baseQuery
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
