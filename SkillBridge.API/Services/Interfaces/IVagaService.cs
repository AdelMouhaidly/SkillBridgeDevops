using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Vagas;
using SkillBridge.API.Models;

namespace SkillBridge.API.Services.Interfaces;

public interface IVagaService
{
    Task<PagedResult<VagaResponse>> GetAsync(PaginationQuery query);
    Task<VagaResponse?> GetByIdAsync(Guid id);
    Task<VagaResponse> CreateAsync(CreateVagaRequest request);
    Task<VagaResponse?> UpdateAsync(Guid id, UpdateVagaRequest request);
    Task<bool> DeleteAsync(Guid id);
}
