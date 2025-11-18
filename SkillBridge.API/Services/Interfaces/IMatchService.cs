using SkillBridge.API.DTOs.Match;

namespace SkillBridge.API.Services.Interfaces;

public interface IMatchService
{
    Task<MatchResponse> CalculateScoreAsync(MatchRequest request);
    Task<MatchResponse> CalculateScoreForAplicacaoAsync(Guid aplicacaoId);
}
