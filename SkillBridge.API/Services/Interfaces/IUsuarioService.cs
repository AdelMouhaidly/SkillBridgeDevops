using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.Models;

namespace SkillBridge.API.Services.Interfaces;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponse>> GetAsync(PaginationQuery query);
    Task<PagedResult<UsuarioSummaryResponse>> GetSummariesAsync(PaginationQuery query);
    Task<UsuarioResponse?> GetByIdAsync(Guid id);
    Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request);
    Task<UsuarioResponse?> UpdateAsync(Guid id, UpdateUsuarioRequest request);
    Task<bool> DeleteAsync(Guid id);
}
