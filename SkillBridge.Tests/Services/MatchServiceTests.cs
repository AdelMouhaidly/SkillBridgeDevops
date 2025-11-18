using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkillBridge.API.DTOs.Match;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Implementations;
using SkillBridge.API.Services;
using SkillBridge.Tests.Common;
using Xunit;

namespace SkillBridge.Tests.Services;

public class MatchServiceTests
{
    private readonly Mock<ILogger<MatchService>> _loggerMock = new();

    [Fact]
    public async Task CalculateScoreAsync_ShouldReturnScoreBetween0And100()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var aplicacaoRepository = new AplicacaoRepository(context);
        var usuarioRepository = new UsuarioRepository(context);
        var vagaRepository = new VagaRepository(context);
        var service = new MatchService(aplicacaoRepository, usuarioRepository, vagaRepository, _loggerMock.Object);
        var request = new MatchRequest
        {
            CompetenciasUsuario = "C#, Azure, SQL",
            RequisitosVaga = "C#, Azure"
        };

        // Act
        var response = await service.CalculateScoreAsync(request);

        // Assert
        response.Pontuacao.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(100);
        response.Descricao.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CalculateScoreForAplicacaoAsync_ShouldUseStoredEntities()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = "User", Email = "user@mail.com", Competencias = "Python, Machine Learning" };
        var vaga = new Vaga { Id = Guid.NewGuid(), Titulo = "Data Scientist", Empresa = "SkillBridge", Requisitos = "Python, Machine Learning", TipoContrato = "CLT" };
        var aplicacao = new Aplicacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            VagaId = vaga.Id,
            DataAplicacao = DateTime.UtcNow
        };

        context.Usuarios.Add(usuario);
        context.Vagas.Add(vaga);
        context.Aplicacoes.Add(aplicacao);
        await context.SaveChangesAsync();

        var aplicacaoRepository = new AplicacaoRepository(context);
        var usuarioRepository = new UsuarioRepository(context);
        var vagaRepository = new VagaRepository(context);
        var service = new MatchService(aplicacaoRepository, usuarioRepository, vagaRepository, _loggerMock.Object);

        // Act
        var response = await service.CalculateScoreForAplicacaoAsync(aplicacao.Id);

        // Assert
        response.Pontuacao.Should().BeGreaterThan(0);
        response.Descricao.Should().ContainEquivalentOf("compatibilidade");
    }
}
