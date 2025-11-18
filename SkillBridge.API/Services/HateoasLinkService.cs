using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SkillBridge.API.Configuration;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Aplicacoes;
using SkillBridge.API.DTOs.Match;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.DTOs.Vagas;
using SkillBridge.API.Services.Interfaces;

namespace SkillBridge.API.Services;

public class HateoasLinkService : IHateoasLinkService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HateoasLinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public T AddLinks<T>(T resource) where T : ResourceResponse
    {
        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext não disponível para gerar links HATEOAS.");

        ResourceResponse enriched = resource switch
        {
            UsuarioResponse usuario => usuario with { Links = BuildUsuarioLinks(context, usuario.Id) },
            UsuarioSummaryResponse resumo => resumo with { Links = BuildUsuarioSummaryLinks(context, resumo.Id) },
            VagaResponse vaga => vaga with { Links = BuildVagaLinks(context, vaga.Id) },
            AplicacaoResponse aplicacao => aplicacao with { Links = BuildAplicacaoLinks(context, aplicacao.Id, aplicacao.UsuarioId, aplicacao.VagaId) },
            MatchResponse match => match with { Links = BuildMatchLinks(context) },
            _ => resource
        };

        return (T)enriched;
    }

    public PagedResponse<T> AddLinks<T>(PagedResponse<T> pagedResponse) where T : ResourceResponse
    {
        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext não disponível para gerar links HATEOAS.");

        var enrichedItems = pagedResponse.Items.Select(AddLinks).ToList();
        var links = BuildPaginationLinks(context, pagedResponse.PageNumber, pagedResponse.PageSize, pagedResponse.TotalPages, pagedResponse.TotalCount);

        return new PagedResponse<T>
        {
            Items = enrichedItems,
            PageNumber = pagedResponse.PageNumber,
            PageSize = pagedResponse.PageSize,
            TotalCount = pagedResponse.TotalCount,
            TotalPages = pagedResponse.TotalPages,
            Links = links
        };
    }

    private IList<LinkDto> BuildUsuarioLinks(HttpContext context, Guid id)
    {
        return new List<LinkDto>
        {
            CreateLink(context, ApiRoutes.Usuarios.GetById, new { id }, "self", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Usuarios.Update, new { id }, "update", HttpMethods.Put),
            CreateLink(context, ApiRoutes.Usuarios.Delete, new { id }, "delete", HttpMethods.Delete),
            CreateLink(context, ApiRoutes.Aplicacoes.GetByUsuario, new { usuarioId = id }, "applications", HttpMethods.Get)
        };
    }

    private IList<LinkDto> BuildUsuarioSummaryLinks(HttpContext context, Guid id)
    {
        return new List<LinkDto>
        {
            CreateLink(context, ApiRoutes.Usuarios.GetById, new { id }, "details", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Usuarios.Summary, null, "collection", HttpMethods.Get)
        };
    }

    private IList<LinkDto> BuildVagaLinks(HttpContext context, Guid id)
    {
        return new List<LinkDto>
        {
            CreateLink(context, ApiRoutes.Vagas.GetById, new { id }, "self", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Vagas.Update, new { id }, "update", HttpMethods.Put),
            CreateLink(context, ApiRoutes.Vagas.Delete, new { id }, "delete", HttpMethods.Delete),
            CreateLink(context, ApiRoutes.Aplicacoes.GetByVaga, new { vagaId = id }, "applications", HttpMethods.Get)
        };
    }

    private IList<LinkDto> BuildAplicacaoLinks(HttpContext context, Guid id, Guid usuarioId, Guid vagaId)
    {
        return new List<LinkDto>
        {
            CreateLink(context, ApiRoutes.Aplicacoes.GetById, new { id }, "self", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Aplicacoes.Update, new { id }, "update", HttpMethods.Put),
            CreateLink(context, ApiRoutes.Aplicacoes.Delete, new { id }, "delete", HttpMethods.Delete),
            CreateLink(context, ApiRoutes.Usuarios.GetById, new { id = usuarioId }, "usuario", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Vagas.GetById, new { id = vagaId }, "vaga", HttpMethods.Get),
            CreateLink(context, ApiRoutes.Match.CalculateByAplicacao, new { aplicacaoId = id }, "match", HttpMethods.Get)
        };
    }

    private IList<LinkDto> BuildMatchLinks(HttpContext context)
    {
        return new List<LinkDto>
        {
            CreateLink(context, ApiRoutes.Match.Calculate, null, "self", HttpMethods.Post)
        };
    }

    private IList<LinkDto> BuildPaginationLinks(HttpContext context, int pageNumber, int pageSize, int totalPages, long totalCount)
    {
        var links = new List<LinkDto>
        {
            BuildPageLink(context, pageNumber, pageSize, "self")
        };

        if (pageNumber < totalPages)
        {
            links.Add(BuildPageLink(context, pageNumber + 1, pageSize, "next"));
        }

        if (pageNumber > 1)
        {
            links.Add(BuildPageLink(context, pageNumber - 1, pageSize, "prev"));
        }

        return links;
    }

    private LinkDto BuildPageLink(HttpContext context, int pageNumber, int pageSize, string rel)
    {
        var request = context.Request;
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.QueryString.Value ?? string.Empty)
            .ToDictionary(p => p.Key, p => (string?)p.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        query["pageNumber"] = pageNumber.ToString();
        query["pageSize"] = pageSize.ToString();

        var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(string.Empty, query);
        var uri = $"{request.Scheme}://{request.Host}{request.Path}{queryString}";
        return new LinkDto(uri, rel, HttpMethods.Get);
    }

    private LinkDto CreateLink(HttpContext context, string routeName, object? values, string rel, string method)
    {
        var uri = _linkGenerator.GetUriByName(context, routeName, values);
        if (string.IsNullOrEmpty(uri))
        {
            return new LinkDto(string.Empty, rel, method);
        }

        return new LinkDto(uri, rel, method);
    }
}
