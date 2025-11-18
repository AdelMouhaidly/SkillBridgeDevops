using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs.Match;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/match")]
public class MatchController : ApiControllerBase
{
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService, IHateoasLinkService hateoasLinkService)
        : base(hateoasLinkService)
    {
        _matchService = matchService;
    }

    [HttpPost(Name = ApiRoutes.Match.Calculate)]
    public async Task<ActionResult<MatchResponse>> CalculateAsync([FromBody] MatchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _matchService.CalculateScoreAsync(request);
        return OkWithLinks(response);
    }

    [HttpGet("aplicacao/{aplicacaoId:guid}", Name = ApiRoutes.Match.CalculateByAplicacao)]
    public async Task<ActionResult<MatchResponse>> CalculateByAplicacaoAsync(Guid aplicacaoId)
    {
        try
        {
            var response = await _matchService.CalculateScoreForAplicacaoAsync(aplicacaoId);
            return OkWithLinks(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
