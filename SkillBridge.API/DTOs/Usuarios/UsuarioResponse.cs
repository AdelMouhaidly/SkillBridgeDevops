namespace SkillBridge.API.DTOs.Usuarios;

public record UsuarioResponse : ResourceResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Competencias { get; init; } = string.Empty;
    public DateTime DataCadastro { get; init; }
}

