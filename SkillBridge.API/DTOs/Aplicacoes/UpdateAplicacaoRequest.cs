using System.ComponentModel.DataAnnotations;

namespace SkillBridge.API.DTOs.Aplicacoes;

public class UpdateAplicacaoRequest
{
    [Range(0, 100)]
    public double PontuacaoCompatibilidade { get; set; }
}
