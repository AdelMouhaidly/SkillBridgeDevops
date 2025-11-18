using Microsoft.Extensions.DependencyInjection;
using SkillBridge.API.Repositories.Implementations;
using SkillBridge.API.Repositories.Interfaces;
using SkillBridge.API.Services;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IVagaRepository, VagaRepository>();
        services.AddScoped<IAplicacaoRepository, AplicacaoRepository>();

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IVagaService, VagaService>();
        services.AddScoped<IAplicacaoService, AplicacaoService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IHateoasLinkService, HateoasLinkService>();

        return services;
    }
}
