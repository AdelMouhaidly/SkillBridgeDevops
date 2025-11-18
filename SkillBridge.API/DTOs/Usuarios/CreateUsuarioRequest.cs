using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.DTOs.Usuarios;

public class CreateUsuarioRequest
{
    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Competencias { get; set; } = string.Empty;
}

