using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.Models;

public class Vaga
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Empresa { get; set; } = string.Empty;

    [Required]
    public string Requisitos { get; set; } = string.Empty;

    public decimal Salario { get; set; }

    [Required]
    [MaxLength(80)]
    public string TipoContrato { get; set; } = string.Empty;

    public ICollection<Aplicacao> Aplicacoes { get; set; } = new List<Aplicacao>();
}

