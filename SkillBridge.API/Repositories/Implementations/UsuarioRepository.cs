using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Data;
using SkillBridge.API.DTOs;
using SkillBridge.API.Extensions;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;

namespace SkillBridge.API.Repositories.Implementations;

public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(SkillBridgeContext context) : base(context)
    {
    }

    public Task<Usuario?> GetByEmailAsync(string email)
    {
        return Context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(IReadOnlyCollection<Usuario> Items, long TotalCount)> GetPagedWithApplicationsAsync(PaginationQuery query)
    {
        var baseQuery = Context.Usuarios
            .Include(u => u.Aplicacoes)
            .AsNoTracking()
            .ApplyOrdering(query.OrderBy, query.SortDirection);

        var totalCount = await baseQuery.LongCountAsync();
        var items = await baseQuery
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public override async Task<(IReadOnlyCollection<Usuario> Items, long TotalCount)> GetPagedAsync(PaginationQuery query)
    {
        var baseQuery = Context.Usuarios
            .AsNoTracking()
            .ApplyOrdering(query.OrderBy, query.SortDirection);

        var totalCount = await baseQuery.LongCountAsync();
        var items = await baseQuery
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
