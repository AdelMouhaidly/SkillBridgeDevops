using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.DTOs.Aplicacoes;

public class CreateAplicacaoRequest
{
    [Required]
    public Guid UsuarioId { get; set; }

    [Required]
    public Guid VagaId { get; set; }
}
