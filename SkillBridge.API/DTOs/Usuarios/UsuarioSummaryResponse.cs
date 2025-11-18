namespace SkillBridge.API.DTOs.Usuarios;

public record UsuarioSummaryResponse : ResourceResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public int TotalAplicacoes { get; init; }
    public double MediaPontuacao { get; init; }
}

