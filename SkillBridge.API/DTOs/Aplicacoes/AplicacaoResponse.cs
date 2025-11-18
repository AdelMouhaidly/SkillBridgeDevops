namespace SkillBridge.API.DTOs.Aplicacoes;

public record AplicacaoResponse : ResourceResponse
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public Guid VagaId { get; init; }
    public DateTime DataAplicacao { get; init; }
    public double PontuacaoCompatibilidade { get; init; }
    public string UsuarioNome { get; init; } = string.Empty;
    public string VagaTitulo { get; init; } = string.Empty;
}
