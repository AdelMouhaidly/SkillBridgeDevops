using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.API.Models;

public class Aplicacao
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [Required]
    public Guid VagaId { get; set; }

    [ForeignKey(nameof(VagaId))]
    public Vaga? Vaga { get; set; }

    public DateTime DataAplicacao { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    public double PontuacaoCompatibilidade { get; set; }
}

