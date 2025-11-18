using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.DTOs.Vagas;

public class CreateVagaRequest
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string Empresa { get; set; } = string.Empty;

    [Required]
    public string Requisitos { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Salario { get; set; }

    [Required]
    [MaxLength(80)]
    public string TipoContrato { get; set; } = string.Empty;
}

