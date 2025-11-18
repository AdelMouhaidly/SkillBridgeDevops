using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Data;
using SkillBridge.API.DTOs;
using SkillBridge.API.Extensions;
using SkillBridge.API.Repositories.Interfaces;

namespace SkillBridge.API.Repositories.Implementations;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly SkillBridgeContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected GenericRepository(SkillBridgeContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<(IReadOnlyCollection<TEntity> Items, long TotalCount)> GetPagedAsync(PaginationQuery query)
    {
        var baseQuery = DbSet.AsNoTracking();
        baseQuery = baseQuery.ApplyOrdering(query.OrderBy, query.SortDirection);

        var totalCount = await baseQuery.LongCountAsync();
        var items = await baseQuery
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual Task<TEntity?> GetByIdAsync(Guid id) => DbSet.FindAsync(id).AsTask();

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        return entity is not null;
    }
}
