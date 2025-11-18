namespace SkillBridge.API.DTOs.Vagas;

public record VagaResponse : ResourceResponse
{
    public Guid Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Empresa { get; init; } = string.Empty;
    public string Requisitos { get; init; } = string.Empty;
    public decimal Salario { get; init; }
    public string TipoContrato { get; init; } = string.Empty;
}
