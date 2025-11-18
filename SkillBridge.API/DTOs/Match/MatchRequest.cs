using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.DTOs.Match;

public class MatchRequest
{
    [Required]
    public string CompetenciasUsuario { get; set; } = string.Empty;

    [Required]
    public string RequisitosVaga { get; set; } = string.Empty;
}
