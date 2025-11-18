using SkillBridge.API.DTOs;

namespace SkillBridge.API.Repositories.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<(IReadOnlyCollection<TEntity> Items, long TotalCount)> GetPagedAsync(PaginationQuery query);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(Guid id);
}
