using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SkillBridge.API.Configuration;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            var info = new OpenApiInfo
            {
                Title = "SkillBridge API",
                Version = description.ApiVersion.ToString(),
                Description = "SkillBridge – Plataforma de Requalificação e Empregabilidade com IA"
            };

            if (description.IsDeprecated)
            {
                info.Description += " (versão descontinuada)";
            }

            options.SwaggerDoc(description.GroupName, info);
        }

        options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    }
}
