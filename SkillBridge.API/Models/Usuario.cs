using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.Models;

public class Usuario
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Competencias { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Aplicacao> Aplicacoes { get; set; } = new List<Aplicacao>();
}

