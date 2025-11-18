using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Logging;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Match;
using SkillBridge.API.Repositories.Interfaces;
using SkillBridge.API.Services.Interfaces;
using SkillBridge.API.Logging;

namespace SkillBridge.API.Services;

public class MatchService : IMatchService
{
    private readonly MLContext _mlContext;
    private readonly IAplicacaoRepository _aplicacaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(
        IAplicacaoRepository aplicacaoRepository,
        IUsuarioRepository usuarioRepository,
        IVagaRepository vagaRepository,
        ILogger<MatchService> logger)
    {
        _aplicacaoRepository = aplicacaoRepository;
        _usuarioRepository = usuarioRepository;
        _vagaRepository = vagaRepository;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);
    }

    public Task<MatchResponse> CalculateScoreAsync(MatchRequest request)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("MatchService.CalculateScoreAsync");
        activity?.SetTag("competenciasUsuario.length", request.CompetenciasUsuario?.Length ?? 0);
        activity?.SetTag("requisitosVaga.length", request.RequisitosVaga?.Length ?? 0);
        var score = ComputeScore(request.CompetenciasUsuario, request.RequisitosVaga);
        var description = DescribeScore(score);
        _logger.LogInformation(
            "Pontuação de compatibilidade calculada. Score: {Score}, Usuário: {Competencias}, Vaga: {Requisitos}",
            score,
            request.CompetenciasUsuario,
            request.RequisitosVaga);

        var response = new MatchResponse(score, description)
        {
            Links = new List<LinkDto>()
        };

        return Task.FromResult(response);
    }

    public async Task<MatchResponse> CalculateScoreForAplicacaoAsync(Guid aplicacaoId)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("MatchService.CalculateScoreForAplicacaoAsync");
        activity?.SetTag("aplicacaoId", aplicacaoId);
        var aplicacao = await _aplicacaoRepository.GetByIdAsync(aplicacaoId)
                         ?? throw new KeyNotFoundException("Aplicação não encontrada.");

        var usuario = aplicacao.Usuario ?? await _usuarioRepository.GetByIdAsync(aplicacao.UsuarioId)
                      ?? throw new KeyNotFoundException("Usuário não encontrado para a aplicação.");
        var vaga = aplicacao.Vaga ?? await _vagaRepository.GetByIdAsync(aplicacao.VagaId)
                  ?? throw new KeyNotFoundException("Vaga não encontrada para a aplicação.");

        var score = ComputeScore(usuario.Competencias, vaga.Requisitos);
        var description = DescribeScore(score);

        return new MatchResponse(score, description)
        {
            Links = new List<LinkDto>()
        };
    }

    private double ComputeScore(string competenciasUsuario, string requisitosVaga)
    {
        var cleanedUsuario = competenciasUsuario ?? string.Empty;
        var cleanedVaga = requisitosVaga ?? string.Empty;

        var features = ExtractFeatures(cleanedUsuario, cleanedVaga);
        if (features.UserFeatures.Length == 0 || features.JobFeatures.Length == 0)
        {
            return 0;
        }

        var cosine = CosineSimilarity(features.UserFeatures, features.JobFeatures);
        var lexical = LexicalOverlap(cleanedUsuario, cleanedVaga);

        var blendedScore = (cosine * 0.7 + lexical * 0.3) * 100;
        return Math.Round(Math.Clamp(blendedScore, 0, 100), 2);
    }

    private (float[] UserFeatures, float[] JobFeatures) ExtractFeatures(string usuario, string vaga)
    {
        var data = _mlContext.Data.LoadFromEnumerable(new[]
        {
            new MatchInput { UsuarioCompetencias = usuario, VagaRequisitos = vaga }
        });

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("UserFeatures", nameof(MatchInput.UsuarioCompetencias))
            .Append(_mlContext.Transforms.Text.FeaturizeText("JobFeatures", nameof(MatchInput.VagaRequisitos)));

        var model = pipeline.Fit(data);
        var transformed = model.Transform(data);
        var row = _mlContext.Data.CreateEnumerable<MatchFeatures>(transformed, reuseRowObject: false).First();
        return (row.UserFeatures, row.JobFeatures);
    }

    private static double CosineSimilarity(IReadOnlyList<float> vectorA, IReadOnlyList<float> vectorB)
    {
        if (vectorA.Count != vectorB.Count)
        {
            return 0;
        }

        double dot = 0;
        double magA = 0;
        double magB = 0;

        for (var i = 0; i < vectorA.Count; i++)
        {
            var valA = vectorA[i];
            var valB = vectorB[i];
            dot += valA * valB;
            magA += valA * valA;
            magB += valB * valB;
        }

        if (magA == 0 || magB == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }

    private static double LexicalOverlap(string usuario, string vaga)
    {
        var usuarioTokens = Tokenize(usuario);
        var vagaTokens = Tokenize(vaga);

        if (usuarioTokens.Count == 0 || vagaTokens.Count == 0)
        {
            return 0;
        }

        var intersection = usuarioTokens.Intersect(vagaTokens).Count();
        var union = usuarioTokens.Union(vagaTokens).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static HashSet<string> Tokenize(string text)
    {
        return text.Split(new[] { ',', ';', '/', '\\', '|', '\n', '\r', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 1)
            .ToHashSet();
    }

    private static string DescribeScore(double score)
    {
        return score switch
        {
            >= 85 => "Alta compatibilidade",
            >= 60 => "Boa compatibilidade",
            >= 40 => "Compatibilidade moderada",
            >= 20 => "Compatibilidade baixa",
            _ => "Compatibilidade muito baixa"
        };
    }

    private sealed class MatchInput
    {
        public string UsuarioCompetencias { get; set; } = string.Empty;
        public string VagaRequisitos { get; set; } = string.Empty;
    }

    private sealed class MatchFeatures
    {
        [ColumnName("UserFeatures")]
        public float[] UserFeatures { get; set; } = Array.Empty<float>();

        [ColumnName("JobFeatures")]
        public float[] JobFeatures { get; set; } = Array.Empty<float>();
    }
}
