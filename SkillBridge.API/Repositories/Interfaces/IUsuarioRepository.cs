using SkillBridge.API.DTOs;
using SkillBridge.API.Models;

namespace SkillBridge.API.Repositories.Interfaces;

public interface IUsuarioRepository : IGenericRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<(IReadOnlyCollection<Usuario> Items, long TotalCount)> GetPagedWithApplicationsAsync(PaginationQuery query);
}
