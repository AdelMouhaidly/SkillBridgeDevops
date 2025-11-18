using SkillBridge.API.Data;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Interfaces;

namespace SkillBridge.API.Repositories.Implementations;

public class VagaRepository : GenericRepository<Vaga>, IVagaRepository
{
    public VagaRepository(SkillBridgeContext context) : base(context)
    {
    }
}
